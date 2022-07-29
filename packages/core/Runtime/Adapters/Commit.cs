using System;
using System.Collections.Generic;
using Speckle.Core.Api;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public sealed class CommitAdapter : GenericAdapter<Commit>
	{
		public string id;
		public string message;
		public string branchName;
		public string authorName;
		public string authorId;
		public string authorAvatar;
		public string createdAt;
		public string sourceApplication;
		public string referencedObject;
		public int totalChildrenCount;
		public List<string> parents;

		protected override Commit Get()
		{
			return new Commit
			{
				authorId = this.authorId,
				authorName = this.authorName,
				authorAvatar = this.authorAvatar,
				id = this.id,
				message = this.message,
				branchName = this.branchName,
				referencedObject = this.referencedObject,
				totalChildrenCount = this.totalChildrenCount,
				createdAt = this.createdAt,
				sourceApplication = this.sourceApplication,
				parents = this.parents
			};
		}

		public override string ToString() => source.ToString();

		public CommitAdapter(Commit value) : base(value)
		{
			if (value == null) return;

			authorId = value.authorId;
			authorName = value.authorName;
			authorAvatar = value.authorAvatar;

			id = value.id;
			message = value.message;
			branchName = value.branchName;
			referencedObject = value.referencedObject;
			totalChildrenCount = value.totalChildrenCount;

			createdAt = value.createdAt;
			sourceApplication = value.sourceApplication;

			parents = value.parents;
		}
	}
}