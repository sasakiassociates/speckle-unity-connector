using System.Collections.Generic;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	public interface IWantContextObj
	{
		public List<ApplicationPlaceholderObject> contextObjects { get; set; }
	}

	public interface ISpeckleMeshConverter : IWantContextObj
	{
		public bool addMeshCollider { get; }
		public bool addMeshRenderer { get; }
		public bool recenterTransform { get; }
		public bool useRenderMaterial { get; }
		public Material defaultMaterial { get; }
	}
}