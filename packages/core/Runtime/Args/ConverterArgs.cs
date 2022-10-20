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

}