using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	///   A Speckle Sender, it's a wrapper around a basic Speckle Client
	///   that handles conversions for you
	/// </summary>
	[AddComponentMenu("Speckle/Sender")]
	[ExecuteAlways]
	public class Sender : SpeckleTempClient
	{

		[SerializeField] string commitMessage;

		public UnityAction<string> onDataSent;

		ServerTransport transport;

		/// <summary>
		/// Converts and sends the speckle node to the active stream of this client
		/// </summary>
		/// <param name="obj">Top level object to send</param>
		/// <param name="message">Commit message</param>
		/// <param name="tokenSource">Cancellation token</param>
		/// <returns></returns>
		public async UniTask<string> Send(SpeckleNode obj, string message = null, CancellationTokenSource tokenSource = null)
		{
			if (obj != null)
				_root = obj;

			return await Send(message, tokenSource);
		}

		/// <summary>
		/// Converts all objects attached to the speckle node and sends it to the active stream in this client object
		/// </summary>
		/// <param name="message">Commit message to add</param>
		/// <param name="tokenSource">Cancellation token. Will default to attached source token</param>
		/// <returns></returns>
		public async UniTask<string> Send(string message = null, CancellationTokenSource tokenSource = null)
		{
			if (!IsReady())
			{
				SpeckleUnity.Console.Warn($"{name} is not ready");
				return string.Empty;
			}

			if (_root == null)
			{
				SpeckleUnity.Console.Warn("No objects were found to send! Stopping call");
				return string.Empty;
			}

			token = tokenSource?.Token ?? this.GetCancellationTokenOnDestroy();

			var data = _root.SceneToData(converter, token);

			return await Send(data, message);
		}

		/// <summary>
		/// A Unity friendly wrapper to send a Base object to the active stream of this client
		/// </summary>
		/// <param name="data">Top level object to send</param>
		/// <param name="message">Commit Message</param>
		/// <param name="tokenSource">Cancellation token</param>
		/// <returns></returns>
		public async UniTask<string> Send(Base data, string message = null, CancellationTokenSource tokenSource = null)
		{
			var objectId = "";
			token = tokenSource?.Token ?? this.GetCancellationTokenOnDestroy();
			const string temp_stream_id = "9b692137ca";
			const string temp_branch_name = "viewstudy/all-targets";
			try
			{
				SpeckleUnity.Console.Log("Sending data");

				transport = new ServerTransport(client.Account, temp_stream_id);

				objectId = await Operations.Send(data, token, new List<ITransport> { transport }, true, SetProgress, SetError);

				Debug.Log($"Commit sent to {branch}! ({objectId})");

				var commitId = await client.CommitCreate(
					token,
					new CommitCreateInput
					{
						objectId = objectId,
						streamId = temp_stream_id,
						branchName = temp_branch_name,
						message = message.Valid() ? message : commitMessage.Valid() ? commitMessage : $"Objects from Unity {data.totalChildrenCount}",
						sourceApplication = SpeckleUnity.HostApp,
						totalChildrenCount = (int)data.GetTotalChildrenCount()
					});

				//TODO: point to new commit 
				Debug.Log($"commit created! {commitId}");

				onDataSent?.Invoke(objectId);

				await UniTask.Yield();
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Exception(e);
			}
			finally
			{
				transport?.Dispose();
			}

			return objectId;
		}

		protected override void CleanUp()
		{
			base.CleanUp();
			transport?.Dispose();
		}

	}
}