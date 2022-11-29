using Cysharp.Threading.Tasks;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using System;
namespace Speckle.ConnectorUnity
{

  public static class SpeckleAccountManager
  {
    public static Account GetDefaultAccount()
    {
      return AccountManager.GetDefaultAccount();
    }
    
    public static async UniTask<Account> GetAccountByTokenAsync(string input) => await CheckAccountsForAsync(account => account.token.Equals(input));

    public static async UniTask<Account> GetAccountByNameAsync(string input) => await CheckAccountsForAsync(account => account.userInfo.name.Equals(input));

    public static async UniTask<Account> GetAccountByEmailAsync(string input) =>
      await CheckAccountsForAsync(account => account.userInfo.email.Equals(input));

    public static bool Check(this UserInfo a, UserInfo b, bool includeId)
    {
      if (a != null && b != null && a.name.Equals(b.name) && a.email.Equals(b.email) && a.company.Equals(b.company))
      {
        return!includeId || a.id.Equals(b.id);
      }

      return false;
    }

    public static Account GetAccountByUserInfo(this UserInfo input, bool includeId = false) =>
      CheckAccountsFor(account => account.userInfo.Check(input, includeId));

    public static Account GetAccountByToken(string input) => CheckAccountsFor(account => account.token.Equals(input));

    public static Account GetAccountByName(string input) => CheckAccountsFor(account => account.userInfo.name.Equals(input));

    public static Account GetAccountByEmail(string input) => CheckAccountsFor(account => account.userInfo.email.Equals(input));

    static Account CheckAccountsFor(Func<Account, bool> Check)
    {
      Account res = null;
      try
      {
        foreach (var account in AccountManager.GetAccounts())
        {
          if (account != null && Check(account))
          {
            res = account;
            SpeckleUnity.Console.Log($"Account Found {res.userInfo.name} | {res.serverInfo.name}");
            break;
          }
        }
      }

      catch (SpeckleException e)
      {
        SpeckleUnity.Console.Warn(e.Message);
      }

      return res;
    }
    
    static async UniTask<Account> CheckAccountsForAsync(Func<Account, bool> Check)
    {
      Account res = null;
      try
      {
        await AccountManager.UpdateAccounts();
        foreach (var account in AccountManager.GetAccounts())
        {
          if (account != null && Check(account))
          {
            res = account;
            SpeckleUnity.Console.Log($"Account Found {res.userInfo.name} | {res.serverInfo.name}");
            break;
          }
        }
      }

      catch (SpeckleException e)
      {
        SpeckleUnity.Console.Warn(e.Message);
      }

      return res;
    }

  }
}
