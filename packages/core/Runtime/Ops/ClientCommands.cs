using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace Speckle.ConnectorUnity.Ops
{

	public static class StreamCommands
	{
		public static async UniTask<bool> LoadObject(this SpeckleStream stream, ISpeckleUnityClient client, string value)
		{
			if (client.IsValid() && stream != null)
				stream.@object = await client.ObjectGet(stream.id, value);

			return stream.@object != null;
		}

		public static async UniTask<bool> LoadBranches(this SpeckleStream stream, ISpeckleUnityClient client, int branchLimit = 10, int commitLimit = 5)
		{
			if (client.IsValid() && stream != null)
				stream.branches = await client.BranchesGet(stream.id, branchLimit, commitLimit);

			return stream.branches.Valid();
		}

		public static async UniTask<bool> LoadBranch(this SpeckleStream stream, ISpeckleUnityClient client, string input)
		{
			if (client.IsValid() && stream != null)
				stream.branch = await client.BranchGet(stream.id, input);

			return stream.branch != null && stream.branch.name.Equals(input);
		}

		public static async UniTask<bool> LoadCommit(this SpeckleStream stream, ISpeckleUnityClient client, string input)
		{
			if (client.IsValid() && stream != null)
				stream.commit = await client.CommitGet(stream.id, input);

			return stream.commit != null && stream.commit.id.Equals(input);
		}

		public static async UniTask<bool> LoadCommits(this SpeckleStream stream, ISpeckleUnityClient client, int limit = 10)
		{
			if (client.IsValid() && stream != null)
				stream.commits = await client.source.StreamGetCommits(client.token, stream.id, limit);

			return stream.commits.Valid();
		}
	}

	public static class ClientCommands
	{

		public static bool IsValid(this ISpeckleUnityClient client) => client?.account != null && client.source != null;

		public static async UniTask<List<Branch>> BranchesGet(this ISpeckleUnityClient client, string streamId, int branchLimit = 10, int commitLimit = 5)
		{
			var items = new List<Branch>();
			if (client.IsValid() && streamId.Valid())
				items = await client.source.StreamGetBranches(streamId, branchLimit, commitLimit);

			return items;
		}

		public static async UniTask<Texture2D> GetPreview(this ISpeckleUnityClient client, string streamId)
		{
			if (!client.IsValid())
				return null;

			return await GetPreviewTexture(client.GetUrl(true, streamId));
		}

		public static async UniTask<Texture2D> GetPreview(this ISpeckleUnityClient client, string streamId, StreamWrapperType type, string value)
		{
			if (!client.IsValid())
				return null;

			return await GetPreviewTexture(client.GetUrl(true, streamId, type, value));
		}

		public static string GetUrl(this ISpeckleUnityClient client, bool isPreview, string streamId) =>
			SpeckleUnity.GetUrl(isPreview, client.account.serverInfo.url, streamId);

		public static string GetUrl(this ISpeckleUnityClient client, bool isPreview, string streamId, StreamWrapperType type, string value) =>
			SpeckleUnity.GetUrl(isPreview, client.account.serverInfo.url, streamId, type, value);

		public static async UniTask<SpeckleObject> ObjectGet(this ISpeckleUnityClient client, string streamId, string input)
		{
			SpeckleObject res = null;
			try
			{
				if (client.IsValid())
				{
					res = await client.source.ObjectGet(client.token, streamId, input);
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		public static async UniTask<Branch> BranchGet(this ISpeckleUnityClient client, string streamId, string input)
		{
			Branch res = null;
			try
			{
				if (client.IsValid())
				{
					res = await client.source.BranchGet(client.token, streamId, input);
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		public static async UniTask<Commit> CommitGet(this ISpeckleUnityClient client, string streamId, string input)
		{
			Commit res = null;
			try
			{
				if (client.IsValid())
					res = await client.source.CommitGet(client.token, streamId, input);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// Creates a new stream with the account associated with the client
		/// If no valid account is found it will cancel the task 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="streamName"></param>
		/// <param name="description"></param>
		/// <param name="isPublic"></param>
		/// <returns></returns>
		public static async UniTask<string> StreamCreate(this ISpeckleUnityClient client, string streamName, string description, bool isPublic) =>
			await StreamCreate
			(client, new StreamCreateInput
				{
					name = streamName, isPublic = isPublic, description = description
				}
			);

		public static async UniTask<string> StreamCreate(this ISpeckleUnityClient client, StreamCreateInput input)
		{
			var streamId = "invalid";

			try
			{
				if (client.IsValid())
				{
					streamId = await client.source.StreamCreate(client.token, input);
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return streamId;
		}

		public static async UniTask<bool> StreamDestroy(this ISpeckleUnityClient client, string input)
		{
			var res = false;
			try
			{
				if (client.IsValid())
					res = await client.source.StreamDelete(client.token, input);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		public static async UniTask<bool> StreamExists(this ISpeckleUnityClient client, string input) => await StreamGet(client, input) != null;

		/// <summary>
		/// An Operation call to read in a speckle Stream. This will retrieve the stream information and populate the available branches  
		/// </summary>
		/// <param name="client"></param>
		/// <param name="input"></param>
		/// <param name="branchLimit"></param>
		/// <returns></returns>
		public static async UniTask<Stream> StreamGet(this ISpeckleUnityClient client, string input, int branchLimit = 10)
		{
			Stream res = null;
			try
			{
				if (client.IsValid())
				{
					res = await client.source.StreamGet(client.token, input, branchLimit);
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		static async UniTask<Texture2D> GetPreviewTexture(string url)
		{
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