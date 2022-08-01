using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Speckle.ConnectorUnity.Converters.Commands
{
	[BurstCompile]
	public struct SpeckleMeshJob : IJob
	{
		public Mesh.MeshData outputMesh;

		[ReadOnly]
		public float Scale;

		[DeallocateOnJobCompletion]
		[ReadOnly]
		public NativeArray<double> inputVertices;

		[DeallocateOnJobCompletion]
		[ReadOnly]
		public NativeArray<int> inputFaces;

		public void Execute()
		{
			var outputVerts = outputMesh.GetVertexData<float3>();

			for (int i = 0, k = 0; i < inputVertices.Length; i += 3)
				outputVerts[k++] = float3(
					(float)inputVertices[i] * Scale,
					(float)inputVertices[i + 2] * Scale, // flip y with z
					(float)inputVertices[i + 1] * Scale
				);

			var outputTris = outputMesh.GetIndexData<int3>();

			// skip the 0 index and then only grab every 3 
			for (int i = 0, k = 0; i < inputFaces.Length; i += 4, k++)			
				outputTris[k] = new int3(inputFaces[i + 1], inputFaces[i + 3], inputFaces[i + 2]);

		}

	}
}