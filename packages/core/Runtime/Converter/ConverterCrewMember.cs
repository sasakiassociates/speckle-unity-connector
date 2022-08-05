using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Converter
{
	public readonly struct ConverterCrewArgs
	{
		public readonly int index;
		public readonly int count;
		public readonly string time;

		public ConverterCrewArgs(int index, int count, string time)
		{
			this.index = index;
			this.count = count;
			this.time = time;
		}
	}

	/// <summary>
	/// A class for helping with conversions
	/// </summary>
	[AddComponentMenu(SpeckleUnity.NAMESPACE + "/Converter Crew")]
	public class ConverterCrewMember : MonoBehaviour
	{
		Queue<ComponentConverterArgs> queue { get; set; }

		[SerializeField] ComponentConverter _converter;

		public event UnityAction<ConverterCrewArgs> OnListUpdate;

		public bool Equals(ComponentConverter other) => _converter != null && _converter.Equals(other);

		public UniTask PostWork()
		{
			return UniTask.CompletedTask;
		}

		public void Initialize(ComponentConverter converter)
		{
			if (converter == null)
			{
				SpeckleUnity.Console.Warn($"Cannot complete {nameof(Initialize)} with a null component converter");
				return;
			}

			_converter = converter;
			queue ??= new Queue<ComponentConverterArgs>();

			// TODO: look into what should handle this
			_converter.OnObjectConverted += args =>
			{
				queue.Enqueue(args);
				SpeckleUnity.Console.Log($"Component Id: {args.componentInstanceId}\n"
				                         + $"Object Id: {args.targetObject.GetInstanceID()}\n"
				                         + $"Base Id: {args.baseId}");
			};
		}

	}
}