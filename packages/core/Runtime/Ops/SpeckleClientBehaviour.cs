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
	public abstract class SpeckleClientBehaviour : MonoBehaviour, ISpeckleOps, IOperationEvents
	{

		[SerializeField] SpeckleNode _root;
		[SerializeField] StreamWrapper _stream;
		[SerializeField] SpeckleUnityClient _client;
		[SerializeField] List<ScriptableSpeckleConverter> _converters;
		[SerializeField] float _progressAmount;

		public float progress
		{
			get => _progressAmount;
			protected set => _progressAmount = value;
		}

		public SpeckleUnityClient client => _client;

		public CancellationToken token { get; set; }

		public int totalChildCount { get; protected set; }

		public void Init(Account account)
		{
			if (account == null)
			{
				SpeckleUnity.Console.Warn("Invalid Account being used to Create this stream");
				return;
			}

			_client = new SpeckleUnityClient(account);
		}

		public void LoadStream(Stream stream)
		{
			if (_client == null || !_client.IsValid())
				return;
		}

		public Stream GetStream() => _stream?.source;

		#region unity methods

		protected void OnEnable()
		{
			// TODO: during the build process this should compile and store these objects. 
			#if UNITY_EDITOR
			_converters = SpeckleUnity.GetAllInstances<ScriptableSpeckleConverter>();
			#endif

			token = this.GetCancellationTokenOnDestroy();

			SetDefaultActions();
			SetSubscriptions();
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

		protected virtual void SetSubscriptions()
		{ }

		#endregion

		#region events

		public event UnityAction<ConcurrentDictionary<string, int>> OnProgressAction;

		public event UnityAction<string, Exception> OnErrorAction;

		public event UnityAction<int> OnTotalChildCountAction;

		#endregion

	}
}