# Timezone ID converter (Sample)

Sample API using .NET 6 APIs to convert timezone ID between IANA and Windows.
This is the first API I have ever created with C#.

## Prerequisites

- .NET 6
- Azure (this API is hosted on Azure Functions.)
- Azure CLI or PowerShell
- Azure Functions Core tools

## Syntax

~~~
GET https://<azure functions URL>/api/timezone-conversion?{iana|win}=TimeZoneID
~~~

- TimeZone ID is case sensitive (case ignorance is not implemented).

### Query parameter

- iana : IANA timezone ID (e.g. `Asia/Tokyo`)
- win  : Windows timezone ID (e.g. `Tokyo Standard Time`)

### Response example

#### HTTP 200 (OK)

```
GET /api/timezone-conversion?iana=Asia/Tokyo
{
    "iana":"Asia/Tokyo",
    "win":"Tokyo Standard Time",
    "description":"Windows timezone mapped to IANA timezone Asia/Tokyo is Tokyo Standard Time."
}

GET /api/timezone-conversion?win=Tokyo Standard Time
{
    "iana":"Asia/Tokyo",
    "win":"Tokyo Standard Time",
    "description":"IANA timezone mapped to Windows timezone Tokyo Standard Time is Asia/Tokyo."
}
```

### HTTP 403 (Forbidden)

```
GET /api/timezone-conversion
{
    "iana": null,
    "win": null,
    "description": "No query parameter (iana or win) is specified."
}

GET /api/timezone-conversion?win=Tokyo Standard Time&iana=Asia/Tokyo
StatusCode: 403
{
    "iana":"Asia/Tokyo",
    "win":"Tokyo Standard Time",
    "description":"Both query parameters (iana and win) are specified."
}
```

### HTTP 404 (Not found)

```
GET /api/timezone-conversion?win=Asia/Tokyo
{
    "iana": null,
    "win": "Asia/Tokyo",
    "description": "IANA timezone mapped to Windows timezone Asia/Tokyo is not found."
}

GET /api/timezone-conversion?iana=Tokyo Standard Time
{
    "iana": "Tokyo Standard Time",
    "win": null,
    "description": "Windows timezone mapped to IANA timezone Tokyo Standard Time is not found."
}

GET /api/timezone-conversion?win
{
    "iana":null,
    "win":"",
    "description":"No query parameter for Windows timezone is specified."
}

GET /api/timezone-conversion?iana
{
    "iana":null,
    "win":"",
    "description":"No query parameter for IANA timezone is specified."
}
```
