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
    public static class TimeZoneConversion
    {
        [Function("timezone-conversion")]
        public static HttpResponseData tz2iana([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("timezone-conversion");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse();
            string tz_win=null;
            string tz_iana=null;
            TimeZoneData tz = new TimeZoneData();

            // 両方設定されていれば、エラーにして返す
            var bindingData = executionContext.BindingContext.BindingData;
            if(bindingData.ContainsKey("iana") && bindingData.ContainsKey("win")) {
                tz_iana = bindingData["iana"].ToString();
                tz_win = bindingData["win"].ToString();
                if(string.IsNullOrWhiteSpace(tz_iana)) tz.iana = null;
                if(string.IsNullOrWhiteSpace(tz_win)) tz.win = null;
                tz.description = "Both query parameters (iana and win) are specified.";
                return CreateResponse(req, tz, HttpStatusCode.Forbidden);
            }

            // 両方設定されていなければ、エラーにして返す
            if(!bindingData.ContainsKey("iana") && !bindingData.ContainsKey("win")) {
                tz.description = "No query parameter (iana or win) is specified.";
                return CreateResponse(req, tz, HttpStatusCode.Forbidden);
            }

            // 以下はどちらか一方に設定がある場合
            // ianaが指定されている場合
            if(bindingData.ContainsKey("iana")) {
                tz_iana = bindingData["iana"].ToString();
                if(string.IsNullOrWhiteSpace(tz_iana)) {
                    tz.description = "No query parameter for IANA timezone is specified.";
                    return CreateResponse(req, tz, HttpStatusCode.NotFound);
                }

                // IANA -> Windows
                if(TimeZoneInfo.TryConvertIanaIdToWindowsId(tz_iana, out tz_win)) {
                    tz.description = $"Windows timezone mapped to IANA timezone {tz_iana} is {tz_win}.";
                    tz.iana = tz_iana;
                    tz.win = tz_win;
                    return CreateResponse(req, tz, HttpStatusCode.OK);
                }

                // ここまで到達していれば、指定されたIANA timezoneがWindows timezoneに存在しない
                tz.iana = tz_iana;
                tz.description = $"Windows timezone mapped to IANA timezone {tz_iana} is not found.";
            }
            // winが指定されている場合
            else if(bindingData.ContainsKey("win")) {
                tz_win = bindingData["win"].ToString();
                if(string.IsNullOrWhiteSpace(tz_win)) {
                    tz.description = "No query parameter for Windows timezone is specified.";
                    return CreateResponse(req, tz, HttpStatusCode.NotFound);
                }
                // Windows -> IANA
                if(TimeZoneInfo.TryConvertWindowsIdToIanaId(tz_win, out tz_iana)) {
                    tz.win = tz_win;
                    tz.iana = tz_iana;
                    tz.description = $"IANA timezone mapped to Windows timezone {tz_win} is {tz_iana}.";
                    return CreateResponse(req, tz, HttpStatusCode.OK);
                }

                // ここまで到達していれば、指定されたWindows timezoneがIANA timezoneに存在しない
                tz.win = tz_win;
                tz.description = $"IANA timezone mapped to Windows timezone {tz_win} is not found.";
            }

            // ここまで来るときは、not found
            return CreateResponse(req, tz, HttpStatusCode.NotFound);
        }

        static HttpResponseData CreateResponse(HttpRequestData _req, TimeZoneData _tz, HttpStatusCode _statusCode)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            HttpResponseData response = _req.CreateResponse();
            response.WriteAsJsonAsync<TimeZoneData>(_tz, token);
            response.StatusCode=_statusCode;
            return response;
        }
    }
}
