using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{

	[Serializable]
	public class StreamAdapter : GenericAdapter<Stream>
	{
		[SerializeField] string _id;
		[SerializeField] string _name;
		[SerializeField] string _description;
		[SerializeField] string _role;
		[SerializeField] string _createdAt;
		[SerializeField] string _updatedAt;
		[SerializeField] string _favoritedDate;
		[SerializeField] bool _isPublic;

		[SerializeField] CommitAdapter _commit;
		[SerializeField] BranchAdapter _branch;
		[SerializeField] List<BranchAdapter> _branches;
		[SerializeField] List<CommitAdapter> _commits;
		[SerializeField] SpeckleObjectAdapter _speckleObject;

		public string name => _name;

		public string id => _id;

		public string description => _description;

		public string role => _role;

		public string createdAt => _createdAt;

		public string updatedAt => _updatedAt;

		public string favoritedDate => _favoritedDate;

		public bool isPublic => _isPublic;

		public event Action<Branch> OnBranchSet;

		public event Action<Commit> OnCommitSet;

		public StreamAdapter(Stream value) : base(value)
		{
			if (value == null)
				return;

			_id = value.id;
			_name = value.name;
			_description = value.description;
			_role = value.role;
			_createdAt = value.createdAt;
			_updatedAt = value.updatedAt;
			_favoritedDate = value.favoritedDate;
			_isPublic = value.isPublic;

			activity = value.activity;

			@object = value.@object;
			branch = value.branch;
			commit = value.commit;

			branches = value.branches?.items;
			commits = value.commits?.items;
		}

		public StreamWrapperType type
		{
			get
			{
				var res = StreamWrapperType.Undefined;

				if (@object != null)
					res = StreamWrapperType.Object;
				else if (commit != null)
					res = StreamWrapperType.Commit;
				else if (branch != null)
					res = StreamWrapperType.Branch;
				else if (id.Valid())
					res = StreamWrapperType.Stream;

				return res;
			}
		}

		public Activity activity { get; set; }

		public SpeckleObject @object
		{
			get => _speckleObject?.source;
			set
			{
				if (value == null)
					return;

				_speckleObject = new SpeckleObjectAdapter(value);
			}
		}

		/// <summary>
		/// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.BranchGet(System.Threading.CancellationToken,System.String,System.String,System.Int32)" />.
		/// </summary>
		public Branch branch
		{
			get => _branch?.source;
			set
			{
				if (value == null)
					return;

				_branch = new BranchAdapter(value);

				commits = value.commits?.items ?? new List<Commit>();

				SpeckleUnity.Console.Log($"Setting Active {typeof(Branch)} to {_branch.source}");
				OnBranchSet?.Invoke(_branch.source);
			}
		}

		/// <summary>
		/// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.CommitGet(System.Threading.CancellationToken,System.String,System.String)" />.
		/// </summary>
		public Commit commit
		{
			get => _commit?.source;
			set
			{
				if (value == null)
					return;

				_commit = new CommitAdapter(value);
				branch = new Branch { name = value.branchName };

				SpeckleUnity.Console.Log($"Setting Active {typeof(Commit)} to {_commit.source}");
				OnCommitSet?.Invoke(_commit.source);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public List<Branch> branches
		{
			get => _branches.Valid() ? _branches.Select(x => x.source).ToList() : new List<Branch>();
			set
			{
				if (value == null || !value.Valid())
					return;

				_branches = value.Select(x => new BranchAdapter(x)).ToList();
			}
		}

		/// <summary>
		/// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.StreamGetCommits(System.Threading.CancellationToken,System.String,System.Int32)" />
		/// </summary>
		public List<Commit> commits
		{
			get => _commits.Valid() ? _commits.Select(x => x.source).ToList() : new List<Commit>();
			set
			{
				if (value == null || !value.Valid())
					return;

				_commits = value.Select(x => new CommitAdapter(x)).ToList();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public Branch BranchGet(int input) => _branches.Valid(input) ? _branches[input].source : null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public Branch BranchGet(string input)
		{
			Branch res = null;

			if (_branches.Valid() && input.Valid())
				foreach (var b in _branches)
					if (b.name.Valid() && b.name.Equals(input))
						res = b.source;

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public bool BranchSet(int input)
		{
			branch = BranchGet(input);
			return branch != null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public bool BranchSet(string input)
		{
			branch = BranchGet(input);
			return branch != null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="commitId"></param>
		/// <returns></returns>
		public Commit CommitGet(int commitId) => _commits.Valid(commitId) ? _commits[commitId].source : null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="commitId"></param>
		/// <returns></returns>
		public Commit CommitGet(string commitId)
		{
			Commit res = null;

			if (_commits.Valid() && commitId.Valid())
				foreach (var b in _commits)
					if (b.id.Valid() && b.id.Equals(commitId))
						res = b.source;

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public bool CommitSet(int input)
		{
			commit = CommitGet(input);
			return commit != null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public bool CommitSet(string input)
		{
			commit = CommitGet(input);
			return commit != null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="isPreview"></param>
		/// <param name="serverUrl"></param>
		/// <returns></returns>
		public string GetUrl(bool isPreview, string serverUrl) => $"{serverUrl}/{GetUrl(isPreview)}";

		/// <summary>
		/// Gets a value a url for how this stream is setup
		/// </summary>
		/// <param name="isPreview"></param>
		/// <returns>returns the url without the server url</returns>
		public string GetUrl(bool isPreview)
		{
			string url = $"{(isPreview ? "preview" : "streams")}/{id}";
			switch (type)
			{
				case StreamWrapperType.Stream:
					return url;
				case StreamWrapperType.Commit:
					url += $"/commits/{commit.id}";
					break;
				case StreamWrapperType.Branch:
					url += $"/branches/{branch.name}";
					break;
				case StreamWrapperType.Object:
					url += $"objects/{@object.id}";
					break;
				case StreamWrapperType.Undefined:
				default:
					SpeckleUnity.Console.Warn($"{id} is not a valid stream, bailing on the preview thing");
					url = null;
					break;
			}

			return url;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public async UniTask<bool> ModifyInfo(SpeckleUnityClient client, StreamUpdateInput input)
		{
			var res = false;
			if (client.IsValid() && input != null)
				res = await client.StreamUpdate(input);

			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public async UniTask<bool> LoadObject(SpeckleUnityClient client, string value)
		{
			if (client.IsValid())
				@object = await client.ObjectGet(id, value);

			return @object != null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="branchLimit"></param>
		/// <param name="commitLimit"></param>
		/// <returns></returns>
		public async UniTask<bool> LoadBranches(SpeckleUnityClient client, int branchLimit = 10, int commitLimit = 5)
		{
			if (client.IsValid())
				branches = await client.BranchesGet(id, branchLimit, commitLimit);

			return branches.Valid();
		}

		/// <summary>
		/// Loads a stream branch and commits
		/// </summary>
		/// <param name="client"></param>
		/// <param name="branchName"></param>
		/// <param name="commitLimit"></param>
		/// <returns></returns>
		public async UniTask<bool> LoadBranch(SpeckleUnityClient client, string branchName, int commitLimit = 10)
		{
			if (client.IsValid())
			{
				branch = await client.BranchGet(id, branchName, commitLimit);
			}

			return branch != null && branch.name.Equals(branchName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public async UniTask<bool> LoadCommit(SpeckleUnityClient client, string input)
		{
			if (client.IsValid())
				commit = await client.CommitGet(id, input);

			return commit != null && commit.id.Equals(input);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		public async UniTask<bool> LoadCommits(SpeckleUnityClient client, int limit = 10)
		{
			if (client.IsValid())
				commits = await client.source.StreamGetCommits(client.token, id, limit);

			return commits.Valid();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="before"></param>
		/// <param name="after"></param>
		/// <param name="cursor"></param>
		/// <param name="actionType"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		public async UniTask<bool> LoadActivity(
			SpeckleUnityClient client,
			DateTime? before = null, DateTime? after = null, DateTime? cursor = null,
			string actionType = null,
			int limit = 10
		)
		{
			if (client.IsValid())
			{
				var res = await client.StreamActivity(id, before, after, cursor, actionType, limit);
				if (res.Valid())
				{
					activity = new Activity()
					{
						cursor = cursor ?? default,
						totalCount = res.Count,
						items = res
					};
				}
			}

			return activity != null;
		}

		public bool Equals(StreamAdapter obj) => Equals(obj.source);

		public bool Equals(Stream obj) => source != null && obj != null && id.Equals(obj.id) && name.Equals(obj.name);

		protected override Stream Get() => new Stream
		{
			id = this.id,
			name = this.name,
			description = this.description,
			role = this.role,
			createdAt = this.createdAt,
			updatedAt = this.updatedAt,
			favoritedDate = this.favoritedDate,
			isPublic = this.isPublic
		};

		void Clear()
		{
			_id = string.Empty;
			_name = string.Empty;
			_description = string.Empty;
			_role = string.Empty;
			_createdAt = string.Empty;
			_updatedAt = string.Empty;
			_favoritedDate = string.Empty;
			_isPublic = false;

			branches = null;
			commits = null;
			activity = null;
			@object = null;
		}

		/// <summary>
		/// Simple check if the id is set to this stream
		/// </summary>
		/// <returns></returns>
		public bool IsValid() => id.Valid();
	}
}