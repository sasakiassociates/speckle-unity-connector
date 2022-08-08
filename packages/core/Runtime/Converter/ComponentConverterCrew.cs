using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	/// <summary>
	/// A class for helping with conversions
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

		Queue<ConverterArgs> _queue;

		Queue<ConverterArgs> queue
		{
			get
			{
				_queue ??= new Queue<ConverterArgs>();
				return _queue;
			}
		}

		public bool HasWorkToDo => _queue.Valid();

		public bool Equals(ComponentConverter other) => _converter != null && _converter.Equals(other);

		/// <summary>
		/// Starts the conversion process for all objects in crew. 
		/// </summary>
		/// <returns></returns>
		public UniTask PostWork()
		{
			if (!HasWorkToDo) return UniTask.CompletedTask;

			SpeckleUnity.Console.Log($"{nameof(PostWork)} for {name} with {queue.Count}");
			try
			{
				lock (_queue)
				{
					foreach (var item in queue)
						_converter.ToNativeConversion(item.@base, ref item.component);
				}
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}

			return UniTask.CompletedTask;
		}

		/// <summary>
		/// Starts the conversion process for all objects in a parallel thread. 
		/// </summary>
		/// <returns></returns>
		public async UniTask PostWorkAsync()
		{
			SpeckleUnity.Console.Log($"{nameof(PostWorkAsync)} for {name} with {queue.Count}");
			try
			{
				while (_queue.Count < 0)
				{
					var chunk = new ConverterArgs[chunkSize];

					lock (_queue)
					{
						for (int i = 0, c = 0; c < chunk.Length && i < _queue.Count; i++)
						{
							if (_queue.TryDequeue(out var res))
							{
								Debug.Log($"Is res valid? {res}");
								chunk[i] = res;
								c++;
							}
						}
					}

					await UniTask.WhenAll(chunk.Select(x => _converter.ToNativeConversionAsync(x.@base, x.component)));
				}
			}

			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
		}

		public void Add(ConverterArgs args)
		{
			queue.Enqueue(args);
			SpeckleUnity.Console.Log($"Component Id: {args.component.GetInstanceID()}\n"
			                         + $"Object Id: {args.component.gameObject.GetInstanceID()}\n"
			                         + $"Base Id: {args.@base.id}");
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
			name = $"{(_converter.name.Valid() ? _converter.name : _converter.GetType().ToString().Split('.').LastOrDefault())} Crew";
		}

	}
}