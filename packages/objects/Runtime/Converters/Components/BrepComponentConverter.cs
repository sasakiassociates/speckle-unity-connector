using System.Collections.Generic;
using Objects.Geometry;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = nameof(BrepComponentConverter), menuName = SpeckleUnity.Categories.CONVERTERS + "Create Brep Converter")]
	public class BrepComponentConverter : ComponentConverter<Brep, SpeckleBrep>, ISpeckleMeshConverter
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

		public Material defaultMaterial
		{
			get => _defaultMaterial;
		}

		public bool combineMeshes
		{
			get => _combineMeshes;
		}

		protected override void ConvertBase(Brep obj, ref SpeckleBrep instance) => this.MeshToNative(obj.displayValue, instance.gameObject);

		public override Base ConvertComponent(SpeckleBrep component) => this.MeshToSpeckle(component.mesh);

	}
}