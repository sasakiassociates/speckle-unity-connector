using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	/// <summary>
	/// A scene object for helping with conversions
	/// </summary>
	[AddComponentMenu(SpeckleUnity.NAMESPACE + "/Converter Crew")]
	public class ComponentConverterCrew : MonoBehaviour
	{
		[SerializeField] int _chunkSize = 20;

		[SerializeField] ComponentConverter _converter;

		public int chunkSize
		{
			get => _chunkSize;
			set => _chunkSize = value;
		}

		ConcurrentQueue<ConverterArgs> _queue;

		public bool HasWorkToDo => _queue.Valid();

		public bool Equals(ComponentConverter other) => _converter != null && _converter.Equals(other);

		/// <summary>
		/// Starts the conversion process for all objects in a parallel thread. 
		/// </summary>
		/// <returns></returns>
		public async UniTask PostWorkAsync()
		{
			SpeckleUnity.Console.Log($"{nameof(PostWorkAsync)} for {name} with {_queue.Count}");
			try
			{
				var chunk = new List<ConverterArgs>();

				while (_queue.TryDequeue(out var args))
				{
					Debug.Log($"Deque called {args.speckleObj}");

					chunk.Add(args);

					if (_queue.Count <= 0 || chunk.Count >= chunkSize)
					{
						Debug.Log("Working through chunk");
					
						await UniTask.WhenAll(chunk.Select(x => _converter.ToNativeConversionAsync(x.speckleObj, x.unityObj)));
						
						Debug.Log("Chunk complete");
						
						chunk = new List<ConverterArgs>();
					}
				}

				Debug.Log($"deque done! {_queue.Count}");
			}

			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
		}

		public void Add(ConverterArgs args)
		{
			_queue.Enqueue(args);
			SpeckleUnity.Console.Log($"Component Id: {args.unityObj.GetInstanceID()}\n"
			                         + $"Object Id: {args.unityObj.gameObject.GetInstanceID()}\n"
			                         + $"Base Id: {args.speckleObj.id}");
		}

		public void Add(Base @base, Component component) => Add(new ConverterArgs(@base, component));

		public void Initialize(ComponentConverter converter)
		{
			if (converter == null)
			{
				SpeckleUnity.Console.Warn($"Cannot complete {nameof(Initialize)} with a null component converter");
				return;
			}

			_converter = converter;
			_queue = new ConcurrentQueue<ConverterArgs>();
			name = $"{(_converter.name.Valid() ? _converter.name : _converter.GetType().ToString().Split('.').LastOrDefault())} Crew";
		}

	}
}