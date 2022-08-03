using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{
	public class SpeckleLayer : MonoBehaviour
	{

		[SerializeField] Transform _parent;

		[SerializeField] List<GameObject> _data;

		[SerializeField] List<SpeckleLayer> _layers;

		/// <summary>
		///   Active parent for all layer objects
		/// </summary>
		public Transform Parent
		{
			get => _parent;
		}

		/// <summary>
		///   Converted object data within layer
		/// </summary>
		public List<GameObject> Data
		{
			get => _data.Valid() ? _data : new List<GameObject>();
		}

		/// <summary>
		///   Layer Name
		/// </summary>
		public string LayerName
		{
			get => name;
			set => name = value;
		}

		/// <summary>
		///   Nested Layers
		/// </summary>
		public List<SpeckleLayer> Layers
		{
			get => _layers.Valid() ? _layers : new List<SpeckleLayer>();
		}

		/// <summary>
		///   Set parent for all objects in a layer
		/// </summary>
		/// <param name="t"></param>
		public void SetObjectParent(Transform t)
		{
			if (t == null)
				return;

			_parent = t;

			if (Data.Any())
				Data.ForEach(x => x.transform.SetParent(_parent));
		}

		public void Add(SpeckleLayer layer)
		{
			_layers ??= new List<SpeckleLayer>();
			_layers.Add(layer);
		}

		public void Add(GameObject @object)
		{
			_data ??= new List<GameObject>();
			_data.Add(@object);
		}
	}
}