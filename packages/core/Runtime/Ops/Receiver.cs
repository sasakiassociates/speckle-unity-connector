using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Api.SubscriptionModels;
using Speckle.Core.Logging;
using UnityEngine;
using UnityEngine.Events;

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
		[SerializeField] bool autoReceive;

		[SerializeField] bool deleteOld = true;

		[SerializeField] Texture preview;

		[SerializeField] int commitIndex;

		[SerializeField] bool showPreview = true;

		[SerializeField] bool renderPreview = true;

		public Action<GameObject> onDataReceivedAction;

		public Texture Preview
		{
			get => preview;
		}

		public List<Commit> Commits { get; protected set; }

		public Commit activeCommit
		{
			get => Commits.Valid(commitIndex) ? Commits[commitIndex] : null;
		}

		public bool ShowPreview
		{
			get => showPreview;
			set => showPreview = value;
		}

		void OnDestroy()
		{
			client?.CommitCreatedSubscription?.Dispose();
		}

		public event Action onPreviewSet;

		public override void SetBranch(int i)
		{
			base.SetBranch(i);
			Commits = branch != null ? branch.commits.items : new List<Commit>();
			SetCommit(0);
		}

		public void SetCommit(int i)
		{
			commitIndex = Commits.Check(i);

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

			preview = await stream.GetPreview();

			onPreviewSet?.Invoke();

			await UniTask.Yield();
		}

		protected override void SetSubscriptions()
		{
			if (client != null && autoReceive)
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

			try
			{
				SpeckleUnity.Console.Log("Receive Started");

				if (converter == null)
				{
					SpeckleUnity.Console.Warn("No active converter found!");
					await UniTask.Yield();
					return;
				}

				// get the reference object from the commit
				var @base = await this.GetCommitData();

				if (@base == null)
				{
					SpeckleUnity.Console.Warn("The data pulled from stream was not recieved correctly");
					await UniTask.Yield();
					return;
				}

				// TODO: check if this getting the commit updates the instance
				if (sendReceive)
					this.CommitReceived().Forget();

				// TODO: handle the process for update objects and not just force deleting
				if (deleteOld && _root != null)
					SpeckleUnity.SafeDestroy(_root.gameObject);

				SpeckleUnity.Console.Log("Converting Started");

				_root = new GameObject().AddComponent<SpeckleNode>();

				await _root.DataToScene(@base, converter, token);

				Debug.Log("Conversion complete");

				onDataReceivedAction?.Invoke(_root.gameObject);
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
			renderPreview = render;
			RenderPreview();
		}

		public void RenderPreview()
		{
			Debug.Log($"Render preview? {renderPreview}");
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
		public UnityAction<CommitInfo> OnCommitCreated;

		public UnityAction<CommitInfo> OnCommitUpdated;
		#endregion

	}
}