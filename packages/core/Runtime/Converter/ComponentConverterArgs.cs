using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

	public class ConverterArgs
	{
		public Component unityObj;
		public readonly Base speckleObj;

		public ConverterArgs(Base speckleObj, Component unityObj)
		{
			this.speckleObj = speckleObj;
			this.unityObj = unityObj;
		}
	}

	public class ComponentConverterArgs
	{
		public readonly int componentInstanceId;
		public readonly string baseId;
		public readonly GameObject targetObject;

		public ComponentConverterArgs(int componentInstanceId, GameObject targetObject, string baseId)
		{
			this.componentInstanceId = componentInstanceId;
			this.targetObject = targetObject;
			this.baseId = baseId;
		}
	}
}