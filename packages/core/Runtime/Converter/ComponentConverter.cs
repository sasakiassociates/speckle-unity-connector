using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Speckle.ConnectorUnity.Mono;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	public abstract class ComponentConverter : ScriptableObject, IComponentConverter
	{

		public const string ModelUnits = Units.Meters;

		public bool storeProps = true;

		public bool convertProps = true;

		public abstract bool CanConvertToNative(Base type);
		public abstract bool CanConvertToSpeckle(Component type);

		public abstract GameObject ToNative(Base @base);
		public abstract Base ToSpeckle(Component component);

		public abstract Type unity_type { get; }
		public abstract string speckle_type { get; }

		[Serializable]
		[HideInInspector]
		protected readonly struct ComponentInfo
		{
			public readonly string speckleTypeName;

			public readonly string unityTypeName;

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

		[SerializeField] protected ComponentInfo info;

		public override string speckle_type
		{
			get => info.speckleTypeName;
		}

		public override Type unity_type
		{
			get => typeof(TComponent);
		}

		protected virtual HashSet<string> excludedProps
		{
			get
			{
				return new HashSet<string>(
					typeof(Base).GetProperties(
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
					).Select(x => x.Name));
			}
		}

		protected bool IsRuntime
		{
			get => Application.isPlaying;
		}

		protected virtual void OnEnable()
		{
			info = new ComponentInfo(
				typeof(TComponent).ToString(),
				Activator.CreateInstance<TBase>().speckle_type
			);
		}

		// TODO: this is silly, probably a much smarter way of handling this 
		public override bool CanConvertToNative(Base type) => type != null && type.GetType() == typeof(TBase);

		public override bool CanConvertToSpeckle(Component type) => type != null && type.GetType() == typeof(TComponent);

		protected abstract GameObject ConvertBase(TBase @base);

		protected abstract Base ConvertComponent(TComponent component);

		/// <summary>
		///   Unity Component to search for when trying to convert a game object
		/// </summary>
		public override GameObject ToNative(Base @base)
		{
			if (!CanConvertToNative(@base))
				return null;

			var root = ConvertBase((TBase)@base).gameObject;

			if (storeProps && root != null)
			{
				var comp = (BaseBehaviour)root.GetComponent(typeof(BaseBehaviour));
				if (comp == null)
					comp = root.AddComponent<BaseBehaviour>();

				comp.SetProps(@base, excludedProps);
			}

			return root;
		}

		public override Base ToSpeckle(Component component) => CanConvertToSpeckle(component) ? ConvertComponent((TComponent)component) : null;

		protected TComponent BuildGo(string goName = null) => new GameObject(string.IsNullOrEmpty(goName) ? speckle_type : goName).AddComponent<TComponent>();
	}

}