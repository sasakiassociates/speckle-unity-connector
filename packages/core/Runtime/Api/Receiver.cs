using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
using Speckle.ConnectorUnity.Models;
using Speckle.Core.Api;
using Speckle.Core.Api.SubscriptionModels;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	///   A Speckle Receiver, it's a wrapper around a basic Speckle Client
	///   that handles conversions and subscriptions for you
	/// </summary>
	[ExecuteAlways]
	[AddComponentMenu(SpeckleUnity.NAMESPACE + "/Receiver")]
	public class Receiver : ClientBehaviour<ReceiveWorkArgs>
	{
		[SerializeField] ReceiveMode _mode;
		[SerializeField] bool _autoReceive;
		[SerializeField] bool _sendReceive;
		[SerializeField] bool _deleteOld = true;
		[SerializeField] Texture _preview;
		[SerializeField] bool _showPreview = true;
		[SerializeField] bool _renderPreview = true;

		public Texture preview => _preview;

		public bool showPreview
		{
			get => _showPreview;
			set => _showPreview = value;
		}

		protected override async UniTask Execute()
		{
			try
			{
				SpeckleUnity.Console.Log("Receive Started");

				_root = new GameObject().AddComponent<SpeckleObjectBehaviour>();

				// NOTE: this might now need to happen
				switch (_stream.type)
				{
					case StreamWrapperType.Commit:
						var c = await _client.CommitGet(_stream.id, _stream.commit.id);

						// TODO: check if this getting the commit updates the instance
						if (_sendReceive)
							_client.CommitReceived(new CommitReceivedInput()
							{
								streamId = _stream.id,
								commitId = c.id,
								message = "Received Commit from Unity",
								sourceApplication = SpeckleUnity.APP
							}).Forget();

						_root.source = await _client.ObjectGet(_stream.id, c.referencedObject);
						break;
					case StreamWrapperType.Object:
						_root.source = await _client.ObjectGet(_stream.id, _stream.@object.id);
						break;
					case StreamWrapperType.Branch:
					case StreamWrapperType.Stream:
						SpeckleUnity.Console.Warn("A commit or object needs to be set in this stream in order to receive something");
						break;
					case StreamWrapperType.Undefined:
					default:
						SpeckleUnity.Console.Error("Stream is not properly ready to receive");
						break;
				}

				if (!_root.IsValid())
				{
					Args.message = "The reference object pulled down from this stream is not valid";
					SpeckleUnity.Console.Warn($"{Args.client}-" + Args.message);
					return;
				}

				Base @base = await SpeckleOps.Receive(_client, _stream.id, _root.id, HandleProgress, HandleError, HandleChildCount);

				if (@base == null)
				{
					Args.message = "Data from Commit was not valid";
					SpeckleUnity.Console.Warn($"{Args.client}-" + Args.message);
					return;
				}

				Args.referenceObj = _root.id;

				// TODO: handle the process for update objects and not just force deleting
				if (_deleteOld)
					_root.Purge();

				// TODO: Handle separating the operation call from the conversion
				await _root.ConvertToScene(@base, Converter, Token);

				Args.success = true;
				Args.message = $"Completed {nameof(Execute)}";
				OnNodeComplete?.Invoke(_root);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				Progress = 0f;
				IsWorking = false;

				await UniTask.Yield();

				HandleRefresh();
			}
		}

		protected override void SetSubscriptions()
		{
			if (_client != null && _client.IsValid())
			{
				_client.source.SubscribeCommitCreated(Stream.id);
				_client.source.OnCommitCreated += (_, c) => OnCommitCreated?.Invoke(c);
				_client.source.SubscribeCommitUpdated(Stream.id);
				_client.source.OnCommitUpdated += (_, c) => OnCommitUpdated?.Invoke(c);
			}
		}

		protected override UniTask PostLoadCommit()
		{
			UpdatePreview().Forget();
			return UniTask.CompletedTask;
		}

		async UniTask UpdatePreview()
		{
			if (!IsValid())
				await UniTask.Yield();

			_preview = await SpeckleUnity.GetPreview(_stream.GetUrl(true, Account.serverInfo.url));

			OnPreviewSet?.Invoke();

			await UniTask.Yield();
		}

		#region Events

		public event UnityAction<SpeckleObjectBehaviour> OnNodeComplete;

		public event UnityAction OnPreviewSet;

		#endregion

		#region Subscriptions

		public event UnityAction<CommitInfo> OnCommitCreated;

		public event UnityAction<CommitInfo> OnCommitUpdated;

		#endregion

	}
}