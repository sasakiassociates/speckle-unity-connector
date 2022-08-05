using Speckle.ConnectorUnity.Models;
using Speckle.ConnectorUnity.Mono;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

	[CreateAssetMenu(fileName = nameof(BaseConverter), menuName = SpeckleUnity.Categories.CONVERTERS + "Create Base Converter")]
	public class BaseConverter : ComponentConverter<Base, BaseBehaviour_v2>
	{

		protected override void ConvertBase(Base @base, ref BaseBehaviour_v2 instance)
		{
			// if (@base["displayValue"] is Mesh mesh)
			// {
			// 	Debug.Log("Handling Singluar Display Value");
			//
			// 	var go = new GameObject(@base.speckle_type);
			// 	go.AddComponent<BaseBehaviour>().properties = new SpeckleProperties
			// 		{ Data = @base.FetchProps() };
			//
			// 	var res = ConvertToNative(mesh) as Component;
			// 	res.transform.SetParent(go.transform);
			// 	return res.gameObject;
			// }
			//
			// if (@base["displayValue"] is IEnumerable<Base> bs)
			// {
			// 	Debug.Log("Handling List of Display Value");
			//
			// 	var go = new GameObject(@base.speckle_type);
			// 	go.AddComponent<BaseBehaviour>().properties = new SpeckleProperties
			// 		{ Data = @base.FetchProps() };
			//
			// 	var displayValues = new GameObject("DisplayValues");
			// 	displayValues.transform.SetParent(go.transform);
			//
			// 	foreach (var b in bs)
			// 		if (b is Mesh displayMesh)
			// 		{
			// 			var obj = ConvertToNative(displayMesh) as GameObject;
			// 			if (obj != null)
			// 				obj.transform.SetParent(displayValues.transform);
			// 		}
			// 	return go;
			// }

			// Debug.LogWarning($"Skipping {@base.GetType()} {@base.id} - Not supported type");
			// return null;
			SpeckleUnity.Console.Log(name + "does not support converting yet");
		}

		protected override Base ConvertComponent(BaseBehaviour_v2 component)
		{
			SpeckleUnity.Console.Log(name + "does not support converting yet");
			return null;
		}
	}
}