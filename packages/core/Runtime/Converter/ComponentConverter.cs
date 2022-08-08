using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Models;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Converter
{

	public abstract class ComponentConverter : ScriptableObject, IComponentConverter
	{
		[SerializeField] protected ComponentInfo _info;

		[SerializeField, HideInInspector] ComponentConverterCrew _crew;

		protected ComponentConverterCrew crew
		{
			get
			{
				if (_crew == null)
				{
					_crew = new GameObject().AddComponent<ComponentConverterCrew>();
					_crew.Initialize(this);
				}

				return _crew;
			}

		}

		public bool storeProps = true;

		public bool convertProps = true;

		public const string ModelUnits = Units.Meters;

		public abstract bool CanConvertToNative(Base type);

		public abstract bool CanConvertToSpeckle(Component type);

		public abstract GameObject ToNative(Base @base);

		public abstract void ToNativeConversion(Base @base, ref Component obj);

		public abstract UniTask ToNativeConversionAsync(Base @base, Component obj);

		public abstract Base ToSpeckle(Component component);

		public abstract Type unity_type { get; }

		public abstract string speckle_type { get; }

		public bool HasWorkToDo => crew != null && crew.HasWorkToDo;

		public UniTask PostWork() => crew.HasWorkToDo ? crew.PostWork() : UniTask.CompletedTask;

		public async UniTask PostWorkAsync() => await crew.PostWorkAsync();

		public ScriptableConverterSettings settings { get; set; }

		public virtual bool Equals(ComponentConverter other)
		{
			return other != null
			       && other.convertProps == convertProps
			       && other.storeProps == storeProps
			       && other.unity_type != null
			       && other.unity_type == unity_type
			       && other.speckle_type.Valid()
			       && other.speckle_type.Equals(speckle_type);
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

		protected virtual BaseBehaviour GetBaseType(GameObject obj)
		{
			if (obj == null) obj = new GameObject();

			var bb = obj.GetComponent<BaseBehaviour>();

			if (bb == null) bb = obj.AddComponent<BaseBehaviour>();

			return bb;
		}

	}

	public abstract class ComponentConverter<TBase, TComponent> : ComponentConverter
		where TComponent : Component
		where TBase : Base
	{

		public override string speckle_type => _info.speckleTypeName;

		public override Type unity_type => typeof(TComponent);

		protected virtual HashSet<string> excludedProps =>
			new HashSet<string>(
				typeof(Base).GetProperties(
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
				).Select(x => x.Name));

		protected bool IsRuntime => Application.isPlaying;

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
			_info = new ComponentInfo(
				typeof(TComponent).ToString(),
				Activator.CreateInstance<TBase>().speckle_type
			);
		}

		// TODO: this is silly, probably a much smarter way of handling this 
		public override bool CanConvertToNative(Base type) => type != null && type.GetType() == typeof(TBase);

		public override bool CanConvertToSpeckle(Component type) => type != null && type.GetType() == typeof(TComponent);

		protected abstract Base ConvertComponent(TComponent component);

		protected abstract void ConvertBase(TBase @base, ref TComponent instance);

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

		/// <summary>
		///   Unity Component to search for when trying to convert a game object
		/// </summary>
		public override GameObject ToNative(Base @base)
		{
			if (!CanConvertToNative(@base))
				return null;

			// NOTE: this is where the conversion of the base object goes into a game object
			// NOTE: the settings for the parent converter should be stored here

			if (@base is TBase compBase)
			{
				// NOTE: okay, this is where the settings needs to inform the converter what to do with the object. 
				// 0: the gameobject is added to the scene from the converter
				var component = CreateComponentInstance();

				switch (settings.style)
				{
					// ex. 1: create the gameobject and convert the data
					case ConverterStyle.Direct:

						// 11: the object data is parsed through the converter
						ConvertBase(compBase, ref component);
						// ConvertBase(compBase, ref obj);

						// 1c: the object is returned
						if (storeProps && component != null)
						{
							var bb = GetBaseType(component.gameObject);
							bb.Store(@base);
						}

						return component.gameObject;

					// ex. 2: create the gameobject and store the data for conversion later
					case ConverterStyle.Queue:

						var baseType = GetBaseType(component.gameObject);

						// 2a: the base object is stored in the base behaviour 
						baseType.Store(@base);

						// 2b: the converter passes back some info around the gameobject with the stored data to be used later for working through all the objects
						crew.Add(@base, component);

						// no need to do this stuff, since we are not right now
						// ConvertBase(compBase, ref obj);

						break;
					default:
						return null;
				}

				return component.gameObject;
			}

			SpeckleUnity.Console.Warn($"{@base.speckle_type} somehow ended up in the wrong converter!\n{this}");
			return null;
		}

		public override Base ToSpeckle(Component component) => CanConvertToSpeckle(component) ? ConvertComponent((TComponent)component) : null;
		public TComponent CreateComponentInstance(string n = null) =>
			GetBaseType(new GameObject(n.Valid() ? nameof(TBase) : n).AddComponent<TComponent>().gameObject).GetComponent<TComponent>();

	}

}