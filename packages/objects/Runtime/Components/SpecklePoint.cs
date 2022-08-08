using Speckle.ConnectorUnity.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity
{
	// TODO: add in rendering support for speckle point
	public class SpecklePoint : BaseBehaviour
	{
		public Vector3 pos
		{
			get => transform.position;
			set => transform.position = pos;
		}
	}
}