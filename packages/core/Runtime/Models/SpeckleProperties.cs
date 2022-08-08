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

namespace Speckle.ConnectorUnity.Models
{
	/// <summary>
	///   This class gets attached to GOs and is used to store Speckle's metadata when sending / receiving
	/// </summary>
	[Serializable]
	public class SpeckleProperties
	{
		public SpeckleProperties()
		{
			_observableConcurrentDict = new ObservableConcurrentDictionary<string, object>();
			_observableConcurrentDict.CollectionChanged += (_, args) => OnCollectionChange?.Invoke(args);
		}

		[SerializeField] [HideInInspector] string _jsonString = "";

		public event UnityAction<NotifyCollectionChangedEventArgs> OnCollectionChange;

		HashSet<string> _excludedProps;

		public HashSet<string> excludedProps
		{
			get
			{
				return _excludedProps ??= new HashSet<string>(
					typeof(Base).GetProperties().Select(x => x.Name));
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

		public UniTask Serialize(Base @base, HashSet<string> props)
		{
			if (props != null) excludedProps = props;
			return Serialize(@base);
		}

		public UniTask Serialize(Base @base)
		{
			Data = @base.GetMembers().Where(x => !excludedProps.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
			_jsonString = Operations.Serialize(@base);
			return UniTask.CompletedTask;
		}

		public UniTask Serialize(IDictionary<string, object> objectProps)
		{
			Data = objectProps;
			_jsonString = Operations.Serialize(new BasePropsWrapper(Data));
			return UniTask.CompletedTask;
		}

		/// <summary>
		/// Serializes the stored properties in <see cref="Data"/>
		/// </summary>
		/// <returns></returns>
		public UniTask Serialize()
		{
			var tData = new Base();

			foreach (var d in Data)
				tData[d.Key] = d.Value;

			_jsonString = Operations.Serialize(new BasePropsWrapper(Data));
			return UniTask.CompletedTask;
		}

		public async UniTask Test_Serialize(Base @base)
		{
			var watch = Stopwatch.StartNew();

			await UniTask.Create(() =>
			{
				_jsonString = Operations.Serialize(@base);
				return UniTask.CompletedTask;
			});

			SpeckleUnity.Console.Log($"Step 1: Serialize to string with Operations-{watch.Elapsed}");

			await UniTask.Yield();
			watch.Restart();

			await UniTask.Create(() =>
			{
				Data = @base.GetMembers().Where(x => !excludedProps.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
				return UniTask.CompletedTask;
			});

			SpeckleUnity.Console.Log($"Step 2: Serialize dictionary of props-{watch.Elapsed}");
			await UniTask.Yield();
			watch.Restart();

			await UniTask.Create(() =>
			{
				var speckleData = Operations.Deserialize(_jsonString);

				SpeckleUnity.Console.Log(speckleData.speckle_type);
				SpeckleUnity.Console.Log(speckleData.id);

				return UniTask.CompletedTask;
			});

			SpeckleUnity.Console.Log($"Step 4: DeSerializing string-{watch.Elapsed}");
			await UniTask.Yield();
			watch.Stop();
		}

		[Serializable]
		internal sealed class BasePropsWrapper : Base
		{
			public BasePropsWrapper(IDictionary<string, object> data)
			{
				foreach (var v in data) this[v.Key] = v.Value;
			}
		}

	}
}