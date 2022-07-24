using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Logging;

namespace Speckle.ConnectorUnity.Ops
{
	public static class ClientCommands
	{
		public static bool IsValid(this ISpeckleUnityClient source) => source != null && source.account != null && source.client != null;


		public static async UniTask<bool> LoadBranches(this SpeckleStream stream, ISpeckleUnityClient client, int branchLimit = 10, int commitLimit = 5)
		{
			if (stream != null && client != null && client.IsValid())
			{
				stream.branches = await client.client.StreamGetBranches(stream.id, branchLimit, commitLimit);
				return stream.branches.Valid();
			}

			return false;
		}

		public static async UniTask<SpeckleObject> ObjectGet(this ISpeckleUnityClient source, string streamId, string objectId)
		{
			SpeckleObject res = null;
			try
			{
				if (source.IsValid())
				{
					res = await source.client.ObjectGet(source.token, streamId, objectId);
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		public static async UniTask<List<Branch>> StreamGetBranches(this ISpeckleUnityClient source, string streamId, int branchLimit = 10, int commitLimit = 5)
		{
			List<Branch> res = new List<Branch>();
			try
			{
				if (source.IsValid())
				{
					res = await source.client.StreamGetBranches(source.token, streamId, branchLimit, commitLimit);
				}
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
		/// <param name="source"></param>
		/// <param name="streamName"></param>
		/// <param name="description"></param>
		/// <param name="isPublic"></param>
		/// <returns></returns>
		public static async UniTask<string> StreamCreate(this ISpeckleUnityClient source, string streamName, string description, bool isPublic) =>
			await StreamCreate
			(source, new StreamCreateInput
				{
					name = streamName, isPublic = isPublic, description = description
				}
			);

		public static async UniTask<string> StreamCreate(this ISpeckleUnityClient source, StreamCreateInput input)
		{
			var streamId = "invalid";

			try
			{
				if (source.IsValid())
				{
					streamId = await source.client.StreamCreate(source.token, input);
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return streamId;
		}

		public static async UniTask<bool> StreamDestroy(this ISpeckleUnityClient source, string input)
		{
			var res = false;
			try
			{
				if (source.IsValid())
				{
					res = await source.client.StreamDelete(source.token, input);
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		public static async UniTask<bool> StreamExists(this ISpeckleUnityClient source, string input) => await StreamGet(source, input) != null;

		public static async UniTask<SpeckleStream> StreamGet(this ISpeckleUnityClient source, string input, int branchLimit = 10)
		{
			SpeckleStream res = null;
			try
			{
				if (source.IsValid())
				{
					Stream stream = await source.client.StreamGet(source.token, input, branchLimit);
					if (stream != null)
					{
						res = new SpeckleStream();
						res.Load(stream);
					}
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}
	}
}