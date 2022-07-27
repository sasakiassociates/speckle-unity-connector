using System;
using System.Collections.Generic;
using System.Linq;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public class SpeckleStream : GenericWrapper<Stream>
	{
		public string id;
		public string name;
		public string description;
		public string role;
		public string createdAt;
		public string updatedAt;
		public string favoritedDate;
		public bool isPublic;

		[SerializeField] CommitWrapper _commit;

		[SerializeField] BranchWrapper _branch;

		[SerializeField] List<BranchWrapper> _branches;

		[SerializeField] List<CommitWrapper> _commits;

		[SerializeField] SpeckleObjectWrapper _speckleObject;

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

		public SpeckleStream(Stream value) : base(value)
		{
			if (value == null) return;

			id = value.id;
			name = value.name;
			description = value.description;
			role = value.role;
			createdAt = value.createdAt;
			updatedAt = value.updatedAt;
			favoritedDate = value.favoritedDate;
			isPublic = value.isPublic;
			activity = value.activity;

			@object = value.@object;
			branch = value.branch;
			commit = value.commit;

			branches = value.branches?.items;
			commits = value.commits?.items;
		}

		void Clear()
		{
			id = string.Empty;
			name = string.Empty;
			description = string.Empty;
			role = string.Empty;
			createdAt = string.Empty;
			updatedAt = string.Empty;
			favoritedDate = string.Empty;
			isPublic = false;

			branches = null;
			commits = null;
			activity = null;
			@object = null;
		}

		public Activity activity { get; set; }

		public SpeckleObject @object
		{
			get => _speckleObject?.source;
			set
			{
				if (value == null) return;

				_speckleObject = new SpeckleObjectWrapper(value);
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

				_branch = new BranchWrapper(value);
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

				_commit = new CommitWrapper(value);
				SpeckleUnity.Console.Log($"Setting Active {typeof(Commit)} to {_commit.source}");
				OnCommitSet?.Invoke(_commit.source);
			}
		}

		public List<Branch> branches
		{
			get => _branches.Valid() ? _branches.Select(x => x.source).ToList() : new List<Branch>();
			set
			{
				if (value == null || !value.Valid())
					return;

				_branches = value.Select(x => new BranchWrapper(x)).ToList();
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

				_commits = value.Select(x => new CommitWrapper(x)).ToList();
			}
		}

		public Branch BranchGet(int input) => _branches.Valid(input) ? _branches[input].source : null;

		public Branch BranchGet(string input)
		{
			Branch res = null;

			if (_branches.Valid() && input.Valid())
				foreach (var b in _branches)
					if (b.name.Valid() && b.name.Equals(input))
						res = b.source;

			return res;
		}

		public bool BranchSet(int input)
		{
			branch = BranchGet(input);
			return branch != null;
		}

		public bool BranchSet(string input)
		{
			branch = BranchGet(input);
			return branch != null;
		}

		public static string GetUrl(bool isPreview, string serverUrl, string streamId) => $"{serverUrl}/{(isPreview ? "preview" : "streams")}/{streamId}";

		public static string GetUrl(bool isPreview, string serverUrl, string streamId, StreamWrapperType type, string value)
		{
			string url = GetUrl(isPreview, serverUrl, streamId);
			switch (type)
			{
				case StreamWrapperType.Stream:
					return url;
				case StreamWrapperType.Commit:
					url += $"/commits/{value}";
					break;
				case StreamWrapperType.Branch:
					url += $"/branches/{value}";
					break;
				case StreamWrapperType.Object:
					url += $"objects/{value}";
					break;
				case StreamWrapperType.Undefined:
				default:
					SpeckleUnity.Console.Warn($"{streamId} is not a valid stream for server {serverUrl}, bailing on the preview thing");
					url = null;
					break;
			}

			return url;
		}

		public bool Equals(SpeckleStream obj) => Equals(obj.source);

		public bool Equals(Stream obj) => source != null && obj != null && id.Equals(obj.id) && name.Equals(obj.name);

		public event Action<Branch> OnBranchSet;

		public event Action<Commit> OnCommitSet;

	}
}