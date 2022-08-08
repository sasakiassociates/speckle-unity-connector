using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

	public class ConverterArgs
	{
		public Component component;
		public readonly Base @base;

		public ConverterArgs(Base @base, Component component)
		{
			this.@base = @base;
			this.component = component;
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