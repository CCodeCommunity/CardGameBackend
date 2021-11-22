using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Services;

public class AccessTokenTrackingService
{
    private static readonly Dictionary<string, DateTime> BlackList = new();

    public async Task BlackListAccessTokensForUserAsync(string accountId)
    {
        if (BlackList.ContainsKey(accountId))
        {
            BlackList[accountId] = DateTime.UtcNow.AddHours(6);   
        }
        else
        {
            BlackList.Add(accountId, DateTime.UtcNow.AddHours(6));
        }
    }

    public async Task<bool> IsTokenBlackListedAsync(string accountId, DateTime issuedAt)
    {
        if (BlackList.ContainsKey(accountId) == false) return false;

        return BlackList[accountId] > issuedAt;
    }
}