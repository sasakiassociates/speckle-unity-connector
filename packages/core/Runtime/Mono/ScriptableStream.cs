using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	/// A unity asset object for interacting with a <see cref="SpeckleStream"/>.
	/// Only use this type of object if you would like editor based capabilities with a stream.
	/// If no editor functionality is needed, try using <see cref="SpeckleStream"/> instead.
	/// </summary>
	[CreateAssetMenu(menuName = "Speckle/Speckle Stream", fileName = "SpeckleStream", order = 0)]
	public class ScriptableSpeckleStream : ScriptableObject, ISpeckleOps
	{

		[SerializeField, HideInInspector] SpeckleStream _stream;
		[SerializeField, HideInInspector] AccountWrapper _account;
		[SerializeField, HideInInspector] SpeckleUnityClient _client;
		[SerializeField, HideInInspector] StreamUpdateInputWrapper _update;

		public event Action OnUpdatesAdded;

		public Account account => _account?.source;

		public Stream stream => _stream?.source;

		public CancellationToken token { get; set; }

		public string id
		{
			get => _stream?.id;
		}

		public string streamName
		{
			get => _stream?.name;
		}

		public string description
		{
			get => _stream?.description;
		}

		public bool isPublic
		{
			get => _stream?.isPublic ?? false;
		}

		public Branch branch
		{
			get => _stream?.branch;
		}

		public List<Branch> branches
		{
			get => _stream?.branches;
		}

		public Commit commit
		{
			get => _stream?.commit;
		}

		public List<Commit> commits
		{
			get => _stream?.commits;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountToUse"></param>
		/// <param name="streamId"></param>
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
				name = _stream.name;
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				OnClientRefresh?.Invoke();
			}
		}

		public async UniTask<bool> Create(Account accountToUse)
		{
			if (_client == null)
			{
				SpeckleUnity.Console.Warn("Invalid Account being used to Create this stream");
				return false;
			}

			var resId = await _client.StreamCreate(new StreamCreateInput
			{
				isPublic = this.isPublic,
				description = this.description,
				name = this.streamName
			});

			await Initialize(accountToUse, resId);

			return _stream != null && _stream.IsValid();
		}

		/// <summary>
		/// Sets new stream details to be applied to this <see cref="Stream"/> 
		/// </summary>
		/// <param name="updatedDescription"></param>
		/// <param name="updatedPublic"></param>
		/// <param name="immediatelyUpdate">Will push the changes automatically</param>
		/// <param name="updatedName"></param>
		public void Modify(bool updatedPublic, string updatedName = null, string updatedDescription = null, bool immediatelyUpdate = false)
		{
			if (_stream == null)
			{
				SpeckleUnity.Console.Warn($"{this.name} does not have a valid stream associated with it.");
				return;
			}

			// store any changes being made
			_update = new StreamUpdateInputWrapper
			{
				isPublic = updatedPublic,
				name = updatedName.Valid() ? updatedName : this.streamName,
				description = updatedDescription.Valid() ? updatedDescription : this.description
			};

			if (!immediatelyUpdate)
				return;

			UniTask.Create(async () =>
			{
				if (await _client.StreamUpdate(_update.Get(this.id)))
				{
					OnUpdatesAdded?.Invoke();
				}
			});
		}

		public event UnityAction OnClientRefresh;

		[Serializable]
		internal class StreamUpdateInputWrapper
		{
			public string name;
			public string description;
			public bool isPublic;

			public StreamUpdateInput Get(string id) => new StreamUpdateInput
			{
				id = id,
				name = this.name,
				description = this.description,
				isPublic = this.isPublic
			};
		}

	}
}