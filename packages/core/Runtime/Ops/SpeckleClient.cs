using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Converter;
using Speckle.Core.Api;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{
	public interface IveMadeProgress
	{
		public float progress
		{
			get;
		}
	}

	public interface ISpeckleInstance : ISpeckleStream, ISpeckleClient, IveMadeProgress, ISpeckleProgress
	{
		public UniTask<bool> SetStream(SpeckleStream stream);
	}

	public interface ISpeckleProgress
	{
		public event Action<ConcurrentDictionary<string, int>> OnProgressAction;
		public event Action<string, Exception> OnErrorAction;
		public event UnityAction<int> OnTotalChildCountAction;
	}

	public interface ISpeckleStream
	{
		public SpeckleStream stream
		{
			get;
		}
		public Branch branch
		{
			get;
		}
		public Commit commit
		{
			get;
		}
		public List<Branch> branches
		{
			get;
		}
		public List<Commit> commits
		{
			get;
		}
		public string StreamUrl
		{
			get;
		}
	}

	public interface ISpeckleClient
	{
		public Client client
		{
			get;
		}
		public CancellationToken token
		{
			get;
		}
	}

	// BUG: issue with refreshing object data to editor, probably something with serializing the branch or commit data  
	public abstract class SpeckleClient : MonoBehaviour, ISpeckleInstance, IveMadeProgress
	{

		[SerializeField] protected SpeckleNode _root;

		[SerializeField] protected SpeckleStream _stream;

		[SerializeField] protected List<ScriptableSpeckleConverter> _converters;

		[SerializeField] float _progressAmount;

		[SerializeField] int _branchIndex;

		[SerializeField] int _converterIndex;

		List<Branch> _branches = new();

		/// <summary>
		///   a disposable speckle client that we use to access speckle things
		/// </summary>
		Client _client;

		/// <summary>
		///   an internal toggle to use with uni-task commands
		/// </summary>
		protected bool isCanceled;

		/// <summary>
		///   Event hooked in to Speckle API when calling to the client object
		/// </summary>
		public event Action<string, Exception> OnErrorAction;

		/// <summary>
		///   An unformatted progress report during send or receive calls
		/// </summary>
		public event Action<ConcurrentDictionary<string, int>> OnProgressAction;

		/// <summary>
		///   Event for knowing total child count when a stream is pulled in
		/// </summary>
		public event UnityAction<int> OnTotalChildCountAction;

		public int totalChildCount
		{
			get;
			protected set;
		}

		public bool isWorking
		{
			get;
			protected set;
		}

		/// <summary>
		///   A list of all converters available for this client object
		/// </summary>
		public List<ScriptableSpeckleConverter> converters
		{
			get => _converters.Valid() ? _converters : new List<ScriptableSpeckleConverter>();
		}

		/// <summary>
		///   the active converter for this client object
		/// </summary>
		protected ScriptableSpeckleConverter converter
		{
			get => _converters.Valid(_converterIndex) ? _converters[_converterIndex] : null;
		}

		protected virtual void OnEnable()
		{
			// TODO: during the build process this should compile and store these objects. 
			#if UNITY_EDITOR
			_converters = SpeckleUnity.GetAllInstances<ScriptableSpeckleConverter>();
			#endif

			token = this.GetCancellationTokenOnDestroy();

			SetDefaultActions();

			SetStream(stream).Forget();
		}
		
		protected void SetChildCount(int args)
		{
			// Necessary for calling to main thread
			UniTask.Create(async () =>
			{
				await UniTask.Yield();
				OnTotalChildCountAction?.Invoke(args);
			});

		}


		protected void SetProgress(ConcurrentDictionary<string, int> args)
		{
			OnProgressAction?.Invoke(args);
		}

		protected void SetError(string message, Exception exception)
		{
			OnErrorAction?.Invoke(message, exception);
		}

		protected void SetDefaultActions()
		{
			OnTotalChildCountAction = i => totalChildCount = i;
			OnErrorAction = (message, exception) => SpeckleUnity.Console.Log($"Error From Client:{message}\n{exception.Message}");
			OnProgressAction = args =>
			{
				// from speckle gh connector
				var msg = "";
				var total = 0.0f;
				foreach (var kvp in args)
				{
					msg += $"{kvp.Key}: {kvp.Value}";
					//NOTE: progress set to indeterminate until the TotalChildrenCount is correct
					total += kvp.Value;
				}

				progress = total / args.Keys.Count;
			};
		}

		void OnDisable()
		{
			CleanUp();
		}

		void OnDestroy()
		{
			CleanUp();
		}

		public SpeckleStream stream
		{
			get => _stream;
			protected set => _stream = value;
		}

		public Branch branch
		{
			get => branches.Valid(_branchIndex) ? branches[_branchIndex] : null;
		}

		public Commit commit
		{
			get;
			protected set;
		}

		public List<Branch> branches
		{
			get => _branches.Valid() ? _branches : new List<Branch>();
			protected set => _branches = value;
		}

		public List<Commit> commits
		{
			get;
			protected set;
		}

		public string StreamUrl
		{
			get => stream == null || !stream.IsValid() ? "no stream" : stream.GetUrl(false);

		}

		public Client client
		{
			get;
			protected set;
		}

		public CancellationToken token
		{
			get;
			protected set;
		}

		/// <summary> Necessary setup for interacting with a speckle stream from unity </summary>
		/// <param name="newStream">root stream object to use, will default to editor field</param>
		/// <returns></returns>
		public async UniTask<bool> SetStream(SpeckleStream newStream)
		{
			stream = newStream;
			if (stream == null || !stream.IsValid())
			{
				SpeckleUnity.Console.Log("Speckle stream object is not setup correctly");
				return false;
			}

			await LoadStream();

			SetSubscriptions();

			onRepaint?.Invoke();

			return client != null;
		}

		public float progress
		{
			get => _progressAmount;
			protected set => _progressAmount = value;
		}

		public event Action onRepaint;

		public virtual void SetBranch(int i)
		{
			_branchIndex = branches.Check(i);
		}

		public void SetConverter(int i)
		{
			_converterIndex = _converters.Check(i);
		}

		/// <summary> Necessary setup for interacting with a speckle stream from unity</summary>
		/// <param name="rootStream">root stream object to use, will default to editor field</param>
		/// <param name="onProgressAction">Action to run when there is download/conversion progress</param>
		/// <param name="onErrorAction">Action to run on error</param>
		/// <param name="onTotalChildCountAction">Report for total child count</param>
		public async UniTask<bool> SetStream(
			SpeckleStream rootStream,
			Action<ConcurrentDictionary<string, int>> onProgressAction,
			Action<string, Exception> onErrorAction,
			Action<int> onTotalChildCountAction
		)
		{
			this.OnErrorAction = onErrorAction;
			this.OnProgressAction = onProgressAction;
			this.OnTotalChildCountAction = res => onTotalChildCountAction?.Invoke(res);

			return await SetStream(rootStream);
		}

		protected virtual async UniTask LoadStream()
		{
			var account = await stream.GetAccount();

			client = new Client(account);

			branches = await client.StreamGetBranches(this.GetCancellationTokenOnDestroy(), stream.Id);
		}

		/// <summary>
		///   Internal method for client objects to handle their subscriptions
		/// </summary>
		protected virtual void SetSubscriptions()
		{
			if (client == null) SpeckleUnity.Console.Log($"No active client on {name} to read from");
		}

		/// <summary>
		///   Check if stream and client is active and ready to use
		/// </summary>
		/// <returns></returns>
		protected bool IsReady()
		{
			var res = true;

			if (stream == null || !stream.IsValid())
			{
				SpeckleUnity.Console.Log($"No active stream ready for {name} to use");
				res = false;
			}

			if (client == null)
			{
				SpeckleUnity.Console.Log($"No active client for {name} to use");
				res = false;
			}

			return res;
		}

		/// <summary>
		///   Clean up to any client things
		/// </summary>
		protected virtual void CleanUp()
		{
			client?.Dispose();
		}

	}
}