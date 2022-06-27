using System.Collections.Generic;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	public struct MeshData
	{

		public List<Vector2> uvs;

		public List<Color> vertexColors;

		public List<Vector3> vertices;

		public List<List<int>> subMeshes;
	}
}