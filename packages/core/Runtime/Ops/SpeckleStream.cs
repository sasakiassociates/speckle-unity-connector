using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
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

		public MetaData(SpeckleStream obj)
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

	[CreateAssetMenu(menuName = "Speckle/Create Speckle Stream Object", fileName = "SpeckleStream", order = 0)]
	public class SpeckleStream : ScriptableObject
	{

		[SerializeField] string description;

		[SerializeField] string streamName;

		[SerializeField] string serverUrl;

		[SerializeField] string id;

		[SerializeField] string branchName;

		[SerializeField] string commitId;

		[SerializeField] string objectId;

		[SerializeField] string originalInput;

		[SerializeField] List<BranchWrapper> _branches;

		[SerializeField] List<CommitWrapper> _commits;

		StreamWrapper _wrapper;

		public StreamWrapper Wrapper
		{
			get { return _wrapper ??= new StreamWrapper(originalInput); }
		}

		public string Name
		{
			get => streamName;
		}

		public string Description
		{
			get => description;
		}

		public string ServerUrl
		{
			get => serverUrl;
		}

		public string Id
		{
			get => id;
		}

		public string CommitId
		{
			get => commitId;
		}

		public string BranchName
		{
			get => branchName;
		}

		public string ObjectId
		{
			get => objectId;
		}

		public string OriginalInput
		{
			get => originalInput;
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
				originalInput = streamUrlOrId;

			_wrapper = new StreamWrapper(originalInput);

			return Setup();
		}

		public async UniTask<bool> TrySetNew(string streamId, string user, string server)
		{
			_wrapper = new StreamWrapper(streamId, user, server);

			if (!Setup())
			{
				Debug.Log("Setup was not done correctly");
				return false;
			}

			var client = new Client(await _wrapper.GetAccount());

			try
			{
				var stream = await client.StreamGet(_wrapper.StreamId);

				if (stream == null)
					return false;

				streamName = stream.name;
				description = stream.description;

				// TODO: probably a better way to set the most active branch... or just main
				if (!branchName.Valid())
					branchName = stream.branches.items.FirstOrDefault().name;

				_branches = new List<BranchWrapper>();

				foreach (var branch in stream.branches.items)
				{
					var b = new BranchWrapper(branch);
					_branches.Add(b);
				}
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			finally
			{
				client.Dispose();
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

			id = _wrapper.StreamId;
			commitId = _wrapper.CommitId;
			objectId = _wrapper.ObjectId;
			serverUrl = _wrapper.ServerUrl;
			branchName = _wrapper.BranchName;
			originalInput = _wrapper.OriginalInput;

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