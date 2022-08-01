using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
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
	public class Receiver : ClientBehaviour
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

		/// <summary>
		///  Gets and converts the data of the last commit on the Stream
		/// </summary>
		/// <returns></returns>
		public override async UniTask<ClientWorkArgs> Run()
		{
			var args = new ReceiveWorkArgs()
			{
				message = "",
				referenceObj = "",
				client = this,
				success = false
			};

			// might have to check for other objects
			token = this.GetCancellationTokenOnDestroy();

			try
			{
				progress = 0f;
				isWorking = true;

				SpeckleUnity.Console.Log("Receive Started");

				if (!IsValid())
				{
					args.message = "Invalid Client";
					SpeckleUnity.Console.Warn($"{args.client}-" + args.message);
					return args;
				}

				if (converter == null)
				{
					args.message = "No active converter found";
					SpeckleUnity.Console.Warn($"{args.client}-" + args.message);
					await UniTask.Yield();
					return args;
				}

				var referenceObj = await GetDataFromStream();

				if (!referenceObj.Valid())
				{
					args.message = "The reference object pulled down from this stream is not valid";
					SpeckleUnity.Console.Warn($"{args.client}-" + args.message);
					return args;
				}

				Base @base = await SpeckleOps.Receive(_client, _stream.id, referenceObj, HandleProgress, HandleError, HandleChildCount);

				if (@base == null)
				{
					args.message = "Data from Commit was not valid";
					SpeckleUnity.Console.Warn($"{args.client}-" + args.message);
					return args;
				}

				args.referenceObj = referenceObj;

				// TODO: handle the process for update objects and not just force deleting
				if (_deleteOld && _root != null)
					SpeckleUnity.SafeDestroy(_root.gameObject);

				_root = new GameObject().AddComponent<SpeckleNode>();

				// TODO: Handle separating the operation call from the conversion
				await _root.DataToScene(@base, converter, token);

				args.success = true;
				args.message = $"Completed {nameof(Run)}";
				OnNodeComplete?.Invoke(_root);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				progress = 0f;
				isWorking = false;

				await UniTask.Yield();

				HandleRefresh();
			}

			return args;
		}

		protected override void SetSubscriptions()
		{
			if (_client != null && _client.IsValid())
			{
				_client.source.SubscribeCommitCreated(stream.id);
				_client.source.OnCommitCreated += (_, c) => OnCommitCreated?.Invoke(c);
				_client.source.SubscribeCommitUpdated(stream.id);
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

			_preview = await SpeckleUnity.GetPreview(_stream.GetUrl(true, account.serverInfo.url));

			OnPreviewSet?.Invoke();

			await UniTask.Yield();
		}

		async UniTask<string> GetDataFromStream()
		{
			var referenceObj = string.Empty;

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

					referenceObj = c.referencedObject;
					break;
				case StreamWrapperType.Object:
					var obj = await _client.ObjectGet(_stream.id, _stream.@object.id);
					referenceObj = obj.id;
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

			return referenceObj;
		}

		#region Events

		public event UnityAction<SpeckleNode> OnNodeComplete;

		public event UnityAction OnPreviewSet;

		#endregion

		#region Subscriptions

		public event UnityAction<CommitInfo> OnCommitCreated;

		public event UnityAction<CommitInfo> OnCommitUpdated;

		#endregion

	}
}