using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public class SpeckleStream
	{
		public string id;
		public string name;
		public string description;
		public string role;
		public string createdAt;
		public string updatedAt;
		public string favoritedDate;
		public bool isPublic;

		public Stream source
		{
			get
			{
				return new Stream
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
			}
		}

		public void Load(Stream input)
		{
			if (input == null) return;

			Clear();

			id = input.id;
			name = input.name;
			description = input.description;
			role = input.role;
			createdAt = input.createdAt;
			updatedAt = input.updatedAt;
			favoritedDate = input.favoritedDate;
			isPublic = input.isPublic;
		}

		/// <summary>
		/// Loads a new speckle stream by fetching the stream with a valid url
		/// </summary>
		/// <param name="url"></param>
		public async UniTask Load(string url) => Load(await SpeckleUnity.GetStreamByUrlAsync(url));

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

		public SpeckleObject @object { get; set; }

		public List<Branch> branches { get; set; }

		/// <summary>
		/// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.BranchGet(System.Threading.CancellationToken,System.String,System.String,System.Int32)" />.
		/// </summary>
		public Branch branch { get; set; }

		/// <summary>
		/// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.CommitGet(System.Threading.CancellationToken,System.String,System.String)" />.
		/// </summary>
		public Commit commit { get; set; }

		/// <summary>
		/// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.StreamGetCommits(System.Threading.CancellationToken,System.String,System.Int32)" />
		/// </summary>
		public List<Commit> commits { get; set; }

	}
}