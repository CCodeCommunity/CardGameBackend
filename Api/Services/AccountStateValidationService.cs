using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Enums;

namespace Api.Services;

public class AccountStateValidationService
{
    private static readonly Dictionary<string, AccountState> accountStatesCache = new();

    private readonly DatabaseContext db;

    public AccountStateValidationService(DatabaseContext db)
    {
        this.db = db;
    }

    public async Task<bool> ValidateAccountStateAsync(string accountId)
    {
        if (accountStatesCache.ContainsKey(accountId))
        {
            return accountStatesCache[accountId] == AccountState.Active;
        }
            
        var account = await db.Accounts.FindAsync(accountId);
        if (account == null) return false;
                
        accountStatesCache.Add(accountId, account.State);
        return account.State == AccountState.Active;
    }

    public async Task InvalidateAccountStateCacheForUser(string accountId)
    {
        accountStatesCache.Remove(accountId);
    }
}