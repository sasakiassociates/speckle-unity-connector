using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Speckle.ConnectorUnity.Mono
{

	/// <summary>
	///   This class gets attached to GOs and is used to store Speckle's metadata when sending / receiving
	/// </summary>
	[Serializable]
	public class SpeckleProperties
	{
		// TODO: handle rules with this in Editor vs Runtime
		[SerializeField] [HideInInspector] string _jsonString = "";

		HashSet<string> _excludedProps;

		public HashSet<string> excludedProps
		{
			get
			{
				return _excludedProps ??= new HashSet<string>(
					typeof(Base).GetProperties(
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
					).Select(x => x.Name));
			}
			private set => _excludedProps = value;
		}

		ObservableConcurrentDictionary<string, object> _observableConcurrentDict;

		public IDictionary<string, object> Data
		{
			get => _observableConcurrentDict;
			set
			{
				((ICollection<KeyValuePair<string, object>>)_observableConcurrentDict).Clear();

				foreach (var kvp in value) _observableConcurrentDict.Add(kvp.Key, kvp.Value);
			}
		}

		public async UniTask Store(Base @base, HashSet<string> props)
		{
			if (props != null) excludedProps = props;

			await Store(@base);
		}

		public async UniTask Store(Base @base)
		{
			var watch = Stopwatch.StartNew();

			await UniTask.Create(() =>
			{
				_jsonString = Operations.Serialize(@base);
				return UniTask.CompletedTask;
			});

			Debug.Log($"Step 1: Serialize to string with Operations-{watch.Elapsed}");

			await UniTask.Yield();
			watch.Restart();

			await UniTask.Create(() =>
			{
				Data = @base.GetMembers().Where(x => !excludedProps.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
				return UniTask.CompletedTask;
			});

			Debug.Log($"Step 2: Serialize dictionary of props-{watch.Elapsed}");
			await UniTask.Yield();
			watch.Restart();

			await UniTask.Create(() =>
			{
				// _speckleProps = new SpeckleData(Data);
				return UniTask.CompletedTask;
			});

			Debug.Log($"Step 3: Serializing through class-{watch.Elapsed}");
			await UniTask.Yield();
			watch.Restart();

			await UniTask.Create(() =>
			{
				var speckleData = Operations.Deserialize(_jsonString);

				Debug.Log(speckleData.speckle_type);
				Debug.Log(speckleData.id);

				return UniTask.CompletedTask;
			});

			Debug.Log($"Step 4: DeSerializing string-{watch.Elapsed}");
			await UniTask.Yield();
			watch.Stop();
		}

		/// <summary>
		/// Serializes the stored properties in <see cref="Data"/>
		/// </summary>
		/// <returns></returns>
		public UniTask Serialize()
		{
			_jsonString = Operations.Serialize(new SpeckleData(Data));
			return UniTask.CompletedTask;
		}

		/// <summary>
		/// Saves new data into <see cref="Data"/> and creates a new serialized json 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public UniTask Serialize(IDictionary<string, object> data)
		{
			var tempData = new SpeckleData(data);
			Data = tempData.GetMembers().Where(x => !excludedProps.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
			return Serialize();
		}

		public UniTask SimpleStore(Base @base)
		{
			_jsonString = Operations.Serialize(@base);
			Data = @base.GetMembers().Where(x => !excludedProps.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
			return UniTask.CompletedTask;
		}

		public SpeckleProperties()
		{
			_observableConcurrentDict["hh"] = 10;
			_observableConcurrentDict = new ObservableConcurrentDictionary<string, object>();
			_observableConcurrentDict.CollectionChanged += (_, args) => OnCollectionChange?.Invoke(args);
			// hasChanged = true;
		}

		public event UnityAction<NotifyCollectionChangedEventArgs> OnCollectionChange;

		[Serializable]
		internal sealed class SpeckleData : Base
		{
			public SpeckleData(IDictionary<string, object> data)
			{
				foreach (var v in data) this[v.Key] = v.Value;
			}
		}

	}
}