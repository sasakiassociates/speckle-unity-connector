using System;
using System.Collections.Generic;
using UnityEngine;

namespace Speckle.ConnectorUnity.Models
{
	[Serializable]
	public class SpeckleObjectHierarchy
	{

		[SerializeField] SpeckleLayer _defaultLayer;

		public Transform parent;

		public List<SpeckleLayer> layers;

		public SpeckleLayer DefaultLayer => _defaultLayer;

		public SpeckleObjectHierarchy() => layers = new List<SpeckleLayer>();

		public SpeckleObjectHierarchy(Transform parent)
		{
			layers = new List<SpeckleLayer>();
			this.parent = parent;
		}

		public void SetDefault(SpeckleLayer layer)
		{
			_defaultLayer = layer;
			layers.Add(_defaultLayer);
		}

		public void Add(SpeckleLayer layer)
		{
			layers ??= new List<SpeckleLayer>();
			layers.Add(layer);
		}

		public void ParentAllObjects()
		{
			foreach (var obj in GetObjects())
			{
				obj.transform.SetParent(parent, true);
			}
		}

		public List<GameObject> GetObjects() => GetObjects(layers);

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