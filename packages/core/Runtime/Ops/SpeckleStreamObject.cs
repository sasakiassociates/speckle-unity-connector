using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine;
using UnityEngine.Networking;

namespace Speckle.ConnectorUnity.Ops
{

	[CreateAssetMenu(menuName = "Speckle/Speckle Stream", fileName = "SpeckleStream", order = 0)]
	public class SpeckleStreamObject : ScriptableObject
	{

		[SerializeField] string _originalInput;

		[SerializeField, HideInInspector] SpeckleStream _stream;

		[SerializeField, HideInInspector] int _branchIndex;

		[SerializeField, HideInInspector] int _commitIndex;

		StreamWrapper _wrapper;

		/// <summary>
		/// The name of the stream
		/// </summary>
		public string streamName
		{
			get => _stream.name;
			set => _stream.name = value;
		}

		public string description
		{
			get => _stream.description;
			set => _stream.description = value;
		}

		public string id
		{
			get => _stream.id;
			set => _stream.id = value;
		}

		public List<Branch> branches
		{
			get => _stream.branches;
			set => _stream.branches = value;
		}

		
		public void Load(Stream stream) => Load(new SpeckleStream(stream));

		public void Load(SpeckleStream stream) => _stream = stream;

		
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
			// _branchName = b.name;
			SpeckleUnity.Console.Log($"Setting Active branch to {b}");
			return true;
		}

		public virtual bool TrySetBranch(string input)
		{
			// if (!_branches.Valid())
			// {
			// 	SpeckleUnity.Console.Log($"No Branches set for {name}");
			// 	return false;
			// }
			//
			// for (int i = 0; i < _branches.Count; i++)
			// 	if (_branches[i].name.Equals(input))
			// 	{
			// 		TrySetBranch(i);
			// 		break;
			// 	}

			return true;
		}

		/// <summary>
		///   Initialize a simple stream object that connects the stream wrapper data to the editor
		/// </summary>
		/// <param name="streamUrlOrId">If set to null will use the editor data</param>
		/// <returns></returns>
		public async UniTask Init(string streamUrlOrId = null)
		{
			try
			{
				if (!streamUrlOrId.Valid())
				{
					var s = await SpeckleUnityClient.LoadStreamByUrl(streamUrlOrId);
					if (s != null)
						Load(s);
				}
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
		}

		public async UniTask<bool> TryLoadBranches()
		{
			// Client client = null;
			//
			// try
			// {
			// 	client = new Client(await _wrapper.GetAccount());
			//
			// 	var b = await client.StreamGetBranches(_wrapper.StreamId);
			//
			// 	if (!b.Valid())
			// 		return false;
			//
			// 	_branches = new List<BranchWrapper>();
			// 	foreach (var br in b)
			// 	{
			// 		_branches.Add(new BranchWrapper(br));
			// 	}
			// }
			// catch (Exception e)
			// {
			// 	Console.WriteLine(e);
			// 	throw;
			// }
			// finally
			// {
			// 	client?.Dispose();
			// }

			return true;
		}

		public async UniTask<bool> TryLoadStream()
		{
			// Client client = null;
			//
			// try
			// {
			// 	client = new Client(await _wrapper.GetAccount());
			// 	var stream = await client.StreamGet(_wrapper.StreamId);
			//
			// 	if (stream == null)
			// 		return false;
			//
			// 	var o = stream.@object;
			// 	_streamName = stream.name;
			// 	_description = stream.description;
			// }
			// catch (Exception e)
			// {
			// 	Console.WriteLine(e);
			// 	return false;
			// }
			// finally
			// {
			// 	client?.Dispose();
			// }

			return true;
		}

		public async UniTask<bool> TrySetNew(string streamId, string user, string server)
		{
			// _wrapper = new StreamWrapper(streamId, user, server);
			//
			// if (!Setup())
			// {
			// 	Debug.Log("Setup was not done correctly");
			// 	return false;
			// }
			//
			// if (await TryLoadStream() && await TryLoadBranches())
			// {
			// 	// TODO: probably a better way to set the most active branch... or just main
			// 	if (!_branchName.Valid())
			// 		_branchName = _branches.FirstOrDefault().name;
			// }

			return true;
		}

		public async UniTask<Account> GetAccount() => AccountManager.GetDefaultAccount();

		// public bool IsValid() => Wrapper.IsValid;
		//
		// public override string ToString() => Wrapper.ToString();

		public async UniTask<Texture2D> GetPreview()
		{
			return null;
			// var url = Speckle(true);}
			//
			// if (!url.Valid())
			// 	return null;
			//
			// var www = await UnityWebRequestTexture.GetTexture(url).SendWebRequest();
			//
			// if (www.result != UnityWebRequest.Result.Success)
			// {
			// 	SpeckleUnity.Console.Warn(www.error);
			// 	return null;
			// }
			//
			// return DownloadHandlerTexture.GetContent(www);
		}
	}
}