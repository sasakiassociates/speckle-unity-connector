using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Models;
using Speckle.ConnectorUnity.Mono;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Converter
{

	public class ComponentConverterArgs
	{
		public readonly int componentInstanceId;
		public readonly string baseId;
		public readonly GameObject targetObject;

		public ComponentConverterArgs(int componentInstanceId, GameObject targetObject, string baseId)
		{
			this.componentInstanceId = componentInstanceId;
			this.targetObject = targetObject;
			this.baseId = baseId;
		}
	}

	public abstract class ComponentConverter : ScriptableObject, IComponentConverter
	{

		[SerializeField, HideInInspector] ConverterCrewMember _crew;

		[SerializeField] protected ComponentInfo _info;

		public bool storeProps = true;

		public bool convertProps = true;

		public const string ModelUnits = Units.Meters;

		public abstract bool CanConvertToNative(Base type);

		public abstract bool CanConvertToSpeckle(Component type);

		public abstract GameObject ToNative(Base @base);

		public abstract Base ToSpeckle(Component component);

		public abstract Type unity_type { get; }

		public abstract string speckle_type { get; }

		protected ConverterCrewMember crew
		{
			get
			{
				if (_crew == null)
				{
					_crew = new GameObject().AddComponent<ConverterCrewMember>();
					_crew.Initialize(this);
				}

				return _crew;
			}

		}

		public ScriptableSpeckleConverterSettings settings { get; set; }

		public event UnityAction<ComponentConverterArgs> OnObjectConverted;

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

		protected void TriggerObjectConversionEvent(ComponentConverterArgs args) => OnObjectConverted?.Invoke(args);

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

		protected virtual BaseBehaviour_v2 GetBaseType(GameObject obj)
		{
			if (obj == null) obj = new GameObject();

			var bb = obj.GetComponent<BaseBehaviour_v2>();

			if (bb == null) bb = obj.AddComponent<BaseBehaviour_v2>();

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

		protected abstract void ConvertBase(TBase @base, ref TComponent instance);

		protected abstract Base ConvertComponent(TComponent component);

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
				var obj = CreateComponentInstance();

				switch (settings.style)
				{
					// ex. 1: create the gameobject and convert the data
					case ConverterStyle.Direct:

						// 11: the object data is parsed through the converter
						ConvertBase(compBase, ref obj);
						// ConvertBase(compBase, ref obj);

						// 1c: the object is returned
						if (storeProps && obj != null)
						{
							var bb = (BaseBehaviour_v2)obj.GetComponent(typeof(BaseBehaviour_v2));

							if (bb == null) bb = obj.gameObject.AddComponent<BaseBehaviour_v2>();
							bb.Store(@base);
						}

						return obj.gameObject;

					// ex. 2: create the gameobject and store the data for conversion later
					case ConverterStyle.Queue:

						var baseType = GetBaseType(obj.gameObject);

						// 2a: the base object is stored in the base behaviour 
						baseType.Store(@base);

						// 2b: the converter passes back some info around the gameobject with the stored data to be used later for working through all the objects
						var converterObjArgs = new ComponentConverterArgs(obj.GetInstanceID(), obj.gameObject, @base.id);

						// no need to do this stuff, since we are not right now
						// ConvertBase(compBase, ref obj);

						// 2c: the post call is triggered for the converters to work through their objects
						TriggerObjectConversionEvent(converterObjArgs);

						break;
					default:
						return null;
				}

				return obj.gameObject;
			}

			SpeckleUnity.Console.Warn($"{@base.speckle_type} somehow ended up in the wrong converter!\n{this}");
			return null;
		}

		public override Base ToSpeckle(Component component) => CanConvertToSpeckle(component) ? ConvertComponent((TComponent)component) : null;

		protected override BaseBehaviour_v2 GetBaseType(GameObject obj)
		{
			var comp = typeof(TComponent);

			if (obj != null) obj = CreateComponentInstance().gameObject;

			if (comp.IsSubclassOf(typeof(BaseBehaviour_v2)) || comp == typeof(BaseBehaviour_v2))
				return obj.GetComponent<TComponent>() as BaseBehaviour_v2;

			return base.GetBaseType(obj);
		}

		public TComponent CreateComponentInstance(string n = null) =>
			GetBaseType(new GameObject(n.Valid() ? nameof(TBase) : n).AddComponent<TComponent>().gameObject).GetComponent<TComponent>();

	}

}