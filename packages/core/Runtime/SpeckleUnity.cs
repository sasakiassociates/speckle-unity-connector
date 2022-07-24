using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using UnityEngine;
using Co = Speckle.ConnectorUnity.SpeckleUnity.Console;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Speckle.ConnectorUnity
{
	public static partial class SpeckleUnity
	{
		public const string HostApp = HostApplications.Unity.Name;

		public const string NameSpace = "Speckle";

		#if UNITY_EDITOR
		public static List<T> GetAllInstances<T>() where T : ScriptableObject
		{
			var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
			var items = new List<T>();
			foreach (var g in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(g);
				items.Add(AssetDatabase.LoadAssetAtPath<T>(path));
			}

			return items;
		}
		#endif

		public static bool Check(this UserInfo a, UserInfo b, bool includeId)
		{
			if (a != null && b != null && a.name.Equals(b.name) && a.email.Equals(b.email) && a.company.Equals(b.company))
			{
				return!includeId || a.id.Equals(b.id);
			}

			return false;
		}

		public static Account GetAccountByUserInfo(UserInfo input, bool includeId = false) =>
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
						Co.Log($"Account Found {res.userInfo.name} | {res.serverInfo.name}");
						break;
					}
				}
			}

			catch (SpeckleException e)
			{
				Co.Warn(e.Message);
			}

			return res;
		}

		public static async UniTask<Account> GetAccountByTokenAsync(string input) => await CheckAccountsForAsync(account => account.token.Equals(input));

		public static async UniTask<Account> GetAccountByNameAsync(string input) => await CheckAccountsForAsync(account => account.userInfo.name.Equals(input));

		public static async UniTask<Account> GetAccountByEmailAsync(string input) =>
			await CheckAccountsForAsync(account => account.userInfo.email.Equals(input));

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
						Co.Log($"Account Found {res.userInfo.name} | {res.serverInfo.name}");
						break;
					}
				}
			}

			catch (SpeckleException e)
			{
				Co.Warn(e.Message);
			}

			return res;
		}

		public static async UniTask<Stream> GetStreamByUrlAsync(string input)
		{
			Stream res = null;
			Client c = null;
			try
			{
				var s = new StreamWrapper(input);
				
				var account = await s.GetAccount();
				c = new Client(account);
				res = await c.StreamGet(s.StreamId);
			}

			catch (SpeckleException e)
			{
				Co.Warn(e.Message);
				c?.Dispose();
			}

			return res;
		}

		public static async UniTask<Account> GetAccountByStreamAsync(string input)
		{
			Account res = null;
			try
			{
				res = await new StreamWrapper(input).GetAccount();
				Co.Log($"Account Found {res.userInfo.name} | {res.serverInfo.name}");
			}

			catch (SpeckleException e)
			{
				Co.Warn(e.Message);
			}

			return res;
		}

	}
}