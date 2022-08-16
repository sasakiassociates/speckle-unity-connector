using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using UnityEngine;
using UnityEngine.Networking;
using Co = Speckle.ConnectorUnity.SpeckleUnity.Console;
#if UNITY_EDITOR
using Speckle.ConnectorUnity.Converter;
using UnityEditor;
#endif

namespace Speckle.ConnectorUnity
{
	public static partial class SpeckleUnity
	{

		public const string APP = HostApplications.Unity.Name;

		public const string NAMESPACE = "Speckle";

		public static class Categories
		{
			public const string COMPS = NAMESPACE + "/Components/";
			public const string CONVERTERS = NAMESPACE + "/Converters/";
		}

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

		public static ScriptableSpeckleConverter GetDefaultConverter()
		{
			var items = GetAllInstances<ScriptableSpeckleConverter>();
			return items.FirstOrDefault(x => x.Name.ToLower().Equals("converter-unity"));
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

		public static string GetUrl(bool isPreview, string serverUrl, string streamId) => $"{serverUrl}/{(isPreview ? "preview" : "streams")}/{streamId}";

		public static string GetUrl(bool isPreview, string serverUrl, string streamId, StreamWrapperType type, string value)
		{
			string url = $"{serverUrl}/{(isPreview ? "preview" : "streams")}/{streamId}";

			switch (type)
			{
				case StreamWrapperType.Stream:
					return url;
				case StreamWrapperType.Commit:
					url += $"/commits/{value}";
					break;
				case StreamWrapperType.Branch:
					url += $"/branches/{value}";
					break;
				case StreamWrapperType.Object:
					url += $"objects/{value}";
					break;
				case StreamWrapperType.Undefined:
				default:
					Console.Warn($"{streamId} is not a valid stream for server {serverUrl}, bailing on the preview thing");
					url = null;
					break;
			}

			return url;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static async UniTask<Texture2D> GetPreview(string url)
		{
			if (!url.Valid())
				return null;

			var www = await UnityWebRequestTexture.GetTexture(url).SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				SpeckleUnity.Console.Warn(www.error);
				return null;
			}

			return DownloadHandlerTexture.GetContent(www);
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

		public static void LogProgress(ConcurrentDictionary<string, int> args)
		{
			// from speckle gh connector
			var total = 0.0f;
			foreach (var kvp in args)
			{
				//NOTE: progress set to indeterminate until the TotalChildrenCount is correct
				total += kvp.Value;
			}

			var progress = total / args.Keys.Count;
			SpeckleUnity.Console.Log(progress.ToString());
		}

	}

}