using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
using Speckle.ConnectorUnity.Converter;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{
	public abstract class ClientBehaviour : MonoBehaviour, ISpeckleOps, IOperationEvents, IShouldValidate, ICanConverter
	{

		[SerializeField] protected SpeckleNode _root;
		[SerializeField] protected ScriptableSpeckleConverter _converter;

		[SerializeField, HideInInspector] protected StreamAdapter _stream;
		[SerializeField, HideInInspector] protected AccountAdapter _account;
		[SerializeField, HideInInspector] protected SpeckleUnityClient _client;

		[SerializeField, HideInInspector] float _progressAmount;
		[SerializeField, HideInInspector] int _childCountTotal;

		public Account account => _account?.source;

		public Stream stream => _stream?.source;

		public Branch branch => _stream?.branch;

		public List<Branch> branches => _stream?.branches ?? new List<Branch>();

		public List<Commit> commits => _stream?.commits ?? new List<Commit>();

		public Commit commit => _stream?.commit;

		public bool isWorking { get; protected set; }

		public CancellationToken token
		{
			get => _client?.token ?? this.GetCancellationTokenOnDestroy();
			set
			{
				if (_client != null) _client.token = value;
			}
		}

		public float progress
		{
			get => _progressAmount;
			protected set => _progressAmount = value;
		}

		public int totalChildCount
		{
			get => _childCountTotal;
			protected set => _childCountTotal = value;
		}

		public ScriptableSpeckleConverter converter
		{
			get => _converter;
			set => _converter = value;
		}

		#region public

		public async UniTask Initialize(Account accountToUse, string streamId)
		{
			try
			{
				if (accountToUse == null || !streamId.Valid())
				{
					SpeckleUnity.Console.Warn(
						$"Invalid input during {nameof(Initialize)} for {name}\n"
						+ $"stream :{(streamId.Valid() ? streamId : "invalid")}"
						+ $"account :{(accountToUse != null ? account.ToString() : "invalid")}");
					return;
				}

				_account = new AccountAdapter(accountToUse);

				_client = new SpeckleUnityClient(_account.source);
				_client.token = this.GetCancellationTokenOnDestroy();

				if (!_client.IsValid())
				{
					SpeckleUnity.Console.Warn($"{name} did not complete {nameof(Initialize)} properly. Seems like the client is invalid");
					return;
				}

				_stream = new StreamAdapter(await _client.StreamGet(streamId));

				if (!_stream.IsValid())
				{
					SpeckleUnity.Console.Warn($"{name} did not complete {nameof(Initialize)} properly. Seems like the stream is invalid");
					return;
				}

				SpeckleUnity.Console.Log($"{name} is all ready to go! {stream}");

				name = this.GetType() + $"-{stream.id}";

				OnClientRefresh?.Invoke();

				// // TODO: during the build process this should compile and store these objects. 
				if (converter == null)
				{
					#if UNITY_EDITOR
					converter = SpeckleUnity.GetDefaultConverter();
					#endif
				}

				await PostLoadStream();
				await PostLoadBranch();
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
		}

		public abstract UniTask<ClientWorkArgs> Run();

		public void SetDefaultActions(
			UnityAction<ConcurrentDictionary<string, int>> onProgressAction = null,
			UnityAction<string, Exception> onErrorAction = null,
			UnityAction<int> onTotalChildCountAction = null
		)
		{
			OnTotalChildCountAction = onTotalChildCountAction ?? (i => totalChildCount = i);
			OnErrorAction = onErrorAction ?? ((message, exception) => SpeckleUnity.Console.Log($"Error From Client:{message}\n{exception.Message}"));
			OnProgressAction = onProgressAction
			                   ?? (args =>
			                   {
				                   // from speckle gh connector
				                   var total = 0.0f;
				                   foreach (var kvp in args)
				                   {
					                   //NOTE: progress set to indeterminate until the TotalChildrenCount is correct
					                   total += kvp.Value;
				                   }

				                   progress = total / args.Keys.Count;
			                   });
		}

		public int GetBranchIndex() => branches.Valid() && branch != null ? _stream.branches.FindIndex(x => x.name.Equals(branch.name)) : -1;

		public int GetCommitIndex() => commits.Valid() && commit != null ? _stream.commits.FindIndex(x => x.id.Equals(commit.id)) : -1;

		public async UniTask SetCommit(string commitId)
		{
			if (_stream.CommitSet(commitId)) await LoadCommit(commitId);
		}

		public async UniTask SetCommit(int commitIndex)
		{
			if (_stream.CommitSet(commitIndex)) await LoadCommit(_stream.commits[commitIndex].id);
		}

		public async UniTask SetBranch(string branchName)
		{
			if (_stream.BranchSet(branchName)) await LoadBranch(branchName);
		}

		public async UniTask SetBranch(int branchIndex)
		{
			if (_stream.BranchSet(branchIndex)) await LoadBranch(_stream.branches[branchIndex].name);
		}

		async UniTask LoadCommit(string commitId)
		{
			await _stream.LoadCommit(_client, commitId);

			OnCommitSet?.Invoke(commit);

			await PostLoadCommit();
		}

		async UniTask LoadBranch(string branchName)
		{
			await _stream.LoadBranch(_client, branchName);

			OnBranchSet?.Invoke(branch);

			await PostLoadBranch();
		}

		public bool IsValid() => _client != null && _client.IsValid() && _stream != null && _stream.IsValid();

		public string GetUrl() => IsValid() ? _stream.GetUrl(false, _client.account.serverInfo.url) : string.Empty;

		#endregion

		#region protected

		#region inherited event handles

		protected void HandleProgress(ConcurrentDictionary<string, int> args) => OnProgressAction?.Invoke(args);

		protected void HandleError(string message, Exception exception) => OnErrorAction?.Invoke(message, exception);

		protected void HandleChildCount(int args) => UniTask.Create(async () =>
		{
			// Necessary for calling to main thread
			await UniTask.Yield();
			OnTotalChildCountAction?.Invoke(args);
			SpeckleUnity.Console.Log($"Data with {totalChildCount}");
		});

		protected void HandleRefresh() => OnClientRefresh?.Invoke();

		#endregion

		protected virtual async UniTask PostLoadStream()
		{
			if (!branches.Valid())
			{
				SpeckleUnity.Console.Log("No Branches on this stream!");
				return;
			}

			await SetBranch("main");
		}

		protected virtual async UniTask PostLoadBranch()
		{
			if (branch == null)
			{
				SpeckleUnity.Console.Log("No branch set on this stream!");
				return;
			}

			await SetCommit(0);
		}

		protected virtual UniTask PostLoadCommit() => UniTask.CompletedTask;

		protected virtual void SetSubscriptions()
		{ }

		/// <summary>
		///   Clean up to any client things
		/// </summary>
		protected virtual void CleanUp()
		{
			_client?.Dispose();
		}

		#endregion

		#region unity

		protected void OnEnable()
		{
			token = this.GetCancellationTokenOnDestroy();

			SetDefaultActions();
			SetSubscriptions();
		}

		void OnDisable()
		{
			CleanUp();
		}

		void OnDestroy()
		{
			CleanUp();
		}

		#endregion

		#region events

		public event UnityAction<Commit> OnCommitSet;

		public event UnityAction<Branch> OnBranchSet;

		public event UnityAction OnClientRefresh;

		public event UnityAction<ConcurrentDictionary<string, int>> OnProgressAction;

		public event UnityAction<string, Exception> OnErrorAction;

		public event UnityAction<int> OnTotalChildCountAction;

		#endregion

	}
}