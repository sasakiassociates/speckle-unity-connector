using System;
using System.Collections.Generic;
using System.Linq;
using Speckle.Core.Api;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public sealed class BranchWrapper : GenericWrapper<Branch>
	{

		public string name;
		public string id;
		public string description;
		public string commitCursor;
		public int commitTotalCount;
		public List<CommitWrapper> commits;

		public BranchWrapper(Branch value) : base(value)
		{
			if (value == null) return;

			id = value.id;
			name = value.name;
			description = value.description;
			commitCursor = value.commits.cursor;
			commitTotalCount = value.commits.totalCount;

			commits = value.commits != null && value.commits.items.Valid() ?
				value.commits.items.Select(x => new CommitWrapper(x)).ToList() : new List<CommitWrapper>();
		}

		// These wrappers should only be used for ui bits.

		protected override Branch Get() => new Branch
		{
			name = this.name,
			id = this.id,
			description = this.description,
			commits = new Commits
			{
				items = commits.Select(x => x.source).ToList(),
				cursor = commitCursor,
				totalCount = commitTotalCount,
			}
		};

		public override string ToString() => source.ToString();
	}
}