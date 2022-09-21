using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.Core.Models;
using UnityEngine;
using Mesh = Objects.Geometry.Mesh;
using UN = UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

	[CreateAssetMenu(fileName = nameof(MeshComponentConverter), menuName = SpeckleUnity.Categories.CONVERTERS + "Create Mesh Converter")]
	public class MeshComponentConverter : ComponentConverter<Mesh, MeshFilter>, ISpeckleMeshConverter
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

		public List<ApplicationObject> contextObjects { get; set; }

		public bool addMeshCollider => _addMeshCollider;

		public bool addMeshRenderer => _addRenderer;

		public bool recenterTransform => _recenterTransform;

		public bool useRenderMaterial => _useRenderMaterial;

		public bool combineMeshes => _combineMeshes;

		public Material defaultMaterial => _defaultMaterial;

		protected override void OnEnable()
		{
			base.OnEnable();

			if (_defaultMaterial == null) _defaultMaterial = new Material(Shader.Find("Standard"));
		}

		public override UniTask ToNativeConversionAsync(Base @base, Component component) => ValidObjects(@base, component, out var b, out var c)
			? this.MeshToNativeAsync(b, c.gameObject) : UniTask.CompletedTask;

		protected override void ConvertBase(Mesh @base, ref MeshFilter instance) => this.MeshToNative(@base, instance.gameObject);

		protected override Base ConvertComponent(MeshFilter component) => this.MeshToSpeckle(component);

	}
}