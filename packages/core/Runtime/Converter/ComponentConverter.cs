using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Models;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

	public abstract class ComponentConverter : ScriptableObject, IComponentConverter
	{

		[SerializeField] protected ComponentInfo _info;

		ComponentConverterCrew _crew;

		public ScriptableConverterSettings settings { get; set; }

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

		public const string ModelUnits = Units.Meters;

		public abstract bool CanConvertToNative(Base type);

		public abstract bool CanConvertToSpeckle(Component type);

		public abstract GameObject ToNative(Base @base);

		public abstract void ToNativeConversion(Base @base, ref Component obj);

		public abstract UniTask ToNativeConversionAsync(Base @base, Component obj);

		public abstract Base ToSpeckle(Component component);

		public abstract Type unity_type { get; }

		public abstract string speckle_type { get; }

		public async UniTask PostWorkAsync()
		{
			if (crew.HasWorkToDo)
			{
				SpeckleUnity.Console.Log("Post Work Started");
				await crew.PostWorkAsync();
			}

			SpeckleUnity.Console.Log("Post Work Completed");

			SpeckleUnity.SafeDestroy(crew.gameObject);
		}

		public virtual bool Equals(ComponentConverter other)
		{
			return other != null
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

		protected virtual void OnEnable() => _info =
			new ComponentInfo(typeof(TComponent).ToString(), Activator.CreateInstance<TBase>().speckle_type);

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
			if (!CanConvertToNative(@base) || @base is not TBase compBase)
			{
				SpeckleUnity.Console.Warn($"{@base.speckle_type} somehow ended up in the wrong converter!\n{this}");
				return null;
			}

			var component = CreateComponentInstance();

			GetBaseType(component.gameObject).Store(@base);

			// ex. 1: create the gameobject and convert the data
			if (settings.style == ConverterStyle.Direct)
			{
				ConvertBase(compBase, ref component);
			}
			else // ex. 2: create the gameobject and store the data for conversion later
			{
				crew.Add(@base, component);
			}

			return component.gameObject;
		}

		public override Base ToSpeckle(Component component) => CanConvertToSpeckle(component) ? ConvertComponent((TComponent)component) : null;

		public TComponent CreateComponentInstance(string n = null) => GetBaseType(
			new GameObject(n.Valid() ? n : this.GetType().ToString().Split('.').LastOrDefault() + " Instance")
				.AddComponent<TComponent>().gameObject).GetComponent<TComponent>();

	}

}