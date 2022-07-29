using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Api.SubscriptionModels;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	///   A Speckle Receiver, it's a wrapper around a basic Speckle Client
	///   that handles conversions and subscriptions for you
	/// </summary>
	[ExecuteAlways]
	[AddComponentMenu("Speckle/Receiver")]
	public class Receiver : ClientBehaviour
	{
		[SerializeField] bool _autoReceive;
		[SerializeField] bool _sendReceive;
		[SerializeField] bool _deleteOld = true;
		[SerializeField] Texture _preview;
		[SerializeField] bool _showPreview = true;
		[SerializeField] bool _renderPreview = true;

		public Commit commit => _stream?.commit;

		public Texture preview => _preview;

		public bool showPreview
		{
			get => _showPreview;
			set => _showPreview = value;
		}

		public int GetCommitIndex() => commits.Valid() && commit != null ? _stream.commits.FindIndex(x => x.id.Equals(commit.id)) : -1;

		public void SetCommit(string commitId) => CheckIfValidCommit(_stream.CommitSet(commitId));

		public void SetCommit(int commitIndex) => CheckIfValidCommit(_stream.CommitSet(commitIndex));

		void CheckIfValidCommit(bool value)
		{
			if (value)
			{
				OnCommitSet?.Invoke(commit);
				PostLoadCommit().Forget();
			}
		}

		/// <summary>
		///  Gets and converts the data of the last commit on the Stream
		/// </summary>
		/// <returns></returns>
		public async UniTask Receive()
		{
			if (!IsValid())
				return;

			// might have to check for other objects
			token = this.GetCancellationTokenOnDestroy();

			try
			{
				progress = 0f;
				isWorking = true;

				SpeckleUnity.Console.Log("Receive Started");

				if (converter == null)
				{
					SpeckleUnity.Console.Warn("No active converter found!");
					await UniTask.Yield();
					return;
				}

				var referenceObj = await GetDataFromStream();

				if (!referenceObj.Valid())
				{
					SpeckleUnity.Console.Warn("The reference object pulled down from this stream is not valid");
					return;
				}

				Base @base = await SpeckleOps.Receive(_client, _stream.id, referenceObj, HandleProgress, HandleError, HandleChildCount);

				if (@base == null)
				{
					SpeckleUnity.Console.Warn("Data from Commit was not valid");
					return;
				}

				// TODO: handle the process for update objects and not just force deleting
				if (_deleteOld && _root != null)
					SpeckleUnity.SafeDestroy(_root.gameObject);

				_root = new GameObject().AddComponent<SpeckleNode>();
				await _root.DataToScene(@base, converter, token);

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

				HandleRefresh();
				await UniTask.Yield();
			}
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

		protected override async UniTask PostLoadStream()
		{
			if (!branches.Valid())
			{
				SpeckleUnity.Console.Log("No Branches on this stream!");
				return;
			}

			await SetBranch("main");
		}

		protected override UniTask PostLoadBranch()
		{
			if (branch == null)
			{
				SpeckleUnity.Console.Log("No branch set on this stream!");

				return UniTask.CompletedTask;
			}

			SetCommit(0);
			return UniTask.CompletedTask;
		}

		protected virtual UniTask PostLoadCommit()
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
							sourceApplication = SpeckleUnity.HostApp
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

		public event UnityAction<Commit> OnCommitSet;

		public event UnityAction<SpeckleNode> OnNodeComplete;

		public event UnityAction OnPreviewSet;

		#endregion

		#region Subscriptions

		public event UnityAction<CommitInfo> OnCommitCreated;

		public event UnityAction<CommitInfo> OnCommitUpdated;

		#endregion

	}
}