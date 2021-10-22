using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading;
using Azure.Core.Serialization;
using System;

namespace iana_win
{
    public class TZ
    {
        public string iana { get; set; }
        public string win { get; set; }
        public string description {get; set;}
    }
    
    public static class TimeZoneConversion
    {
        [Function("timezone-conversion")]
        public static HttpResponseData tz2iana([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("timezone-conversion");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse();
            string tz_win="not found";
            string tz_iana="not found";
            string description = "";

            // 両方設定されていれば、エラーにして返す
            var bindingData = executionContext.BindingContext.BindingData;
            if(bindingData.ContainsKey("iana") && bindingData.ContainsKey("win")) {
                description = "Both query parameters (iana and win) are specified.";
                return CreateResponse(req, tz_iana, tz_win, description, HttpStatusCode.Forbidden);
            }

            // 両方設定されていなければ、エラーにして返す
            if(!bindingData.ContainsKey("iana") && !bindingData.ContainsKey("win")) {
                description = "No query parameter (iana or win) is specified.";
                return CreateResponse(req, tz_iana, tz_win, description, HttpStatusCode.Forbidden);
            }

            // 以下はどちらか一方に設定がある場合
            // ianaが指定されている場合
            if(bindingData.ContainsKey("iana")) {
                tz_iana = bindingData["iana"].ToString();
                if(string.IsNullOrWhiteSpace(tz_iana)) {
                    tz_iana = "not specified";
                    description = "No query parameter for IANA timezone is specified.";
                    return CreateResponse(req, tz_iana, tz_win, description, HttpStatusCode.NotFound);
                }

                // IANA -> Windows
                if(TimeZoneInfo.TryConvertIanaIdToWindowsId(tz_iana, out tz_win)) {
                    description = $"Windows timezone mapped to IANA timezone {tz_iana} is {tz_win}.";
                    return CreateResponse(req, tz_iana, tz_win, description, HttpStatusCode.OK);
                }

                // ここまで到達していれば、指定されたIANA timezoneがWindows timezoneに存在しない
                description = $"Windows timezone mapped to IANA timezone {tz_iana} is not found.";
            }
            // winが指定されている場合
            else if(bindingData.ContainsKey("win")) {
                tz_win = bindingData["win"].ToString();
                if(string.IsNullOrWhiteSpace(tz_win)) {
                    tz_win = "not specified";
                    description = "No query parameter for Windows timezone is specified.";
                    return CreateResponse(req, tz_iana, tz_win, description, HttpStatusCode.NotFound);
                }
                // Windows -> IANA
                if(TimeZoneInfo.TryConvertWindowsIdToIanaId(tz_win, out tz_iana)) {
                    description = $"IANA timezone mapped to Windows timezone {tz_win} is {tz_iana}.";
                    return CreateResponse(req, tz_iana, tz_win, description, HttpStatusCode.OK);
                }

                // ここまで到達していれば、指定されたWindows timezoneがIANA timezoneに存在しない
                description = $"IANA timezone mapped to Windows timezone {tz_win} is not found.";
            }

            // ここまで来るときは、not found
            return CreateResponse(req, tz_iana, tz_win, description, HttpStatusCode.NotFound);
        }

        static HttpResponseData CreateResponse(HttpRequestData _req, string _iana, string _win, string _description, HttpStatusCode _statusCode)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var tz_conversion = new TZ
            {
                iana = _iana,
                win  = _win, 
                description = _description
            };

            HttpResponseData response = _req.CreateResponse();
            response.WriteAsJsonAsync<TZ>(tz_conversion, token);
            response.StatusCode=_statusCode;
            return response;
        }
    }
}
