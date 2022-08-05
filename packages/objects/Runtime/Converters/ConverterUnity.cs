using System.Collections.Generic;
using Speckle.ConnectorUnity.Models;
using Speckle.ConnectorUnity.Mono;
using Speckle.Core.Models;
using UnityEngine;
using Mesh = Objects.Geometry.Mesh;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = "UnityConverter", menuName = "Speckle/Speckle Unity Converter", order = -1)]
	public class ConverterUnity : ScriptableSpeckleConverter
	{
		protected override void OnEnable()
		{
			base.OnEnable();

			if (defaultConverter == null)
				SetDefaultConverter(CreateInstance<BaseConverter>());
		}

		public override List<ComponentConverter> StandardConverters() => new List<ComponentConverter>
		{
			CreateInstance<MeshConverter>(),
			CreateInstance<PolylineConverter>(),
			CreateInstance<PointConverter>(),
			CreateInstance<PointCloudConverter>(),
			CreateInstance<View3DConverter>(),
			CreateInstance<BrepConverter>()
		};

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