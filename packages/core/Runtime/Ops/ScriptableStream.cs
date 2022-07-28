using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{

	/// <summary>
	/// A unity asset object for interacting with a <see cref="StreamWrapper"/>.
	/// Only use this type of object if you would like editor based capabilities with a stream.
	/// If no editor functionality is needed, try using <see cref="StreamWrapper"/> instead.
	/// </summary>
	[CreateAssetMenu(menuName = "Speckle/Speckle Stream", fileName = "SpeckleStream", order = 0)]
	public class ScriptableSpeckleStream : ScriptableObject, ISpeckleOps
	{

		[SerializeField, HideInInspector] SpeckleUnityClient _client;
		[SerializeField, HideInInspector] StreamWrapper _stream;
		[SerializeField, HideInInspector] StreamUpdateInputWrapper _update;

		public event Action OnUpdatesAdded;

		public SpeckleUnityClient client
		{
			get => _client;
		}

		public CancellationToken token { get; }

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

		public void Init(Account account)
		{
			if (account == null)
			{
				SpeckleUnity.Console.Warn("Invalid Account being used to Create this stream");
				return;
			}

			_client = new SpeckleUnityClient(account);
		}
		
		/// <summary>
		/// Loads in a new <see cref="Stream"/> for the object to use. This will clear any values stored in this object
		/// </summary>
		/// <param name="stream"></param>
		public void LoadStream(Stream stream)
		{
			if (stream == null) return;

			_update = null;

			_stream = new StreamWrapper(stream);
		}

		public Stream GetStream() => throw new NotImplementedException();

		public async UniTask<bool> Create()
		{
			if (client == null)
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

			LoadStream(await _client.StreamGet(resId));
			_client.Dispose();

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