using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	///   A Speckle Sender, it's a wrapper around a basic Speckle Client
	///   that handles conversions for you
	/// </summary>
	[AddComponentMenu(SpeckleUnity.NAMESPACE + "/Sender")]
	[ExecuteAlways]
	public class Sender : ClientBehaviour
	{
		[SerializeField] string _message;

		Base _data;

		/// <summary>
		/// Ignores any speckle node values and sends the loaded <paramref name="@base"/>
		/// </summary>
		/// <param name="base"></param>
		/// <param name="message">Commit message to add</param>
		/// <param name="tokenSource">Cancellation token. Will default to attached source token</param>
		/// <returns></returns>
		public async UniTask<ClientWorkArgs> Run(Base @base, string message = null, CancellationTokenSource tokenSource = null)
		{
			_data = @base;
			return await Run(message, tokenSource);
		}

		/// <summary>
		/// Converts and sends the speckle node to the active stream of this client
		/// </summary>
		/// <param name="obj">Top level object to send</param>
		/// <param name="message">Commit message</param>
		/// <param name="tokenSource">Cancellation token</param>
		/// <returns></returns>
		public async UniTask<ClientWorkArgs> Run(SpeckleNode obj, string message = null, CancellationTokenSource tokenSource = null)
		{
			if (obj != null) _root = obj;

			return await Run(message, tokenSource);
		}

		/// <summary>
		/// Converts all objects attached to the speckle node and sends it to the active stream in this client object
		/// </summary>
		/// <param name="message">Commit message to add</param>
		/// <param name="tokenSource">Cancellation token. Will default to attached source token</param>
		/// <returns></returns>
		public async UniTask<ClientWorkArgs> Run(string message, CancellationTokenSource tokenSource = null)
		{
			token = tokenSource?.Token ?? this.GetCancellationTokenOnDestroy();
			_message = message.Valid() ? message : string.Empty;
			return await Run();
		}

		/// <summary>
		/// A Unity friendly wrapper to send a Base object to the active stream of this client
		/// </summary>
		/// <returns><see cref="SendWorkArgs"/> with values of the url of the commit and the operation result </returns>
		public override async UniTask<ClientWorkArgs> Run()
		{
			var args = new SendWorkArgs()
			{
				message = "",
				url = "",
				client = this,
				success = false
			};

			try
			{
				SpeckleUnity.Console.Log("Send started");

				if (!IsValid())
				{
					args.message = "Invalid Client";
					SpeckleUnity.Console.Warn($"{args.client}-" + args.message);
					return args;
				}

				SpeckleUnity.Console.Log("Converting data");

				if (_root == null && _data == null)
				{
					args.message = $"No objects were found in {nameof(SpeckleNode)} to send. Stopping call";
					SpeckleUnity.Console.Warn(args.message);
					return args;
				}

				_data ??= _root.SceneToData(converter, token);

				if (_data == null)
				{
					args.message = "There is no data in this commit to send. Stopping call";
					SpeckleUnity.Console.Warn(args.message);
					return args;
				}

				var objectId = await SpeckleOps.Send(_client, _data, _stream.id, HandleProgress, HandleError);
				var count = _data.GetTotalChildrenCount();

				var commitId = await _client.CommitCreate(
					new CommitCreateInput
					{
						objectId = objectId,
						streamId = _stream.id,
						branchName = branch.name,
						message = _message.Valid() ? _message : $"Objects from Unity {count}",
						sourceApplication = SpeckleUnity.APP,
						totalChildrenCount = (int)count
					});

				args.success = true;
				args.commitId = commitId;
				args.message = $"Commit sent to {branch}! ({objectId})";
				args.url = SpeckleUnity.GetUrl(false, _client.account.serverInfo.url, _stream.id, StreamWrapperType.Commit, commitId);

				onDataSent?.Invoke(objectId);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				_data = null;

				await UniTask.Yield();
				
				HandleRefresh();
			}

			return args;
		}

		public event UnityAction<string> onDataSent;
	}
}