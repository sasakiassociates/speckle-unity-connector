using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Converter;
using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
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

		[SerializeField] List<ScriptableSpeckleStream> _streams = new();

		[SerializeField] List<Sender> _senders = new();

		[SerializeField] List<Receiver> _receivers = new();

		[SerializeField] List<ScriptableSpeckleConverter> _converters = new();

		[SerializeField] int _accountIndex;

		[SerializeField] int _streamIndex;

		[SerializeField] int _streamLimit = 20;

		Client _client;

		public List<Account> Accounts { get; private set; }

		public List<ScriptableSpeckleStream> Streams
		{
			get => _streams.Valid() ? _streams : new List<ScriptableSpeckleStream>();
		}

		public Account activeAccount
		{
			get => Accounts.Valid(_accountIndex) ? Accounts[_accountIndex] : null;
		}

		public ScriptableSpeckleStream activeStream
		{
			get => _streams.Valid(_streamIndex) ? _streams[_streamIndex] : null;
		}

		public static string PackagePath
		{
			get => "Packages/com.speckle.connector/";
		}

		void OnEnable()
		{
			_senders ??= new List<Sender>();
			_receivers ??= new List<Receiver>();

			Refresh().Forget();
		}

		public event UnityAction onRepaint;

		public event UnityAction<Receiver> OnReceiverCreated;

		public event UnityAction<Sender> OnSenderCreated;

		public void SetStream(int index)
		{
			Debug.Log($"Setting Stream index to{index} from {_streamIndex}");
			_streamIndex = Streams.Check(index);
		}

		public async UniTask Refresh()
		{
			Accounts = AccountManager.GetAccounts().ToList();
			await SetAccount(_accountIndex);
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

				_streams = new List<ScriptableSpeckleStream>();

				_client = null;
				_streamIndex = 0;

				_accountIndex = Accounts.Check(index);

				if (activeAccount != null)
				{
					_client = new Client(activeAccount);

					var res = await _client.StreamsGet(_streamLimit);
					_streams = new List<ScriptableSpeckleStream>();

					foreach (var s in res)
					{
						var wrapper = ScriptableObject.CreateInstance<ScriptableSpeckleStream>();

						// if (await wrapper.TrySetNew(s.id, activeAccount.userInfo.id, _client.ServerUrl))
						// {
						// 	_streams.Add(wrapper);
						// 	onRepaint?.Invoke();
						// }
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

		public static bool TryGetSpeckleStream(string streamUrl, out ScriptableSpeckleStream stream)
		{
			stream = ScriptableObject.CreateInstance<ScriptableSpeckleStream>();
			// stream.Init(streamUrl);
			return false;
			// return stream.IsValid();
		}

		public void OpenStreamInBrowser(EventBase obj)
		{
			UniTask.Create(async () =>
			{
				// copied from desktop ui
				await UniTask.Delay(100);

				// Application.OpenURL(activeStream.GetUrl(false));
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
				if (activeStream == null)
				{
					SpeckleUnity.Console.Log("No Active stream ready to be sent to Receiver");
					return;
				}

				var mono = new GameObject().AddComponent<Sender>();

				#if UNITY_EDITOR
				Selection.activeObject = mono;
				#endif

				await mono.SetStream(activeStream);

				OnSenderCreated?.Invoke(mono);
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

				// await mono.SetStream(activeStream);

				OnReceiverCreated?.Invoke(mono);
			});
		}
	}

}