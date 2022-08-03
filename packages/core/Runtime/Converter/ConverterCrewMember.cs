using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Converter
{
	/// <summary>
	/// A class for helping with conversions
	/// </summary>
	[AddComponentMenu(SpeckleUnity.NAMESPACE + "/Converter Crew")]
	public class ConverterCrewMember : MonoBehaviour
	{
		[SerializeField] List<GameObject> _objects;
		[SerializeField] ComponentConverter _converter;

		public event UnityAction<ConverterCrewArgs> OnListUpdate;

		public void Add(GameObject obj)
		{ }

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

			// TODO: look into what should handle this
			converter.OnObjectConverted += args =>
			{
				SpeckleUnity.Console.Log($"Component Id: {args.componentInstanceId} Object Id: {args.objectInstanceId}");
			};

			_converter = converter;
		}

		public class ConverterCrewArgs
		{
			public int index;
			public int count;
			public string time;
		}
	}
}