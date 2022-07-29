using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Converter;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{
	public abstract class ClientBehaviour : MonoBehaviour, ISpeckleOps, IOperationEvents, IShouldValidate, IConvert
	{

		[SerializeField] protected SpeckleNode _root;
		[SerializeField] protected ScriptableSpeckleConverter _converter;

		[SerializeField, HideInInspector] protected AccountWrapper _account;
		[SerializeField, HideInInspector] protected SpeckleUnityClient _client;
		[SerializeField, HideInInspector] protected SpeckleStream _stream;

		[SerializeField, HideInInspector] float _progressAmount;
		[SerializeField, HideInInspector] int _childCountTotal;

		public Account account => _account?.source;

		public Stream stream => _stream?.source;

		public Branch branch => _stream?.branch;

		public List<Branch> branches => _stream?.branches ?? new List<Branch>();

		public List<Commit> commits => _stream?.commits ?? new List<Commit>();

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

				_account = new AccountWrapper(accountToUse);

				_client = new SpeckleUnityClient(_account.source);
				_client.token = this.GetCancellationTokenOnDestroy();

				if (!_client.IsValid())
				{
					SpeckleUnity.Console.Warn($"{name} did not complete {nameof(Initialize)} properly. Seems like the client is invalid");
					return;
				}

				_stream = new SpeckleStream(await _client.StreamGet(streamId));

				if (!_stream.IsValid())
				{
					SpeckleUnity.Console.Warn($"{name} did not complete {nameof(Initialize)} properly. Seems like the stream is invalid");
					return;
				}

				SpeckleUnity.Console.Log($"{name} is all ready to go! {stream}");

				name = this.GetType() + $"-{stream.id}";

				OnClientRefresh?.Invoke();

				await PostLoadStream();
				await PostLoadBranch();
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
		}

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

		public void SetBranch(string branchName) => CheckIfValidBranch(_stream.BranchSet(branchName));

		public void SetBranch(int branchIndex) => CheckIfValidBranch(_stream.BranchSet(branchIndex));

		public bool IsValid() => _client != null && _client.IsValid() && _stream != null && _stream.IsValid() && _converter != null;

		#endregion

		#region protected

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

		protected virtual UniTask PostLoadBranch() => UniTask.CompletedTask;

		protected virtual UniTask PostLoadStream() => UniTask.CompletedTask;

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

		#region private

		void CheckIfValidBranch(bool value)
		{
			if (value)
			{
				OnBranchSet?.Invoke(_stream.branch);
				PostLoadBranch().Forget();
			}
		}

		#endregion

		#region unity

		protected void OnEnable()
		{
			// // TODO: during the build process this should compile and store these objects. 
			// #if UNITY_EDITOR
			// _converters = SpeckleUnity.GetAllInstances<ScriptableSpeckleConverter>();
			// #endif

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

		public event UnityAction<Branch> OnBranchSet;

		public event UnityAction OnClientRefresh;

		public event UnityAction<ConcurrentDictionary<string, int>> OnProgressAction;

		public event UnityAction<string, Exception> OnErrorAction;

		public event UnityAction<int> OnTotalChildCountAction;

		#endregion

	}
}