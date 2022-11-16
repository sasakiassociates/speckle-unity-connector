using System;
using System.Collections.Generic;
using System.Linq;
using Speckle.Core.Api;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public sealed class SpeckleBranch : GenericAdapter<Branch>
	{

		public string name;
		public string id;
		public string description;
		public string commitCursor;
		public int commitTotalCount;
		public List<SpeckleCommit> commits;

		public SpeckleBranch(Branch value) : base(value)
		{
			if (value == null)
				return;

			id = value.id;
			name = value.name;
			description = value.description;
			commitCursor = value.commits?.cursor;
			commitTotalCount = value.commits?.totalCount ?? 0;

			commits = value.commits != null && value.commits.items.Valid() ?
				value.commits.items.Select(x => new SpeckleCommit(x)).ToList() : new List<SpeckleCommit>();
		}

		// These wrappers should only be used for ui bits.

		protected override Branch Get() => new Branch
		{
			name = this.name,
			id = this.id,
			description = this.description,
			commits = new Commits
			{
				items = commits.Valid() ? commits.Select(x => x.source).ToList() : new List<Commit>(),
				cursor = commitCursor,
				totalCount = commitTotalCount,
			}
		};

		public override string ToString() => source.ToString();
	}
}