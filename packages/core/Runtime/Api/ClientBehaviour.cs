using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
using Speckle.ConnectorUnity.Converter;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{
	public abstract class ClientBehaviour<TArgs> : MonoBehaviour,
		ISpeckleOps,
		ISpeckleOpsEvent,
		IShouldValidate
		where TArgs : ClientWorkArgs
	{

		[SerializeField] protected SpeckleObjectBehaviour _root;
		[SerializeField] protected ScriptableSpeckleConverter _converter;

		[SerializeField, HideInInspector] protected StreamAdapter _stream;
		[SerializeField, HideInInspector] protected AccountAdapter _account;
		[SerializeField, HideInInspector] protected SpeckleUnityClient _client;

		[SerializeField, HideInInspector] float _progressAmount;
		[SerializeField, HideInInspector] int _childCountTotal;

		public Account Account => _account?.source;

		public Stream Stream => _stream?.source;

		public Branch Branch => _stream?.branch;

		public List<Branch> Branches => _stream?.branches ?? new List<Branch>();

		public List<Commit> Commits => _stream?.commits ?? new List<Commit>();

		public Commit Commit => _stream?.commit;

		public bool IsWorking { get; protected set; }

		public CancellationToken Token
		{
			get => _client?.token ?? this.GetCancellationTokenOnDestroy();
			set
			{
				if (_client != null)
					_client.token = value;
			}
		}

		public float Progress
		{
			get => _progressAmount;
			protected set => _progressAmount = value;
		}

		public int TotalChildCount
		{
			get => _childCountTotal;
			protected set => _childCountTotal = value;
		}

		protected TArgs Args { get; set; }

		public ISpeckleConverter Converter { get; protected set; }

		public async UniTask Initialize(Account accountToUse, string streamId)
		{
			try
			{
				if (accountToUse == null || !streamId.Valid())
				{
					SpeckleUnity.Console.Warn(
						$"Invalid input during {nameof(Initialize)} for {name}\n"
						+ $"stream :{(streamId.Valid() ? streamId : "invalid")}"
						+ $"account :{(accountToUse != null ? Account.ToString() : "invalid")}");
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

				SpeckleUnity.Console.Log($"{name} is all ready to go! {Stream}");

				name = this.GetType() + $"-{Stream.id}";

				OnClientRefresh?.Invoke();

				await PostLoadStream();
				await PostLoadBranch();
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}
		}

		protected virtual async UniTask PostLoadStream()
		{
			if (!Branches.Valid())
			{
				SpeckleUnity.Console.Log("No Branches on this stream!");
				return;
			}

			await SetBranch("main");
		}

		protected virtual async UniTask PostLoadBranch()
		{
			if (Branch == null)
			{
				SpeckleUnity.Console.Log("No branch set on this stream!");
				return;
			}

			await SetCommit(0);
		}

		protected virtual UniTask PostLoadCommit() => UniTask.CompletedTask;

		protected abstract void SetSubscriptions();

		public void SetDefaultActions(
			UnityAction<ConcurrentDictionary<string, int>> onProgressAction = null,
			UnityAction<string, Exception> onErrorAction = null,
			UnityAction<int> onTotalChildCountAction = null
		)
		{
			OnTotalChildCountAction = onTotalChildCountAction ?? (i => TotalChildCount = i);
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

				                   Progress = total / args.Keys.Count;
			                   });
		}

		public int GetBranchIndex() => Branches.Valid() && Branch != null ? _stream.branches.FindIndex(x => x.name.Equals(Branch.name)) : -1;

		public int GetCommitIndex() => Commits.Valid() && Commit != null ? _stream.commits.FindIndex(x => x.id.Equals(Commit.id)) : -1;

		public async UniTask SetCommit(string commitId)
		{
			if (_stream.CommitSet(commitId))
				await LoadCommit(commitId);
		}

		public async UniTask SetCommit(int commitIndex)
		{
			if (_stream.CommitSet(commitIndex))
				await LoadCommit(_stream.commits[commitIndex].id);
		}

		public async UniTask SetBranch(string branchName)
		{
			if (_stream.BranchSet(branchName))
				await LoadBranch(branchName);
		}

		public async UniTask SetBranch(int branchIndex)
		{
			if (_stream.BranchSet(branchIndex))
				await LoadBranch(_stream.branches[branchIndex].name);
		}

		async UniTask LoadCommit(string commitId)
		{
			await _stream.LoadCommit(_client, commitId);

			OnCommitSet?.Invoke(Commit);

			await PostLoadCommit();
		}

		async UniTask LoadBranch(string branchName)
		{
			await _stream.LoadBranch(_client, branchName);

			OnBranchSet?.Invoke(Branch);

			await PostLoadBranch();
		}

		/// <summary>
		/// Checks if <see cref="Client"/> and <see cref="Stream"/> are both valid 
		/// </summary>
		/// <returns></returns>
		public bool IsValid() => _client != null && _client.IsValid() && _stream != null && _stream.IsValid();

		public string GetUrl() => IsValid() ? _stream.GetUrl(false, _client.account.serverInfo.url) : string.Empty;

		public async UniTask<TArgs> DoWork(ISpeckleConverter converter = null)
		{
			if (converter != null)
			{
				Converter = converter;
			}
			else
			{
				// // TODO: during the build process this should compile and store these objects. 
				if (_converter == null)
				{
					#if UNITY_EDITOR
					_converter = SpeckleUnity.GetDefaultConverter();
					#endif
				}

				Converter = _converter;
			}

			Args = Activator.CreateInstance<TArgs>();

			Progress = 0f;

			if (!IsValid())
			{
				Args.message = "Invalid Client";
				SpeckleUnity.Console.Warn($"{Args.client}-" + Args.message);
			}
			else if (converter == null)
			{
				Args.message = "No active converter found";
				SpeckleUnity.Console.Warn($"{Args.client}-" + Args.message);
			}
			else
			{
				IsWorking = true;
				await Execute();
			}

			OnWorkArgsSet?.Invoke(Args);

			return Args;
		}

		#region inherited event handles

		protected void HandleProgress(ConcurrentDictionary<string, int> args) => OnProgressAction?.Invoke(args);

		protected void HandleError(string message, Exception exception) => OnErrorAction?.Invoke(message, exception);

		protected void HandleChildCount(int args) => UniTask.Create(async () =>
		{
			// Necessary for calling to main thread
			await UniTask.Yield();
			OnTotalChildCountAction?.Invoke(args);
			SpeckleUnity.Console.Log($"Data with {TotalChildCount}");
		});

		protected void HandleRefresh() => OnClientRefresh?.Invoke();

		#endregion

		public event UnityAction<Commit> OnCommitSet;

		public event UnityAction<Branch> OnBranchSet;

		public event UnityAction OnClientRefresh;

		public event UnityAction<ConcurrentDictionary<string, int>> OnProgressAction;

		public event UnityAction<string, Exception> OnErrorAction;

		public event UnityAction<int> OnTotalChildCountAction;

		public event UnityAction<TArgs> OnWorkArgsSet;

		/// <summary>
		///   Clean up to any client things
		/// </summary>
		protected virtual void CleanUp()
		{
			_client?.Dispose();
		}

		#region unity

		protected void OnEnable()
		{
			Token = this.GetCancellationTokenOnDestroy();

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

		protected abstract UniTask Execute();

		#endregion

	}

}