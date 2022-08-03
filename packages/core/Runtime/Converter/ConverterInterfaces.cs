using System;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

	[Serializable]
	public class ScriptableSpeckleConverterSettings
	{
		public ConverterStyle style;
	}
	

	public enum ConverterStyle
	{
		Direct,
		Queue
	}

	public interface IComponentConverter
	{

		public string speckle_type { get; }

		public Type unity_type { get; }

		public bool CanConvertToNative(Base type);
		public bool CanConvertToSpeckle(Component type);

		public GameObject ToNative(Base @base);
		public Base ToSpeckle(Component component);
	}
}