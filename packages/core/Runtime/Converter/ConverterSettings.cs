using System;

namespace Speckle.ConnectorUnity.Converter
{
	/// <summary>
	/// A data object for the <see cref="ScriptableConverter"/>
	/// </summary>
	[Serializable]
	public class ConverterSettings
	{
		/// <summary>
		/// The type of conversion to use
		/// </summary>
		public ConversionStyle style;
		
		/// <summary>
		/// The speed for working through the queue items, if that conversion style is selected 
		/// </summary>
		public int queueSpeed = 20;

		/// <summary>
		/// The amount of objects allowed to be created before syncing with unity's main thread
		/// </summary>
		public int batchSize = 100;
		
		/// <summary>
		/// A way of telling the converter how to handle the process of converting
		/// </summary>
		public enum ConversionStyle
		{
			/// <summary>
			/// Will force each object to be created and decorated with the necessary properties one by one 
			/// </summary>
			Sync,
			/// <summary>
			/// Will create a new object for each item being converted and stores the item's data in a queue to be handled after
			/// This options is mainly designed to not block Unity's main thread 
			/// </summary>
			Queue
		}	
		
	}

	
}