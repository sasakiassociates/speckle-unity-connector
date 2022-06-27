using System.Collections.Generic;
using Speckle.ConnectorUnity.Mono;
using Speckle.Core.Models;
using UnityEngine;
using Mesh = Objects.Geometry.Mesh;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = "UnityConverter", menuName = "Speckle/Speckle Unity Converter", order = -1)]
	public class ConverterUnity : ScriptableSpeckleConverter
	{
		[SerializeField] ComponentConverterBase defaultConverter;

		void OnEnable()
		{
			if (defaultConverter == null)
				defaultConverter = CreateInstance<ComponentConverterBase>();

			if (!converters.Valid())
				converters = new List<ComponentConverter>
				{
					CreateInstance<ComponentConverterMesh>(),
					CreateInstance<ComponentConverterPolyline>(),
					CreateInstance<ComponentConverterPoint>(),
					CreateInstance<ComponentConverterPointCloud>(),
					CreateInstance<ComponentConverterView3D>(),
					CreateInstance<ComponentConverterBrep>()
				};
		}

		public override object ConvertToNative(Base @base)
		{
			var res = base.ConvertToNative(@base) ?? TryConvertDefault(@base);

			return res;
		}

		GameObject TryConvertDefault(Base @base)
		{
			if (@base["displayValue"] is Mesh mesh)
			{
				Debug.Log("Handling Singluar Display Value");

				var go = new GameObject(@base.speckle_type);
				go.AddComponent<BaseBehaviour>().properties = new SpeckleProperties
					{ Data = @base.FetchProps() };

				var res = ConvertToNative(mesh) as Component;
				res.transform.SetParent(go.transform);
				return res.gameObject;
			}

			if (@base["displayValue"] is IEnumerable<Base> bs)
			{
				Debug.Log("Handling List of Display Value");

				var go = new GameObject(@base.speckle_type);
				go.AddComponent<BaseBehaviour>().properties = new SpeckleProperties
					{ Data = @base.FetchProps() };

				var displayValues = new GameObject("DisplayValues");
				displayValues.transform.SetParent(go.transform);

				foreach (var b in bs)
					if (b is Mesh displayMesh)
					{
						var obj = ConvertToNative(displayMesh) as GameObject;
						if (obj != null)
							obj.transform.SetParent(displayValues.transform);
					}

				return go;
			}

			return null;
		}
	}

}