using System;
using Cysharp.Threading.Tasks;
using Objects.Utils;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Transports;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using SPG = Objects.Geometry;
using U = UnityEngine;

namespace Speckle.ConnectorUnity.Converters.Commands
{
	[U.RequireComponent(typeof(U.MeshFilter), typeof(U.MeshRenderer))]
	public class SpeckleTemp : U.MonoBehaviour
	{

		(string objectId, string streamId, string server) speckle;

		U.Mesh nativeMesh;

		void Awake()
		{
			speckle.objectId = "d611a3e8bf64984c50147e3f9238c925";
			speckle.streamId = "4777dea055";
			speckle.server = "https://speckle.xyz";
		}

		public async void Start()
		{
			U.Debug.Log("Starting Speckle Temp Process");
			var client = new Client(AccountManager.GetDefaultAccount());
			var transport = new ServerTransport(client.Account, speckle.streamId);

			SPG.Mesh speckleMesh = null;

			try
			{
				U.Debug.Log("0: Set up");

				var token = this.GetCancellationTokenOnDestroy();

				U.Debug.Log("1: Grabbing Object");

				var obj = await client.ObjectGet(token, speckle.streamId, speckle.objectId);

				U.Debug.Log($"Object Referenced? {obj != null}");

				U.Debug.Log("2: Receiving Object");

				var @base = await Operations.Receive(obj.id, token, transport);

				U.Debug.Log($"Received Completed? {@base != null}");

				if (@base is not SPG.Mesh mesh)
				{
					U.Debug.Log("Object was not a valid mesh!");
					return;
				}

				speckleMesh = mesh;
			}
			catch (Exception e)
			{
				U.Debug.Log(e.Message);
			}

			if (speckleMesh == null)
				return;

			try
			{
				U.Debug.Log("3: Processing Mesh");

				speckleMesh.AlignVerticesWithTexCoordsByIndex();
				speckleMesh.TriangulateMesh();
				
				var meshDataCollection = U.Mesh.AllocateWritableMeshData(1);
				var meshData = meshDataCollection[0];

				U.Debug.Log("4: Convert Mesh");

				var indexCount = speckleMesh.faces.Count / 4 * 3;
				var vertCount = speckleMesh.VerticesCount;

				var job = new SpeckleMeshJob
				{
					outputMesh = meshData,
					inputVertices = new NativeArray<double>(speckleMesh.vertices.ToArray(), Allocator.TempJob),
					inputFaces = new NativeArray<int>(speckleMesh.faces.ToArray(), Allocator.TempJob),
					Scale = (float)Units.GetConversionFactor(speckleMesh.units, Units.Meters),
				};

				job.outputMesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);
				job.outputMesh.SetVertexBufferParams(vertCount,
				                                     new VertexAttributeDescriptor(VertexAttribute.Position),
				                                     new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1));
				var handle = job.Schedule();

				nativeMesh = new U.Mesh
				{
					name = speckleMesh.id,
					indexFormat = IndexFormat.UInt32
				};

				GetComponent<U.MeshFilter>().mesh = nativeMesh;

				handle.Complete();

				U.Debug.Log("4: Apply Mesh");

				var sm = new SubMeshDescriptor(0, indexCount)
				{
					firstVertex = 0,
					vertexCount = vertCount
				};

				job.outputMesh.subMeshCount = 1;
				job.outputMesh.SetSubMesh(0, sm);

				U.Mesh.ApplyAndDisposeWritableMeshData(meshDataCollection, nativeMesh);
				
				U.Debug.Log("5: Recalc");

				nativeMesh.RecalculateBounds();
				nativeMesh.RecalculateNormals();
			}

			catch (Exception e)
			{
				U.Debug.Log(e.Message);
			}
		}

	}
}