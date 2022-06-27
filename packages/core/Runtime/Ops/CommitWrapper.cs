using System;
using Speckle.Core.Api;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public class CommitWrapper
	{
		public readonly string branch;

		public readonly string id;

		public readonly string message;

		public CommitWrapper(Commit commit)
		{
			id = commit.id;
			message = commit.message;
			branch = commit.branchName;
		}
	}
}