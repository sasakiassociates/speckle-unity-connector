using System;
using System.Collections.Generic;
using UnityEngine;

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

		public List<GameObject> GetObjects(List<SpeckleLayer> objs)
		{
			var res = new List<GameObject>();

			foreach (var layer in objs)
			{
				if (layer.Data.Valid())
					res.AddRange(layer.Data);

				if (!layer.Layers.Valid())
					continue;

				var kids = GetObjects(layer.Layers);

				if (kids.Valid())
					res.AddRange(kids);
			}

			return res;
		}

	}
}