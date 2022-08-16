using Speckle.ConnectorUnity.Models;
using Speckle.Core.Api;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{
	[AddComponentMenu(SpeckleUnity.NAMESPACE + "/Speckle Object")]
	public class SpeckleObjectBehaviour : MonoBehaviour, ICanAdapt<SpeckleObject>, IShouldValidate
	{
		/// <summary>
		///   Reference object id
		/// </summary>
		[ReadOnly] public string id;
		/// <summary>
		///  Reference to when the object was created
		/// </summary>
		[ReadOnly] public string createdAt;
		/// <summary>
		///  Reference speckle type
		/// </summary>
		[ReadOnly] public string speckleType;
		/// <summary>
		///   Reference to application ID
		/// </summary>
		[ReadOnly] public string applicationId;
		/// <summary>
		///   Total child count
		/// </summary>
		[ReadOnly] public int totalChildrenCount;

		[SerializeField] SpeckleObjectHierarchy _hierarchy;

		public SpeckleObjectHierarchy hierarchy
		{
			get => _hierarchy;
			set => _hierarchy = value;
		}

		public SpeckleObject source
		{
			get =>
				new SpeckleObject
				{
					id = this.id,
					speckleType = this.speckleType,
					applicationId = this.applicationId,
					createdAt = this.createdAt,
					totalChildrenCount = this.totalChildrenCount
				};
			set
			{
				if (value == null) return;

				id = value.id;
				createdAt = value.createdAt;
				speckleType = value.speckleType;
				applicationId = value.applicationId;
				totalChildrenCount = value.totalChildrenCount;
			}
		}

		public bool IsValid() => id.Valid();

		void OnEnable() => hierarchy ??= new SpeckleObjectHierarchy();

		public void Purge()
		{
			var objs = hierarchy.GetObjects();

			if (!objs.Valid()) return;

			for (int i = objs.Count - 1; i >= 0; i--)
			{
				if (objs[i] != null)
					SpeckleUnity.SafeDestroy(objs[i]);
			}
		}

		public override string ToString() =>
			$"ID: {id}\nType: {speckleType}\nCount: {totalChildrenCount}\nApplication: {applicationId}\nCreated At: {createdAt}";
	}
}