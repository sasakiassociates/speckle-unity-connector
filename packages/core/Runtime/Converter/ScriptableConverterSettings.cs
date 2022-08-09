using System;

namespace Speckle.ConnectorUnity.Converter
{
	[Serializable]
	public class ScriptableConverterSettings
	{
		public ConverterStyle style;
	}

	public enum ConverterStyle
	{
		Direct,
		Queue
	}
}