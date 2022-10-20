using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Models
{

	/// <summary>
	/// A unity asset object for interacting with a <see cref="StreamAdapter"/>.
	/// Only use this type of object if you would like editor based capabilities with a stream.
	/// If no editor functionality is needed, try using <see cref="StreamAdapter"/> instead.
	/// </summary>
	[CreateAssetMenu(menuName = "Speckle/Speckle Stream", fileName = "SpeckleStream", order = 0)]
	public class ScriptableStream : ScriptableObject, ISpeckleOps
	{

		[SerializeField] StreamAdapter _stream;
		[SerializeField] AccountAdapter _account;
		[SerializeField] SpeckleUnityClient _client;
		[SerializeField] StreamUpdateInputWrapper _update;

		#region binded props

		[SerializeField] string _originalUrlInput;

		[SerializeField] string _serverUrl;

		[SerializeField] string _id;

		[SerializeField] string _commitId;

		[SerializeField] string _branchName;

		[SerializeField] string _objectId;

		#endregion

		CancellationTokenSource _sourceToken;

		public event UnityAction OnClientRefresh;

		public event UnityAction OnUpdate;

		public Account Account
		{
			get => _account?.source;
			private set
			{
				if (value == null)
				{
					SpeckleUnity.Console.Warn($"Invalid Account being passed into {name}");
					return;
				}

				_account = new AccountAdapter(value);
				_serverUrl = value.serverInfo.url;
			}
		}

		public Stream Stream
		{
			get => _stream?.source;
		}

		public string Id
		{
			get => _stream?.id;
		}

		public string Name
		{
			get => _stream?.name;
		}

		public string Description
		{
			get => _stream?.description;
		}

		public bool IsPublic
		{
			get => _stream?.isPublic ?? false;
		}

		public Branch Branch
		{
			get => _stream?.branch;
		}

		public List<Branch> Branches
		{
			get => _stream?.branches;
		}

		public Commit Commit
		{
			get => _stream?.commit;
		}

		public List<Commit> Commits
		{
			get => _stream?.commits;
		}

		public string OriginalUrlInput
		{
			get => _originalUrlInput;
		}

		void OnEnable()
		{
			OnUpdate += UpdateProps;
		}

		void OnDisable()
		{
			OnUpdate -= UpdateProps;
		}

		public async UniTask Initialize(string streamUrl)
		{
			try
			{
				var wrapper = new StreamWrapper(streamUrl);

				_originalUrlInput = streamUrl;

				if (!wrapper.IsValid)
				{
					SpeckleUnity.Console.Warn("Stream url is not valid!" + (streamUrl.Valid() ? $"Input={streamUrl}" : "Invalid Input"));
				}
				else
				{
					Account = await wrapper.GetAccount();

					if (await TryLoadStream(wrapper.StreamId))
					{
						_stream.branch = new Branch { name = wrapper.BranchName };
						switch (wrapper.Type)
						{
							case StreamWrapperType.Undefined:
								SpeckleUnity.Console.Warn("Stream Input type is undefined");
								break;
							case StreamWrapperType.Stream:
								_stream.branches = await _client.BranchesGet(wrapper.StreamId);
								break;
							case StreamWrapperType.Commit:
								_stream.commit = await _client.CommitGet(wrapper.StreamId, wrapper.CommitId);
								break;
							case StreamWrapperType.Branch:
								_stream.branch = await _client.BranchGet(wrapper.StreamId, wrapper.BranchName);
								break;
							case StreamWrapperType.Object:
								_stream.@object = await _client.ObjectGet(wrapper.StreamId, wrapper.ObjectId);
								break;
						}
					}
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}

			finally
			{
				OnUpdate?.Invoke();
			}
		}

		public async UniTask Initialize(Account accountToUse, string streamId)
		{
			try
			{
				Account = accountToUse;
				await TryLoadStream(streamId);
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				OnUpdate?.Invoke();
			}
		}

		async UniTask<bool> TryLoadStream(string streamId)
		{
			if (_sourceToken != null && _sourceToken.Token.CanBeCanceled)
			{
				_sourceToken.Cancel();
				_sourceToken.Dispose();
			}

			_sourceToken = new CancellationTokenSource();

			if (Account == null)
			{
				SpeckleUnity.Console.Warn($"Invalid {nameof(Core.Credentials.Account)}\n" + $"{(Account != null ? Account.ToString() : "invalid")}");
				return false;
			}

			_client = new SpeckleUnityClient(_account.source);

			if (!_client.IsValid())
			{
				SpeckleUnity.Console.Warn($"{name} did not complete {nameof(Initialize)} properly. Seems like the client is invalid");
				return false;
			}

			_client.token = _sourceToken.Token;

			if (!streamId.Valid())
			{
				SpeckleUnity.Console.Warn("Invalid Stream input\n" + $"stream :{(streamId.Valid() ? streamId : "invalid")}");
				return false;
			}

			_stream = new StreamAdapter(await _client.StreamGet(streamId));

			if (!_stream.IsValid())
			{
				SpeckleUnity.Console.Warn($"{name} did not complete {nameof(Initialize)} properly. Seems like the stream is invalid");
				return false;
			}

			return true;
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
				isPublic = this.IsPublic,
				description = this.Description,
				name = this.Name
			});

			await Initialize(accountToUse, resId);

			return _stream != null && _stream.IsValid();
		}

		void UpdateProps()
		{
			_id = Id;
			_objectId = _stream.@object?.id;
			_commitId = Commit?.id;
			_branchName = Branch?.name;
			_serverUrl = Account?.serverInfo.url;
		}

		/// <summary>
		/// Sets new stream details to be applied to this <see cref="Core.Api.Stream"/> 
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
				name = updatedName.Valid() ? updatedName : this.Name,
				description = updatedDescription.Valid() ? updatedDescription : this.Description
			};

			if (!immediatelyUpdate)
				return;

			UniTask.Create(async () =>
			{
				if (await _client.StreamUpdate(_update.Get(this.Id)))
				{
					SpeckleUnity.Console.Log("Updates added");
				}
			});
		}

		

	}

}