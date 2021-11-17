using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Api.Utilities;

// Based on https://stackoverflow.com/a/36316189
public static class IpUtils
{
    public static string GetRequestIP(this HttpContext context, bool tryUseXForwardHeader = true)
    {
        string? ip = null;

        // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

        // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
        // for 99% of cases however it has been suggested that a better (although tedious)
        // approach might be to read each IP from right to left and use the first public IP.
        // http://stackoverflow.com/a/43554000/538763
        //
        if (tryUseXForwardHeader)
            ip = context.GetHeaderValueAs<string>("X-Forwarded-For").SplitCsv().FirstOrDefault();

        // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
        if (ip.IsNullOrWhitespace() && context.Connection?.RemoteIpAddress != null)
            ip = context.Connection.RemoteIpAddress.ToString();

        if (ip.IsNullOrWhitespace())
            ip = context.GetHeaderValueAs<string>("REMOTE_ADDR");

        if (ip.IsNullOrWhitespace())
            throw new Exception("Unable to determine caller's IP.");

        return ip ?? "";
    }

    public static T? GetHeaderValueAs<T>(this HttpContext context, string headerName)
    {
        var values = new StringValues();
        
        if (!(context.Request?.Headers?.TryGetValue(headerName, out values) ?? false)) 
            return default;
        
        var rawValues = values.ToString(); // Writes out as Csv when there are multiple.

        if (!rawValues.IsNullOrWhitespace())
            return (T) Convert.ChangeType(values.ToString(), typeof(T));

        return default;
    }

    public static IEnumerable<string> SplitCsv(this string? csvList, bool nullOrWhitespaceInputReturnsNull = false)
    {
        if (string.IsNullOrWhiteSpace(csvList))
            return nullOrWhitespaceInputReturnsNull ? Array.Empty<string>() : new List<string>();

        return csvList
            .TrimEnd(',')
            .Split(',')
            .AsEnumerable()
            .Select(s => s.Trim())
            .ToList();
    }

    public static bool IsNullOrWhitespace(this string? s)
    {
        return string.IsNullOrWhiteSpace(s);
    }
}