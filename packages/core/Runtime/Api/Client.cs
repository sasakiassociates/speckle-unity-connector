using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;

namespace Speckle.ConnectorUnity.Ops
{

	[Serializable]
	public sealed class SpeckleUnityClient : IDisposable, IShouldValidate
	{

		AccountAdapter _accountAdapter;

		public Account account
		{
			get => _accountAdapter?.source;
		}

		public CancellationToken token { get; set; }

		public Client source { get; private set; }

		public SpeckleUnityClient(Account obj)
		{
			if (obj == null) return;

			_accountAdapter = new AccountAdapter(obj);
			source = new Client(account);
		}

		public void Dispose()
		{
			source?.Dispose();
		}

		/// <summary>
		/// Loads a new speckle stream by fetching the stream with a valid url
		/// </summary>
		/// <param name="url"></param>
		public async UniTask<Stream> LoadStreamByUrl(string url)
		{
			Stream res = null;
			SpeckleUnityClient c = null;

			try
			{
				var s = new StreamWrapper(url);

				if (s.IsValid)
				{
					c = new SpeckleUnityClient(await s.GetAccount());
					res = await c.StreamGet(url);
				}
			}

			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Warn(e.Message);
				c?.Dispose();
			}

			return res;
		}

		/// <summary>
		/// Checks if this this linked with an account and speckle this 
		/// </summary>
		/// <returns></returns>
		public bool IsValid() => account != null && source != null;

		#region object operations

		/// <summary>
		/// 
		/// </summary>
		/// <param name="streamId"></param>
		/// <param name="objectId"></param>
		/// <returns></returns>
		public async UniTask<SpeckleObject> ObjectGet(string streamId, string objectId)
		{
			SpeckleObject res = null;
			try
			{
				if (IsValid())
				{
					res = await source.ObjectGet(token, streamId, objectId);
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		#endregion

		#region branch operations

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async UniTask<string> BranchCreate(BranchCreateInput input)
		{
			var res = string.Empty;
			try
			{
				if (IsValid())
					res = await source.BranchCreate(token, input);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async UniTask<bool> BranchDelete(BranchDeleteInput input)
		{
			var res = false;
			try
			{
				if (IsValid())
					res = await source.BranchDelete(token, input);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="streamId"></param>
		/// <param name="branchLimit"></param>
		/// <param name="commitLimit"></param>
		/// <returns></returns>
		public async UniTask<List<Branch>> BranchesGet(string streamId, int branchLimit = 10, int commitLimit = 5)
		{
			var items = new List<Branch>();
			if (IsValid() && streamId.Valid())
				items = await source.StreamGetBranches(token, streamId, branchLimit, commitLimit);

			return items;
		}

		/// <summary>
		/// Loads a branch and its commits from a speckle stream
		/// </summary>
		/// <param name="streamId"></param>
		/// <param name="branchName"></param>
		/// <param name="commitLimit"></param>
		/// <returns></returns>
		public async UniTask<Branch> BranchGet(string streamId, string branchName, int commitLimit = 10)
		{
			Branch res = null;
			try
			{
				if (IsValid())
				{
					res = await source.BranchGet(token, streamId, branchName, commitLimit);
				}
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		#endregion

		#region commit operations

		/// <summary>
		/// 
		/// </summary>
		/// <param name="streamId"></param>
		/// <param name="commitId"></param>
		/// <returns></returns>
		public async UniTask<Commit> CommitGet(string streamId, string commitId)
		{
			Commit res = null;
			try
			{
				if (IsValid())
					res = await source.CommitGet(token, streamId, commitId);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			UniTask.Yield();
			return res;
		}

		public async UniTask<bool> CommitExists(string streamId, string commitId)
		{
			var res = false;
			try
			{
				if (IsValid())
					res = await source.CommitGet(token, streamId, commitId) != null;
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async UniTask<string> CommitCreate(CommitCreateInput input)
		{
			var res = string.Empty;
			try
			{
				if (IsValid())
					res = await source.CommitCreate(token, input);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async UniTask<bool> CommitUpdate(CommitUpdateInput input)
		{
			var res = false;
			try
			{
				if (IsValid())
					res = await source.CommitUpdate(token, input);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async UniTask<bool> CommitDelete(CommitDeleteInput input)
		{
			var res = false;
			try
			{
				if (IsValid())
					res = await source.CommitDelete(token, input);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async UniTask<bool> CommitReceived(CommitReceivedInput input)
		{
			var res = false;
			try
			{
				if (IsValid())
					res = await source.CommitReceived(token, input);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		#endregion

		#region stream operations

		/// <summary>
		/// Creates a new stream with the account associated with the this
		/// If no valid account is found it will cancel the task 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="isPublic"></param>
		/// <returns></returns>
		public async UniTask<string> StreamCreate(string name, string description, bool isPublic) => await StreamCreate
		(new StreamCreateInput
			{
				name = name, isPublic = isPublic, description = description
			}
		);

		/// <summary>
		/// Creates a new stream with the account associated with the this
		/// If no valid account is found it will cancel the task 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async UniTask<string> StreamCreate(StreamCreateInput input)
		{
			var streamId = "invalid";

			try
			{
				if (IsValid())
					streamId = await source.StreamCreate(token, input);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return streamId;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="streamId"></param>
		/// <returns></returns>
		public async UniTask<bool> StreamDelete(string streamId)
		{
			var res = false;
			try
			{
				if (IsValid())
					res = await source.StreamDelete(token, streamId);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async UniTask<bool> StreamUpdate(StreamUpdateInput input)
		{
			var res = false;
			try
			{
				if (IsValid())
					res = await source.StreamUpdate(token, input);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="streamId"></param>
		/// <param name="before"></param>
		/// <param name="after"></param>
		/// <param name="cursor"></param>
		/// <param name="actionType"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		public async UniTask<List<ActivityItem>> StreamActivity(
			string streamId,
			DateTime? before = null, DateTime? after = null, DateTime? cursor = null,
			string actionType = null,
			int limit = 10
		)
		{
			var res = new List<ActivityItem>();
			try
			{
				if (IsValid())
					res = await source.StreamGetActivity(token, streamId, before, after, cursor, actionType, limit);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="streamId"></param>
		/// <param name="branchLimit"></param>
		/// <returns></returns>
		public async UniTask<List<Stream>> StreamSearch(string streamId, int branchLimit = 10)
		{
			var res = new List<Stream>();
			try
			{
				if (IsValid())
					res = await source.StreamSearch(token, streamId, branchLimit);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		/// <summary>
		/// An Operation call to read in a speckle Stream. This will retrieve the stream information and populate the available branches  
		/// </summary>
		/// <param name="streamId"></param>
		/// <param name="branchLimit"></param>
		/// <returns></returns>
		public async UniTask<Stream> StreamGet(string streamId, int branchLimit = 10)
		{
			Stream res = null;
			try
			{
				if (IsValid())
					res = await source.StreamGet(token, streamId, branchLimit);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}

			return res;
		}

		public async UniTask<bool> StreamExists(string streamId) => await StreamGet(streamId, 0) != null;

		#endregion

	}

}