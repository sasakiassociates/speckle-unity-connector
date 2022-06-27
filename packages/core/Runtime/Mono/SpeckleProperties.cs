using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Speckle.Core.Api;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Mono
{
	/// <summary>
	///   This class gets attached to GOs and is used to store Speckle's metadata when sending / receiving
	/// </summary>
	[Serializable]
	public class SpeckleProperties
		// ISerializationCallbackReceiver
	{

		[SerializeField] [HideInInspector]
		string _serializedData = "";

		ObservableConcurrentDictionary<string, object> _data;

		bool _hasChanged;

		public SpeckleProperties()
		{
			_data = new ObservableConcurrentDictionary<string, object>();
			_data.CollectionChanged += CollectionChangeHandler;
			_hasChanged = true;
		}

		public IDictionary<string, object> Data
		{
			get => _data;
			set
			{
				((ICollection<KeyValuePair<string, object>>)_data).Clear();

				foreach (var kvp in value) _data.Add(kvp.Key, kvp.Value);
			}
		}

		public void OnBeforeSerialize()
		{
			if (!_hasChanged) return;

			_serializedData = Operations.Serialize(new SpeckleData(Data));
			_hasChanged = false;
		}

		public void OnAfterDeserialize()
		{
			var speckleData = Operations.Deserialize(_serializedData);
			Data = speckleData.GetMembers();
			_hasChanged = false;
		}

		public void Store(Base @base, HashSet<string> excludedProps)
		{
			_serializedData = Operations.Serialize(@base);
			Data = @base.GetMembers().Where(x => !excludedProps.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
		}

		void CollectionChangeHandler(object sender, NotifyCollectionChangedEventArgs e)
		{
			_hasChanged = true;
		}

		[Serializable]
		class SpeckleData : Base
		{
			public SpeckleData(IDictionary<string, object> data)
			{
				foreach (var v in data) this[v.Key] = v.Value;
			}
		}
	}
}