using System;
using Speckle.Core.Api;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public sealed class SpeckleObjectWrapper : GenericWrapper<SpeckleObject>
	{
		public string id;
		public string createdAt;
		public string speckleType;
		public string applicationId;
		public int totalChildrenCount;

		public SpeckleObjectWrapper(SpeckleObject value) : base(value)
		{
			if (value == null) return;

			id = value.id;
			speckleType = value.speckleType;
			applicationId = value.applicationId;
			totalChildrenCount = value.totalChildrenCount;
			createdAt = value.createdAt;
		}

		protected override SpeckleObject Get() => new SpeckleObject
		{
			id = this.id,
			speckleType = this.speckleType,
			applicationId = this.applicationId,
			totalChildrenCount = this.totalChildrenCount,
			createdAt = this.createdAt,
		};
	}
}