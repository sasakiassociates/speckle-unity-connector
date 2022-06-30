using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Api.SubscriptionModels;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Transports;
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
	public class Receiver : SpeckleClient
	{
		[SerializeField] bool _autoReceive;

		[SerializeField] bool _deleteOld = true;

		[SerializeField] Texture _preview;

		[SerializeField] int _commitIndex;

		[SerializeField] bool _showPreview = true;

		[SerializeField] bool _renderPreview = true;

		public Texture Preview
		{
			get => _preview;
		}

		public List<Commit> Commits
		{
			get;
			protected set;
		}

		public Commit activeCommit
		{
			get => Commits.Valid(_commitIndex) ? Commits[_commitIndex] : null;
		}

		public bool ShowPreview
		{
			get => _showPreview;
			set => _showPreview = value;
		}
		
		void OnDestroy()
		{
			client?.CommitCreatedSubscription?.Dispose();
		}

		public override void SetBranch(int i)
		{
			base.SetBranch(i);
			Commits = branch != null ? branch.commits.items : new List<Commit>();
			SetCommit(0);
		}

		public void SetCommit(int i)
		{
			_commitIndex = Commits.Check(i);

			if (activeCommit != null)
			{
				stream.Init($"{client.ServerUrl}/streams/{stream.Id}/commits/{activeCommit.id}");

				SpeckleUnity.Console.Log("Active commit loaded! " + activeCommit);

				UpdatePreview().Forget();
			}
		}

		async UniTask UpdatePreview()
		{
			if (stream == null || !stream.IsValid())
				await UniTask.Yield();

			_preview = await stream.GetPreview();

			OnPreviewSet?.Invoke();

			await UniTask.Yield();
		}

		protected override void SetSubscriptions()
		{
			if (client != null && _autoReceive)
			{
				client.SubscribeCommitCreated(stream.Id);
				client.OnCommitCreated += (_, c) => OnCommitCreated?.Invoke(c);
				client.SubscribeCommitUpdated(stream.Id);
				client.OnCommitUpdated += (_, c) => OnCommitUpdated?.Invoke(c);
			}
		}

		protected override async UniTask LoadStream()
		{
			await base.LoadStream();

			if (branches != null)
				Commits = branches.FirstOrDefault().commits.items;

			name = nameof(Receiver) + $"-{stream.Id}";
		}

		/// <summary>
		///   Gets and converts the data of the last commit on the Stream
		/// </summary>
		/// <returns></returns>
		public async UniTask Receive(bool sendReceive = false)
		{
			progress = 0f;
			isWorking = true;
			token = this.GetCancellationTokenOnDestroy();
			
			try
			{
				SpeckleUnity.Console.Log("Receive Started");

				if (converter == null)
				{
					SpeckleUnity.Console.Warn("No active converter found!");
					await UniTask.Yield();
					return;
				}

				var @base = await GetCommitData();

				if (@base == null)
				{
					SpeckleUnity.Console.Warn("Data from Commit was not valid");
					return;
				}

				SpeckleUnity.Console.Log($"Data with {@base.totalChildrenCount}");

				// TODO: check if this getting the commit updates the instance
				if (sendReceive)
					this.CommitReceived().Forget();

				CheckRoot();

				await _root.DataToScene(@base, converter, token);

				Debug.Log("Conversion complete");

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
			}
		}

		async UniTask<Base> GetCommitData()
		{
			if (stream == null || !stream.IsValid())
			{
				SpeckleUnity.Console.Log("Stream is not valid");
				return null;
			}

			if (client == null)
			{
				SpeckleUnity.Console.Log("Account is not valid");
				return null;
			}

			Base @base = null;

			var watch = Stopwatch.StartNew();

			var transport = new ServerTransport(client.Account, stream.Id);

			try
			{
				// only use Task with any client calls to speckle. Not worth the conversion 
				await Task.Run(async () =>
				{
					SpeckleUnity.Console.Log($"Getting Commit\nstream id:{stream.Id} commit id:{stream.CommitId}");

					var data = await client.CommitGet(token, stream.Id, stream.CommitId);

					SpeckleUnity.Console.Log($"Commit Fetch:{data.referencedObject}\n{watch.Elapsed}");

					SpeckleUnity.Console.Log($"Now Receiving...\n{watch.Elapsed}");

					@base = await Operations.Receive(objectId: data.referencedObject,
					                                 cancellationToken: token,
					                                 remoteTransport: transport,
					                                 onProgressAction: SetProgress,
					                                 onErrorAction: SetError,
					                                 onTotalChildrenCountKnown: SetChildCount);

					SpeckleUnity.Console.Log($"Object Recieved:{@base}");

					SpeckleUnity.Console.Log("Total time:" + watch.Elapsed);
					
				}, token);
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				// clean up 
				transport.Dispose();

				// report
				watch.Stop();
				SpeckleUnity.Console.Log($"Command Complete\n{watch.Elapsed}");
			}

			return @base;
		}
		void CheckRoot()
		{
			// TODO: handle the process for update objects and not just force deleting
			if (_deleteOld && _root != null)
				SpeckleUnity.SafeDestroy(_root.gameObject);

			SpeckleUnity.Console.Log("Converting Started");

			_root = new GameObject().AddComponent<SpeckleNode>();
		}

		// private async UniTask<ReadOnlyCollection<DisplayMesh>> BufferDisplayMesh(Base @base)
		// {
		// 	var buffer = new List<DisplayMesh>();
		//
		// 	// TODO: get display mesh data
		// 	// TODO: read through any properties for
		//
		// 	return new ReadOnlyCollection<DisplayMesh>(buffer);
		// }

		public void RenderPreview(bool render)
		{
			_renderPreview = render;
			RenderPreview();
		}

		public void RenderPreview()
		{
			Debug.Log($"Render preview? {_renderPreview}");
		}

		public readonly struct DisplayMesh
		{
			public readonly Vector3[] verts;

			public readonly int[] tris;

			public DisplayMesh(Vector3[] verts, int[] tris)
			{
				this.verts = verts;
				this.tris = tris;
			}

		}

		#region Subscriptions

		public event UnityAction<CommitInfo> OnCommitCreated;

		public event UnityAction<CommitInfo> OnCommitUpdated;

		public event UnityAction<SpeckleNode> OnNodeComplete;

		public event UnityAction OnPreviewSet;

		#endregion

	}
}