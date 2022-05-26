using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using UnityEngine;
using UnityEngine.UIElements;

using Speckle.ConnectorUnity.Converter;

using Application = UnityEngine.Device.Application;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Speckle.ConnectorUnity
{

	[AddComponentMenu("Speckle/Speckle Connector")]
	[ExecuteAlways]
	public class SpeckleConnector : MonoBehaviour
	{

		public const string HostApp = HostApplications.Unity.Name;

		[SerializeField] private List<SpeckleStream> streams = new List<SpeckleStream>();

		[SerializeField] private List<Sender> senders = new List<Sender>();
		[SerializeField] private List<Receiver> receivers = new List<Receiver>();

		[SerializeField] private List<ScriptableSpeckleConverter> converters = new List<ScriptableSpeckleConverter>();

		[SerializeField] private int accountIndex;
		[SerializeField] private int streamIndex;

		private Client client;

		public List<Account> Accounts { get; private set; }

		public List<SpeckleStream> Streams
		{
			get => streams.Valid() ? streams : new List<SpeckleStream>();
		}

		public Account activeAccount
		{
			get => Accounts.Valid(accountIndex) ? Accounts[accountIndex] : null;
		}

		public SpeckleStream activeStream
		{
			get => streams.Valid(streamIndex) ? streams[streamIndex] : null;
		}

		public static string PackagePath => "Packages/com.speckle.connector/";

		private void OnEnable()
		{
			senders ??= new List<Sender>();
			receivers ??= new List<Receiver>();

			Refresh().Forget();
		}

		public event Action onRepaint;

		public void SetStream(int index)
		{
			streamIndex = Streams.Check(index);
		}

		public async UniTask Refresh()
		{
			Accounts = AccountManager.GetAccounts().ToList();
			await SetAccount(accountIndex);
		}

		public async UniTask SetAccount(int index)
		{
			try
			{
				if (Accounts == null)
				{
					SpeckleUnity.Console.Warn("Accounts are not set properly to this connector");
					return;
				}

				streams = new List<SpeckleStream>();

				client = null;
				streamIndex = 0;

				accountIndex = Accounts.Check(index);

				if (activeAccount != null)
				{
					client = new Client(activeAccount);

					var res = await client.StreamsGet();
					streams = new List<SpeckleStream>();

					foreach (var s in res)
					{
						var wrapper = ScriptableObject.CreateInstance<SpeckleStream>();

						if (await wrapper.TrySetNew(s.id, activeAccount.userInfo.id, client.ServerUrl))
						{
							streams.Add(wrapper);
							onRepaint?.Invoke();
						}
					}

				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				onRepaint?.Invoke();
			}
		}

		#if UNITY_EDITOR
		public static List<T> GetAllInstances<T>() where T : ScriptableObject
		{
			var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
			var items = new List<T>();
			foreach (var g in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(g);
				items.Add(AssetDatabase.LoadAssetAtPath<T>(path));
			}
			return items;
		}

		#endif

		public static bool TryGetSpeckleStream(string streamUrl, out SpeckleStream stream)
		{
			stream = ScriptableObject.CreateInstance<SpeckleStream>();
			stream.Init(streamUrl);
			return stream.IsValid();
		}

		public void OpenStreamInBrowser(EventBase obj)
		{
			UniTask.Create(async () =>
			{
				// copied from desktop ui
				await UniTask.Delay(100);

				Application.OpenURL(activeStream.GetUrl(false));
			});
		}

		public void CreateSender(EventBase obj)
		{
			if (activeStream == null)
			{
				SpeckleUnity.Console.Log("No Active stream ready to be sent to sender");
				return;
			}

			UniTask.Create(async () =>
			{
				var mono = new GameObject().AddComponent<Sender>();

				#if UNITY_EDITOR
				Selection.activeObject = mono;
				#endif

				await mono.SetStream(activeStream);
			});
		}

		public void CreateReceiver(EventBase obj)
		{
			if (activeStream == null)
			{
				SpeckleUnity.Console.Log("No Active stream ready to be sent to Receiver");
				return;
			}

			UniTask.Create(async () =>
			{
				var mono = new GameObject().AddComponent<Receiver>();

				#if UNITY_EDITOR
				Selection.activeObject = mono;
				#endif

				await mono.SetStream(activeStream);
			});
		}
	}

}