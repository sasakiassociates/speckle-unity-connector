using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public struct MetaData
	{
		public readonly string branchName;

		public readonly string commitId;

		public readonly string description;

		public readonly string id;

		public readonly string objectId;

		public readonly string serverUrl;

		public readonly string streamName;

		public MetaData(SpeckleStreamObject obj)
		{
			description = obj.Description;
			streamName = obj.Name;
			serverUrl = obj.ServerUrl;
			id = obj.Id;
			branchName = obj.BranchName;
			commitId = obj.CommitId;
			objectId = obj.ObjectId;
		}
	}

	[CreateAssetMenu(menuName = "Speckle/Speckle Stream", fileName = "SpeckleStream", order = 0)]
	public class SpeckleStreamObject : ScriptableObject
	{

		[SerializeField] string _description;

		[SerializeField] string _streamName;

		[SerializeField] string _serverUrl;

		[SerializeField] string _id;

		[SerializeField] string _branchName;

		[SerializeField] string _commitId;

		[SerializeField] string _objectId;

		[SerializeField] string _originalInput;

		[SerializeField] List<BranchWrapper> _branches;

		[SerializeField] List<CommitWrapper> _commits;

		[SerializeField, HideInInspector] int _branchIndex;

		[SerializeField, HideInInspector] int _commitIndex;

		StreamWrapper _wrapper;

		public StreamWrapper Wrapper
		{
			get { return _wrapper ??= new StreamWrapper(_originalInput); }
		}

		public string Name
		{
			get => _streamName;
		}

		public string Description
		{
			get => _description;
		}

		public string ServerUrl
		{
			get => _serverUrl;
		}

		public string Id
		{
			get => _id;
		}

		public string CommitId
		{
			get => _commitId;
		}

		public string BranchName
		{
			get => _branchName;
		}

		public List<Branch> branches
		{
			get => new List<Branch>();
		}

		public Branch GetBranch(int input)
		{
			return null;
			// return _branches.Valid(input) ? _branches[input].Cast() : null;
		}

		public bool TrySetBranch(int input)
		{
			var b = GetBranch(input);

			if (b == null)
				return false;

			// TODO: fix issue with the branch name not being set correctly from the branch 
			_branchName = b.name;
			SpeckleUnity.Console.Log($"Setting Active branch to {b}");
			return true;
		}

		public virtual bool TrySetBranch(string input)
		{
			if (!_branches.Valid())
			{
				SpeckleUnity.Console.Log($"No Branches set for {name}");
				return false;
			}

			for (int i = 0; i < _branches.Count; i++)
				if (_branches[i].name.Equals(input))
				{
					TrySetBranch(i);
					break;
				}

			return true;
		}

		public string ObjectId
		{
			get => _objectId;
		}

		public string OriginalInput
		{
			get => _originalInput;
		}

		public StreamWrapperType Type
		{
			get => Wrapper.Type;
		}

		public MetaData getMetaData() => new(this);

		/// <summary>
		///   Initialize a simple stream object that connects the stream wrapper data to the editor
		/// </summary>
		/// <param name="streamUrlOrId">If set to null will use the editor data</param>
		/// <returns></returns>
		public bool Init(string streamUrlOrId = null)
		{
			if (!string.IsNullOrEmpty(streamUrlOrId))
				_originalInput = streamUrlOrId;

			_wrapper = new StreamWrapper(_originalInput);

			return Setup();
		}

		public async UniTask<bool> TryLoadBranches()
		{
			Client client = null;

			try
			{
				client = new Client(await _wrapper.GetAccount());

				var b = await client.StreamGetBranches(_wrapper.StreamId);

				if (!b.Valid())
					return false;

				_branches = new List<BranchWrapper>();
				foreach (var br in b)
				{
					_branches.Add(new BranchWrapper(br));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
			finally
			{
				client?.Dispose();
			}

			return true;
		}

		public async UniTask<bool> TryLoadStream()
		{
			Client client = null;

			try
			{
				client = new Client(await _wrapper.GetAccount());
				var stream = await client.StreamGet(_wrapper.StreamId);

				if (stream == null)
					return false;

				var o = stream.@object;
				_streamName = stream.name;
				_description = stream.description;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
			finally
			{
				client?.Dispose();
			}

			return true;
		}

		public async UniTask<bool> TrySetNew(string streamId, string user, string server)
		{
			_wrapper = new StreamWrapper(streamId, user, server);

			if (!Setup())
			{
				Debug.Log("Setup was not done correctly");
				return false;
			}

			if (await TryLoadStream() && await TryLoadBranches())
			{
				// TODO: probably a better way to set the most active branch... or just main
				if (!_branchName.Valid())
					_branchName = _branches.FirstOrDefault().name;
			}

			return _wrapper.IsValid;
		}

		bool Setup()
		{
			if (_wrapper?.IsValid == false)
			{
				SpeckleUnity.Console.Log("Invalid input for stream");
				return false;
			}

			_id = _wrapper.StreamId;
			_commitId = _wrapper.CommitId;
			_objectId = _wrapper.ObjectId;
			_serverUrl = _wrapper.ServerUrl;
			_branchName = _wrapper.BranchName;
			_originalInput = _wrapper.OriginalInput;

			return true;
		}

		public async UniTask<Account> GetAccount() => await Wrapper.GetAccount();

		public bool IsValid() => Wrapper.IsValid;

		public override string ToString() => Wrapper.ToString();

		public string GetUrl(bool isPreview)
		{
			string url;
			var path = isPreview ? "preview" : "streams";
			switch (Type)
			{
				case StreamWrapperType.Undefined:
					SpeckleUnity.Console.Warn($"{name} is not a valid stream, bailing on the preview thing");
					url = null;
					break;
				case StreamWrapperType.Stream:
					url = $"{Wrapper.ServerUrl}/{path}/{Wrapper.StreamId}";
					break;
				case StreamWrapperType.Commit:
					url = $"{Wrapper.ServerUrl}/{path}/{Wrapper.StreamId}/commits/{Wrapper.CommitId}";
					break;
				case StreamWrapperType.Branch:
					url = $"{Wrapper.ServerUrl}/{path}/{Wrapper.StreamId}/branches/{Wrapper.BranchName}";
					break;
				case StreamWrapperType.Object:
					url = $"{Wrapper.ServerUrl}/{path}/{Wrapper.StreamId}/objects/{Wrapper.ObjectId}";
					break;
				default:
					url = null;
					break;
			}

			return url;
		}

		public async UniTask<Texture2D> GetPreview()
		{
			var url = GetUrl(true);

			if (!url.Valid())
				return null;

			var www = await UnityWebRequestTexture.GetTexture(url).SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				SpeckleUnity.Console.Warn(www.error);
				return null;
			}

			return DownloadHandlerTexture.GetContent(www);
		}
	}
}