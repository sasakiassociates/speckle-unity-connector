using Speckle.ConnectorUnity.Mono;
using UnityEngine;

namespace Speckle.ConnectorUnity
{
	public class SpeckleBrep : BaseBehaviour_v1
	{
		public MeshFilter mesh
		{
			get => gameObject.GetComponent<MeshFilter>();
		}
	}
}