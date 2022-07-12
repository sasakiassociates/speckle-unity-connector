using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Objects.Utils;
using Speckle.Core.Models;
using UnityEngine;
using Mesh = Objects.Geometry.Mesh;
using UN = UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

	[CreateAssetMenu(fileName = nameof(MeshConverter), menuName = "Speckle/Converters/Create Mesh Converter")]
	public class MeshConverter : ComponentConverter<Mesh, MeshFilter>, ISpeckleMeshConverter
	{

		/// <summary>
		/// Adds mesh colliders to each mesh object
		/// </summary>
		[SerializeField] bool _addMeshCollider;

		/// <summary>
		/// Adds a mesh renderer to each object, helpful if you want to see what you're looking at
		/// </summary>
		[SerializeField] bool _addRenderer = true;

		/// <summary>
		/// Repositions the origin of the mesh object to the center of the mesh bounds 
		/// </summary>
		[SerializeField] bool _recenterTransform = true;

		/// <summary>
		/// Reference the speckle mesh material for the mesh renderer
		/// </summary>
		[SerializeField] bool _useRenderMaterial;

		/// <summary>
		/// Default material for a speckle unity object
		/// </summary>
		[SerializeField] Material _defaultMaterial;

		/// <summary>
		/// Combines all the meshes in a speckle object into a single instance. Useful for reducing draw calls
		/// </summary>
		[SerializeField] bool _combineMeshes = false;

		protected override HashSet<string> excludedProps
		{
			get
			{
				var res = base.excludedProps;
				res.Add("displayValue");
				res.Add("displayMesh");
				return res;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if (_defaultMaterial == null)
				_defaultMaterial = new Material(Shader.Find("Standard"));
		}

		public List<ApplicationPlaceholderObject> contextObjects
		{
			get;
			set;
		}

		public bool addMeshCollider
		{
			get => _addMeshCollider;
		}

		public bool addMeshRenderer
		{
			get => _addRenderer;
		}

		public bool recenterTransform
		{
			get => _recenterTransform;
		}

		public bool useRenderMaterial
		{
			get => _useRenderMaterial;
		}

		public bool combineMeshes
		{
			get => _combineMeshes;
		}

		public Material defaultMaterial
		{
			get => _defaultMaterial;
		}

		protected override GameObject ConvertBase(Mesh @base)
		{
			// convert the mesh data
			return this.MeshToNative(new[] { @base }, NewObj().gameObject);
		}

		// copied from repo
		//TODO: support multiple filters?
		protected override Base ConvertComponent(MeshFilter component) => this.MeshToSpeckle(component);

		public static GameObject CreateAndProcess(Mesh @base)
		{
			if (@base == null)
				return null;

			var obj = CreateObjectWithComponent(@base.speckle_type);

			var d = new UnityEngine.Mesh.MeshData();

			// speckleMesh.AlignVerticesWithTexCoordsByIndex();
			// 		speckleMesh.TriangulateMesh();
			//
			// 		var indexOffset = data.vertices.Count;
			//
			// 		// Convert Vertices
			// 		data.vertices.AddRange(speckleMesh.vertices.ArrayToPoints(speckleMesh.units));
			//
			// 		// Convert texture coordinates
			// 		var hasValidUVs = speckleMesh.TextureCoordinatesCount == speckleMesh.VerticesCount;
			// 		if (speckleMesh.textureCoordinates.Count > 0 && !hasValidUVs)
			// 			Debug.LogWarning(
			// 				$"Expected number of UV coordinates to equal vertices. Got {speckleMesh.TextureCoordinatesCount} expected {speckleMesh.VerticesCount}. \nID = {speckleMesh.id}");
			//
			// 		if (hasValidUVs)
			// 		{
			// 			data.uvs.Capacity += speckleMesh.TextureCoordinatesCount;
			// 			for (var j = 0; j < speckleMesh.TextureCoordinatesCount; j++)
			// 			{
			// 				var (u, v) = speckleMesh.GetTextureCoordinate(j);
			// 				data.uvs.Add(new Vector2((float)u, (float)v));
			// 			}
			// 		}

			// 			if (speckleMesh.vertices.Count == 0 || speckleMesh.faces.Count == 0) continue;
			//
			// 			data.AddMesh(speckleMesh);
			// 			// Convert RenderMaterial
			// 			materials.Add(converter.useRenderMaterial ?
			// 				              GetMaterial(converter, speckleMesh["renderMaterial"] as RenderMaterial) :
			// 				              converter.defaultMaterial

			return obj.gameObject;
		}


		// public static UniTask<IEnumerable<Vector3>> GetVerticesTask(List<double> verts)
		// {
		// 	return 
		// }

		public static void AddMesh(MeshData data, Mesh speckleMesh)
		{
			speckleMesh.AlignVerticesWithTexCoordsByIndex();
			speckleMesh.TriangulateMesh();

			var indexOffset = data.vertices.Count;

			var vertices = speckleMesh.vertices.ArrayToVector3(speckleMesh.units);
			// Convert Vertices
			data.vertices.AddRange(vertices);

			// Convert texture coordinates
			var hasValidUVs = speckleMesh.TextureCoordinatesCount == speckleMesh.VerticesCount;
			if (speckleMesh.textureCoordinates.Count > 0 && !hasValidUVs)
				Debug.LogWarning(
					$"Expected number of UV coordinates to equal vertices. Got {speckleMesh.TextureCoordinatesCount} expected {speckleMesh.VerticesCount}. \nID = {speckleMesh.id}");

			if (hasValidUVs)
			{
				data.uvs.Capacity += speckleMesh.TextureCoordinatesCount;
				for (var j = 0; j < speckleMesh.TextureCoordinatesCount; j++)
				{
					var (u, v) = speckleMesh.GetTextureCoordinate(j);
					data.uvs.Add(new Vector2((float)u, (float)v));
				}
			}
			else if (speckleMesh.bbox != null)
			{
				//Attempt to generate some crude UV coordinates using bbox
				////TODO this will be broken for submeshes
				data.uvs.AddRange(speckleMesh.bbox.GenerateUV(data.vertices));
			}

			// Convert vertex colors
			if (speckleMesh.colors != null)
			{
				// if (speckleMesh.colors.Count == speckleMesh.VerticesCount)
					// data.vertexColors.AddRange(speckleMesh.colors.Select(c => c.ToUnityColor()));
				// else if (speckleMesh.colors.Count != 0)
					// TODO what if only some submeshes have colors?
					// Debug.LogWarning(
						// $"{typeof(Mesh)} {speckleMesh.id} has invalid number of vertex {nameof(Mesh.colors)}. Expected 0 or {speckleMesh.VerticesCount}, got {speckleMesh.colors.Count}");
			}

			var tris = new List<int>();

			// Convert faces
			tris.Capacity += (int)(speckleMesh.faces.Count / 4f) * 3;

			for (var i = 0; i < speckleMesh.faces.Count; i += 4)
			{
				//We can safely assume all faces are triangles since we called TriangulateMesh
				tris.Add(speckleMesh.faces[i + 1] + indexOffset);
				tris.Add(speckleMesh.faces[i + 3] + indexOffset);
				tris.Add(speckleMesh.faces[i + 2] + indexOffset);
			}

			data.subMeshes.Add(tris);
		}

		
		// 	public static async UniTask<bool> ProcessMesh(ISpeckleMeshConverter converter, IReadOnlyCollection<Mesh> meshes, ref GameObject obj)
		// 	{
		// 		var filter = obj.GetComponent<MeshFilter>();
		//
		// 		if (filter == null)
		// 			filter = obj.AddComponent<MeshFilter>();
		//
		// 		if (IsRuntime)
		// 			filter.mesh = nativeMesh;
		// 		else
		// 			filter.sharedMesh = nativeMesh;
		//
		// 		if (converter.addMeshCollider)
		// 			filter.gameObject.AddComponent<MeshCollider>().sharedMesh = IsRuntime ? filter.mesh : filter.sharedMesh;
		//
		// 		if (converter.addMeshRenderer)
		// 			filter.gameObject.AddComponent<MeshRenderer>().sharedMaterials = nativeMaterials;
		//
		// 		var materials = new List<Material>(meshes.Count);
		//
		// 		var data = new MeshData
		// 		{
		// 			uvs = new List<Vector2>(),
		// 			vertices = new List<Vector3>(),
		// 			subMeshes = new List<List<int>>(),
		// 			vertexColors = new List<Color>()
		// 		};
		//
		// 		foreach (var speckleMesh in meshes)
		// 		{
		// 			if (speckleMesh.vertices.Count == 0 || speckleMesh.faces.Count == 0) continue;
		//
		// 			data.AddMesh(speckleMesh);
		// 			// Convert RenderMaterial
		// 			materials.Add(converter.useRenderMaterial ?
		// 				              GetMaterial(converter, speckleMesh["renderMaterial"] as RenderMaterial) :
		// 				              converter.defaultMaterial
		// 			);
		// 		}
		//
		// 		var nativeMaterials = materials.ToArray();
		//
		// 		var nativeMesh = new UnityEngine.Mesh
		// 		{
		// 			subMeshCount = data.subMeshes.Count
		// 		};
		//
		// 		nativeMesh.SetVertices(data.vertices);
		// 		nativeMesh.SetUVs(0, data.uvs);
		// 		nativeMesh.SetColors(data.vertexColors);
		//
		// 		var j = 0;
		// 		foreach (var subMeshTriangles in data.subMeshes)
		// 		{
		// 			nativeMesh.SetTriangles(subMeshTriangles, j);
		// 			j++;
		// 		}
		//
		// 		if (nativeMesh.vertices.Length >= ushort.MaxValue)
		// 			nativeMesh.indexFormat = IndexFormat.UInt32;
		//
		// 		nativeMesh.Optimize();
		// 		nativeMesh.RecalculateBounds();
		// 		nativeMesh.RecalculateNormals();
		// 		nativeMesh.RecalculateTangents();
		//
		// 		return obj;
		// 	}
		//
		
	}
}