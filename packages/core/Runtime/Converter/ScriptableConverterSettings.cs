using System;

namespace Speckle.ConnectorUnity.Converter
{
	[Serializable]
	public class ScriptableConverterSettings
	{
		public ConverterStyle style;
		public int spawnSpeed = 20;
	}

	public enum ConverterStyle
	{
		Direct,
		Queue
	}
}