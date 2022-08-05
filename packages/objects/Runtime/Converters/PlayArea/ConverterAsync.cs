using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Objects.Utils;
using Speckle.ConnectorUnity.Converters.Commands;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using OG = Objects.Geometry;

namespace Speckle.ConnectorUnity.Converter.PlayArea
{
	public class ConverterAsync : ISpeckleMeshConverter
	{
		public ConverterAsync()
		{
			_sceneObjects = new Dictionary<string, int>();
			_objectByBaseId = new Dictionary<string, GameObject>();
		}

		public List<ApplicationPlaceholderObject> contextObjects { get; set; } = new List<ApplicationPlaceholderObject>();

		public bool addMeshCollider { get; } = false;

		public bool addMeshRenderer { get; } = true;

		public bool recenterTransform { get; } = true;

		public bool useRenderMaterial { get; } = true;

		public Material defaultMaterial { get; }

		public bool combineMeshes { get; } = false;

		GameObject _main;

		/// <summary>
		/// string = base object Id
		/// int = instance value for game object
		/// </summary>
		Dictionary<string, int> _sceneObjects;

		Dictionary<string, GameObject> _objectByBaseId;

		public void Step1_SetupOnThread(string id)
		{
			var obj = new GameObject().AddComponent<MeshRenderer>().gameObject.AddComponent<MeshFilter>().gameObject;

			var objectInstanceId = obj.GetInstanceID();
			var componentInstanceId = obj.GetComponent<MeshFilter>().GetInstanceID();

			Debug.Log($"Object instance ID:{objectInstanceId} MeshFilter instance ID:{componentInstanceId} ");

			foreach (var comp in obj.GetComponents<Component>())
			{
				if (comp != null && comp.GetInstanceID() == componentInstanceId)
				{
					Debug.Log($"Found comp ({comp.name}) by Id {componentInstanceId}");
				}
			}
			_objectByBaseId.Add(id, obj);
			_sceneObjects.Add(id, objectInstanceId);
		}

		public async UniTask<MeshFilter> Step2_Run(OG.Mesh mesh)
		{
			var obj = _objectByBaseId.First(x => x.Key.Equals(mesh.id)).Value;
			if (obj == null)
			{
				Debug.Log($"No valid object found with id: {mesh.id}");
				return null;
			}

			await this.MeshToNative(mesh, obj);
			return obj.GetComponent<MeshFilter>();
		}

		public async UniTask<MeshFilter> Step2_RunJob(OG.Mesh subMesh)
		{
			var meshDataCollection = Mesh.AllocateWritableMeshData(1);
			var data = meshDataCollection[0];

			data.subMeshCount = 1;

			var job = await CreateJob(subMesh, data, out var indexCount, out var vertCount);

			var handle = job.Schedule();

			var nativeMesh = new Mesh
			{
				name = subMesh.id,
				indexFormat = IndexFormat.UInt32
			};

			var instanceId = _sceneObjects.First(x => x.Key.Equals(subMesh.id)).Value;

			MeshFilter comp = null;

			foreach (var obj in Object.FindObjectsOfType<GameObject>())
			{
				if (obj.GetInstanceID().Equals(instanceId))
					comp = obj.GetComponent<MeshFilter>();
			}

			comp.mesh = nativeMesh;

			handle.Complete();

			var sm = new SubMeshDescriptor(0, indexCount)
			{
				firstVertex = 0,
				vertexCount = vertCount
			};

			// TODO: add in grouping option
			job.outputMesh.SetSubMesh(0, sm);

			Mesh.ApplyAndDisposeWritableMeshData(meshDataCollection, nativeMesh);

			nativeMesh.RecalculateBounds();
			nativeMesh.RecalculateNormals();

			return comp;
		}

		public async UniTask<MeshFilter> Step2_RunJob(List<OG.Mesh> meshes)
		{
			var meshDataCollection = Mesh.AllocateWritableMeshData(meshes.Count);

			for (var i = 0; i < meshes.Count; i++)
			{
				SpeckleUnity.Console.Log($"Creating job {i + 1}:{meshes.Count}");

				var subMesh = meshes[i];

				var data = meshDataCollection[i];
				data.subMeshCount = 1;

				var job = await CreateJob(subMesh, data, out var indexCount, out var vertCount);

				var handle = job.Schedule();

				var nativeMesh = new Mesh
				{
					name = meshes[i].id,
					indexFormat = IndexFormat.UInt32
				};

				var instanceId = _sceneObjects.First(x => x.Key.Equals(subMesh.id)).Value;

				MeshFilter comp = null;
				foreach (var obj in Object.FindObjectsOfType<GameObject>())
				{
					if (obj.GetInstanceID().Equals(instanceId))
						comp = obj.GetComponent<MeshFilter>();
				}

				comp.mesh = nativeMesh;

				handle.Complete();

				var sm = new SubMeshDescriptor(0, indexCount)
				{
					firstVertex = 0,
					vertexCount = vertCount
				};

				// TODO: add in grouping option
				job.outputMesh.SetSubMesh(0, sm);

				Mesh.ApplyAndDisposeWritableMeshData(meshDataCollection, nativeMesh);

				nativeMesh.RecalculateBounds();
				nativeMesh.RecalculateNormals();
			}

			return _main.GetComponent<MeshFilter>();
		}

		UniTask<SpeckleMeshJob> CreateJob(OG.Mesh subMesh, Mesh.MeshData data, out int indexCount, out int vertCount)
		{
			subMesh.AlignVerticesWithTexCoordsByIndex();
			subMesh.TriangulateMesh();

			indexCount = subMesh.faces.Count / 4 * 3;
			vertCount = subMesh.VerticesCount;

			var job = new SpeckleMeshJob
			{
				outputMesh = data,
				inputVertices = new NativeArray<double>(subMesh.vertices.ToArray(), Allocator.TempJob),
				inputFaces = new NativeArray<int>(subMesh.faces.ToArray(), Allocator.TempJob),
				Scale = (float)Units.GetConversionFactor(subMesh.units, Units.Meters),
			};

			job.outputMesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);
			job.outputMesh.SetVertexBufferParams(vertCount,
			                                     new VertexAttributeDescriptor(VertexAttribute.Position),
			                                     new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1));

			return new UniTask<SpeckleMeshJob>(job);
		}

	}
}