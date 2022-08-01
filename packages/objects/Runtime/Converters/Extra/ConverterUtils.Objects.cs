using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	public static partial class ConverterUtils
	{

		public static Mesh SafeMeshGet(this MeshFilter mf) => Application.isPlaying ? mf.mesh : mf.sharedMesh;

		public static void SafeMeshSet(this GameObject go, Mesh m, bool addMeshFilterIfNotFound = true)
		{
			var mf = go.GetComponent<MeshFilter>();
			if (mf == null)
			{
				if (!addMeshFilterIfNotFound) return;

				mf = go.AddComponent<MeshFilter>();
			}

			if (Application.isPlaying)
				mf.mesh = m;
			else
				mf.sharedMesh = m;
		}

		public static void SetupLineRenderer(this LineRenderer lineRenderer, Vector3[] points, float diameter = 1)
		{
			if (points.Length == 0) return;

			lineRenderer.positionCount = points.Length;
			lineRenderer.SetPositions(points);
			lineRenderer.numCornerVertices = lineRenderer.numCapVertices = 8;
			lineRenderer.startWidth = lineRenderer.endWidth = diameter;
		}
	}
}