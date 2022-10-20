using System.Collections.Generic;
using Speckle.ConnectorUnity.Models;
using Speckle.Core.Models;
using UnityEngine;
using Mesh = Objects.Geometry.Mesh;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = "UnityConverter", menuName = SpeckleUnity.NAMESPACE + "Defualt Converter", order = -1)]
	public class ConverterUnity : ScriptableSpeckleConverter
	{

		public override List<ComponentConverter> StandardConverters() => new List<ComponentConverter>
		{
			CreateInstance<MeshComponentConverter>(),
			CreateInstance<PolylineComponentConverter>(),
			CreateInstance<PointComponentConverter>(),
			CreateInstance<PointCloudComponentConverter>(),
			CreateInstance<View3DComponentConverter>(),
			CreateInstance<BrepComponentConverter>()
		};

		public override object ConvertToNative(Base @base) => base.ConvertToNative(@base) ?? TryConvertDefault(@base);

		GameObject TryConvertDefault(Base @base)
		{
			if (@base["displayValue"] is Mesh mesh)
			{
				Debug.Log("Handling Singluar Display Value");

				var res = ConvertToNative(mesh) as Component;
				res.gameObject.AddComponent<BaseBehaviour>().Store(@base);

				return res.gameObject;
			}

			if (@base["displayValue"] is IEnumerable<Base> bs)
			{
				Debug.Log("Handling List of Display Value");

				var displayValues = new GameObject("DisplayValues");
				displayValues.AddComponent<BaseBehaviour>().Store(@base);

				foreach (var b in bs)
					if (b is Mesh displayMesh)
					{
						var obj = ConvertToNative(displayMesh) as GameObject;
						if (obj != null)
							obj.transform.SetParent(displayValues.transform);
					}

				return displayValues;
			}

			return null;
		}
	}

}