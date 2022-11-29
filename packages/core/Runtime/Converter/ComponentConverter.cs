using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Models;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Converter
{

	public abstract class ComponentConverter : ScriptableObject, IComponentConverter, IComponentConverterProcess
	{
		/// <summary>
		/// Simple data container of what the component supports
		/// </summary>
		[SerializeField] protected ComponentInfo _info;
		
		public ScriptableConverter parent { get; set; } 
		
		protected ConcurrentQueue<ConverterArgs> _queue;

		/// <summary>
		/// A container for managing the conversion settings
		/// </summary>
		public ScriptableConverterSettings settings { get; set; }

		/// <summary>
		/// Reports true if any items are in the queue
		/// </summary>
		public bool HasWorkToDo => _queue.Valid();

		/// <summary>
		/// Typical speckle conversion called from <seealso cref="ISpeckleConverter"/>
		/// This method is mainly used for setting up the gameobject and component data on the  main thread 
		/// </summary>
		/// <param name="base">The <see cref="Base"/> object to convert</param>
		/// <returns>The scene object with necessary component info</returns>
		public GameObject ToNative(Base @base)
		{
			if (TryConverterArgs(@base, out var args))
			{
				_queue.Enqueue(args);

				onQueueSizeChanged?.Invoke(_queue.Count);

				return args.unityObj.gameObject;
			}

			SpeckleUnity.Console.Warn("Object was not converted properly");
			return null;
		}

		/// <summary>
		/// Simple check to see if <see cref="Base"/> is supported by the converter
		/// </summary>
		/// <param name="type"></param>
		/// <returns>Returns true if casted type is supported</returns>
		public abstract bool CanConvertToNative(Base type);

		/// <summary>
		/// Simple check to see if <see cref="Component"/> is supported by the converter
		/// </summary>
		/// <param name="type"></param>
		/// <returns>Returns true if casted type is supported</returns>
		public abstract bool CanConvertToSpeckle(Component type);

		/// <summary>
		/// Actual conversion logic for processing speckle data into the necessary unity component(s)
		/// </summary>
		/// <param name="base">Speckle Object to convert</param>
		/// <param name="obj">Referenced scene object with component to send data to</param>
		public abstract void ToNativeConversion(Base @base, ref Component obj);

		/// <summary>
		/// Actual conversion logic for processing speckle data into the necessary unity component(s), but with the spice of async
		/// </summary>
		/// <param name="base">Speckle Object to convert</param>
		/// <param name="obj">Referenced scene object with component to send data to</param>
		public abstract UniTask ToNativeConversionAsync(Base @base, Component obj);

		/// <summary>
		/// Conversion logic to parse the unity components into a Speckle Object
		/// </summary>
		/// <param name="component">Component to convert</param>
		/// <returns>The <see cref="Base"/> processed</returns>
		public abstract Base ToSpeckle(Component component);

		/// <summary>
		/// The unity <see cref="Component"/> type targeted for conversion
		/// </summary>
		public abstract Type unity_type { get; }

		/// <summary>
		/// The <see cref="Base"/> speckle type targeted for conversion
		/// </summary>
		public abstract string speckle_type { get; }

		/// <summary>
		/// Event for notify queue updates during conversion
		/// </summary>
		public event UnityAction<int> onQueueSizeChanged;

		public virtual bool Equals(ComponentConverter other)
		{
			return other != null
			       && other.unity_type != null
			       && other.unity_type == unity_type
			       && other.speckle_type.Valid()
			       && other.speckle_type.Equals(speckle_type);
		}

		/// <summary>
		/// Follow up method for handling all conversion data
		/// Will process speckle data at specific rate set in <see cref="ScriptableConverterSettings.spawnSpeed"/>
		/// </summary>
		public async UniTask PostWorkAsync()
		{
			SpeckleUnity.Console.Log($"{nameof(PostWorkAsync)} for {name} with {_queue.Count}");

			try
			{
				if (_queue.Valid())
				{
					var chunk = new List<ConverterArgs>();
					while (_queue.TryDequeue(out var args))
					{
						chunk.Add(args);

						if (_queue.Count <= 0 || chunk.Count >= settings.spawnSpeed)
						{
							await UniTask.WhenAll(chunk.Select(x => ToNativeConversionAsync(x.speckleObj, x.unityObj)));

							chunk = new List<ConverterArgs>();

							onQueueSizeChanged?.Invoke(_queue.Count);

							// call for making sure the ui thread doesnt get blocked
							await UniTask.Yield();
						}
					}
				}
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				SpeckleUnity.Console.Log("Post Work Completed");
				onQueueSizeChanged?.Invoke(0);
			}
		}

		/// <summary>
		/// Protected method for handling game object creation
		/// </summary>
		/// <param name="base">Object to build off of</param>
		/// <param name="args">Output args to be added to the queue</param>
		/// <returns>Returns true if args are valid</returns>
		protected abstract bool TryConverterArgs(Base @base, out ConverterArgs args);

		protected virtual BaseBehaviour GetBaseType(GameObject obj)
		{
			if (obj == null) obj = new GameObject();

			var bb = obj.GetComponent<BaseBehaviour>();

			if (bb == null) bb = obj.AddComponent<BaseBehaviour>();

			return bb;
		}

		[Serializable]
		protected struct ComponentInfo
		{
			public string speckleTypeName;

			public string unityTypeName;

			public ComponentInfo(string unity, string speckle)
			{
				unityTypeName = unity;
				speckleTypeName = speckle;
			}
		}

	}

	public abstract class ComponentConverter<TBase, TComponent> : ComponentConverter
		where TComponent : Component
		where TBase : Base
	{

		public override string speckle_type => _info.speckleTypeName;

		/// <summary>
		/// <see cref="TComponent"/> targeted for conversion to <see cref="TBase"/>
		/// </summary>
		public override Type unity_type => typeof(TComponent);

		protected bool ValidObjects(Base @base, Component component, out TBase tBase, out TComponent tComp)
		{
			tBase = null;
			tComp = null;
			
			if (@base is TBase b && component is TComponent c)
			{
				tBase = b;
				tComp = c;
			}

			return tBase != null && tComp != null;
		}

		protected virtual void OnEnable()
		{
			_queue = new ConcurrentQueue<ConverterArgs>();
			_info = new ComponentInfo(typeof(TComponent).ToString(), Activator.CreateInstance<TBase>().speckle_type);
		}

		public override bool CanConvertToNative(Base type) => type != null && type.GetType() == typeof(TBase);

		public override bool CanConvertToSpeckle(Component type) => type != null && type.GetType() == typeof(TComponent);

		public abstract Base ConvertComponent(TComponent component);

		protected abstract void ConvertBase(TBase obj, ref TComponent instance);

		protected override BaseBehaviour GetBaseType(GameObject obj)
		{
			var comp = typeof(TComponent);

			if (obj != null && comp.IsSubclassOf(typeof(BaseBehaviour)) || comp == typeof(BaseBehaviour))
				return obj.GetComponent<TComponent>() as BaseBehaviour;

			return base.GetBaseType(obj);
		}

		public override void ToNativeConversion(Base @base, ref Component component)
		{
			if (ValidObjects(@base, component, out var converterObj, out var converterComp))
				ConvertBase(converterObj, ref converterComp);
		}

		public override async UniTask ToNativeConversionAsync(Base @base, Component component) => await UniTask.Create(() =>
		{
			ToNativeConversion(@base, ref component);
			return UniTask.CompletedTask;
		});

		protected override bool TryConverterArgs(Base @base, out ConverterArgs args)
		{
			args = null;
			if (CanConvertToNative(@base) && @base is TBase compBase)
			{
				var component = CreateComponentInstance();

				GetBaseType(component.gameObject).Store(compBase);

				args = new ConverterArgs(compBase, component);
			}
			else
			{
				SpeckleUnity.Console.Warn($"{@base.speckle_type} somehow ended up in the wrong converter!\n{this}");
			}

			return args != null;
		}

		public override Base ToSpeckle(Component component) => CanConvertToSpeckle(component) ? ConvertComponent((TComponent)component) : null;

		public TComponent CreateComponentInstance(string n = null) => GetBaseType(
			new GameObject(n.Valid() ? n : speckle_type).AddComponent<TComponent>().gameObject).GetComponent<TComponent>();

	}

}