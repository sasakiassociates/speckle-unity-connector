using System;
using System.Collections.Generic;

namespace Speckle.ConnectorUnity.Ops
{
	[Serializable]
	public class SpeckleStructure
	{

		public List<SpeckleLayer> layers;

		public SpeckleStructure() => layers = new List<SpeckleLayer>();

		public void Add(SpeckleLayer layer)
		{
			layers ??= new List<SpeckleLayer>();
			layers.Add(layer);
		}
	}
}