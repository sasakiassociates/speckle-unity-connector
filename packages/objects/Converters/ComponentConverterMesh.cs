using System.Collections.Generic;
using Speckle.Core.Models;
using UnityEngine;
using Mesh = Objects.Geometry.Mesh;

namespace Speckle.ConnectorUnity.Converter
{

	[CreateAssetMenu(fileName = nameof(ComponentConverterMesh), menuName = "Speckle/Converters/Create Mesh Converter")]
	public class ComponentConverterMesh : ComponentConverter<Mesh, MeshFilter>, ISpeckleMeshConverter
	{

		[SerializeField] bool _addMeshCollider;

		[SerializeField] bool _addRenderer = true;

		[SerializeField] bool _recenterTransform = true;

		[SerializeField] bool _useRenderMaterial;

		[SerializeField] Material _defaultMaterial;

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

		public List<ApplicationPlaceholderObject> contextObjects { get; set; }

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

		protected override GameObject ConvertBase(Mesh @base)
		{
			// convert the mesh data
			return this.MeshToNative(new[] { @base }, BuildGo().gameObject);
		}

		// copied from repo
		//TODO: support multiple filters?
		protected override Base ConvertComponent(MeshFilter component) => this.MeshToSpeckle(component);
	}

}