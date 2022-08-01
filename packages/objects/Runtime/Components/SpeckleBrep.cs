using Speckle.ConnectorUnity.Mono;
using UnityEngine;

namespace Speckle.ConnectorUnity
{
	public class SpeckleBrep : BaseBehaviour
	{
		public MeshFilter mesh
		{
			get => gameObject.GetComponent<MeshFilter>();
		}
	}
}