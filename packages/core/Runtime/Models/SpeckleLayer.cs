using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Speckle.ConnectorUnity.Models
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
			set => _parent = value;
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
		/// <param name="recursive"></param>
		public void ParentObjects(Transform t, bool recursive = false)
		{
			if (t == null)
			{
				return;
			}

			_parent = t;

			if (Data.Any())
			{
				Data.ForEach(x => x.transform.SetParent(_parent));
			}

			if (!recursive)
			{
				return;
			}

			foreach (var l in _layers)
			{
				l.ParentObjects(t, true);
			}
		}

		/// <summary>
		/// Sets the <see cref="Parent"/> object to the object hosted by this component
		/// </summary>
		public void ParentObjects()
		{
			ParentObjects(_parent != null ? _parent : transform);
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