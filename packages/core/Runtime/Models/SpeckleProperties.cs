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
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Speckle.ConnectorUnity.Mono
{
	public interface IBase : IBaseDynamic
	{
		public UniTask Store(Base @base);
	}

	public interface IBaseDynamic
	{
		public object this[string key] { get; set; }

		public HashSet<string> excluded { get; }

		SpeckleProperties props { get; }

		// public Dictionary<string, object> props { get; }
	}

	public interface IBaseDynamite
	{
		public Object this[string key] { get; set; }
	}

	/// <summary>
	///   This class gets attached to GOs and is used to store Speckle's metadata when sending / receiving
	/// </summary>
	[Serializable]
	public class SpeckleProperties
	{

		// TODO: handle rules with this in Editor vs Runtime
		[SerializeField] [HideInInspector] string _jsonString = "";

		ObservableConcurrentDictionary<string, object> _observableConcurrentDict;
		Dictionary<string, object> _dict;

		public bool hasChanged { get; private set; }

		[SerializeField] internal SpeckleData _speckleProps;

		HashSet<string> _propsHash;

		public HashSet<string> excludedProps
		{
			get
			{
				return _propsHash ??= new HashSet<string>(
					typeof(Base).GetProperties(
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
					).Select(x => x.Name));
			}
			private set => _propsHash = value;
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
				_speckleProps = new SpeckleData(Data);
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

		public object this[string key]
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public SpeckleProperties()
		{
			_observableConcurrentDict = new ObservableConcurrentDictionary<string, object>();
			_observableConcurrentDict.CollectionChanged += CollectionChangeHandler;
			_dict = new Dictionary<string, object>();

			hasChanged = true;
		}

		public IDictionary<string, object> Data
		{
			get => _observableConcurrentDict;
			set
			{
				_dict.Clear();
				((ICollection<KeyValuePair<string, object>>)_observableConcurrentDict).Clear();

				foreach (var kvp in value) _observableConcurrentDict.Add(kvp.Key, kvp.Value);
				foreach (var kvp in value) _dict.Add(kvp.Key, kvp.Value);
			}
		}

		public void OnBeforeSerialize()
		{
			if (!hasChanged) return;

			_jsonString = Operations.Serialize(new SpeckleData(Data));
			hasChanged = false;
		}

		public void OnAfterDeserialize()
		{
			var speckleData = Operations.Deserialize(_jsonString);
			Data = speckleData.GetMembers();
			hasChanged = false;
		}

		void CollectionChangeHandler(object sender, NotifyCollectionChangedEventArgs e)
		{
			hasChanged = true;
		}

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