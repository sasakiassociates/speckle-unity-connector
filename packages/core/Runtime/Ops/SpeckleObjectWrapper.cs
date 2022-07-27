using System;
using Speckle.Core.Api;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public class SpeckleObjectWrapper : GenericWrapper<SpeckleObject>
	{
		public string id;

		public string speckleType;

		public string applicationId;

		public int totalChildrenCount;

		public string createdAt;

		public SpeckleObjectWrapper(SpeckleObject obj) : base(obj)
		{
			id = obj.id;
			speckleType = obj.speckleType;
			applicationId = obj.applicationId;
			totalChildrenCount = obj.totalChildrenCount;
			createdAt = obj.createdAt;
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