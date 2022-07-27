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

		public SpeckleStream(Stream obj) : base(obj)
		{
			if (obj == null) return;

			id = obj.id;
			name = obj.name;
			description = obj.description;
			role = obj.role;
			createdAt = obj.createdAt;
			updatedAt = obj.updatedAt;
			favoritedDate = obj.favoritedDate;
			isPublic = obj.isPublic;

			@object = obj.@object;
			branch = obj.branch;
			commit = obj.commit;
			branches = obj.branches?.items;
			commits = obj.commits?.items;
			activity = obj.activity;
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
			get => _speckleObject.source;
			set => _speckleObject = new SpeckleObjectWrapper(value);
		}

		/// <summary>
		/// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.BranchGet(System.Threading.CancellationToken,System.String,System.String,System.Int32)" />.
		/// </summary>
		public Branch branch
		{
			get => _branch?.source;
			set => _branch = new BranchWrapper(value);
		}

		/// <summary>
		/// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.CommitGet(System.Threading.CancellationToken,System.String,System.String)" />.
		/// </summary>
		public Commit commit
		{
			get => _commit?.source;
			set => _commit = new CommitWrapper(value);
		}

		public List<Branch> branches
		{
			get => _branches.Select(x => x.source).ToList();
			set => _branches = value.Select(x => new BranchWrapper(x)).ToList();
		}

		/// <summary>
		/// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.StreamGetCommits(System.Threading.CancellationToken,System.String,System.Int32)" />
		/// </summary>
		public List<Commit> commits
		{
			get => _commits.Select(x => x.source).ToList();
			set => _commits = value.Select(x => new CommitWrapper(x)).ToList();
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

	}
}