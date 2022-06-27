using System;
using System.Collections.Generic;
using System.Linq;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

	public abstract class ScriptableSpeckleConverter : ScriptableObject, ISpeckleConverter
	{
		[Header("Speckle Converter Informations")] [SerializeField]
		protected string description;

		[SerializeField] protected string author;

		[SerializeField] protected string websiteOrEmail;

		[Space] [SerializeField] protected List<ComponentConverter> converters;

		// protected Dictionary<string, ComponentConverter> compiled;

		public HashSet<Exception> ConversionErrors { get; } = new();

		public List<ApplicationPlaceholderObject> ContextObjects { get; set; } = new();

		public ProgressReport Report { get; protected set; }

		public IEnumerable<string> GetServicedApplications() => new[] { HostApplications.Unity.Name };

		public virtual void SetContextObjects(List<ApplicationPlaceholderObject> objects)
		{
			ContextObjects = objects;
		}

		public virtual void SetPreviousContextObjects(List<ApplicationPlaceholderObject> objects)
		{
			ContextObjects = objects;
		}

		public virtual void SetContextDocument(object doc)
		{
			Debug.Log("Empty call from SetContextDocument");
		}

		public virtual void SetConverterSettings(object settings)
		{
			Debug.Log($"Converter Settings being set with {settings}");
		}

		public virtual Base ConvertToSpeckle(object @object)
		{
			if (TryGetConverter(@object, out var comp, out var converter))
				return converter.ToSpeckle(comp);

			Debug.LogWarning("No components found for converting to speckle");

			return null;
		}

		public virtual object ConvertToNative(Base @base)
		{
			if (@base == null)
			{
				Debug.LogWarning("Trying to convert a null object! Beep Beep! I don't like that");
				return null;
			}

			foreach (var converter in converters)
				if (converter.speckle_type.Equals(@base.speckle_type))
					return converter.ToNative(@base);

			return null;
		}

		public virtual List<Base> ConvertToSpeckle(List<object> objects) => objects.Select(ConvertToSpeckle).ToList();

		public virtual List<object> ConvertToNative(List<Base> objects) => objects.Select(ConvertToNative).ToList();

		public virtual bool CanConvertToSpeckle(object @object)
		{
			if (!converters.Any())
				return false;

			if (@object is GameObject go)
			{
				foreach (var converter in converters)
					if (go.GetComponent(converter.unity_type) != null)
					{
						Debug.Log($"Found {converter.name} for {converter.unity_type}");
						return true;
					}
			}

			else if (@object is Component comp)
			{
				var type = comp.GetType();

				foreach (var converter in converters)
					if (converter.unity_type == type)
					{
						Debug.Log($"Found {converter.name} for {converter.unity_type}");
						return true;
					}
			}

			return false;
		}

		public virtual bool CanConvertToNative(Base @object)
		{
			return converters.Valid() && converters.Any(x => x.CanConvertToNative(@object));
		}

		protected bool TryGetConverter(object @object, out Component comp, out IComponentConverter converter)
		{
			comp = null;
			converter = default;

			if (!converters.Any())
				return false;

			switch (@object)
			{
				case GameObject o:
					foreach (var c in converters)
					{
						comp = o.GetComponent(c.unity_type);
						if (comp == null)
							continue;

						converter = c;
						break;
					}

					break;

				case Component o:
					comp = o;
					foreach (var c in converters)
					{
						if (c.unity_type != comp.GetType())
							continue;

						converter = c;
						break;
					}

					break;
				default:
					Debug.LogException(new SpeckleException($"Native unity object {@object.GetType()} is not supported"));
					break;
			}

			return converter != default && comp != null;
		}

		// protected void CheckIfCompiled(bool toUnity, bool force = false)
		// {
		// 	if (force || !compiled.Valid())
		// 	{
		// 		compiled = new Dictionary<string, ComponentConverter>();
		// 		var fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		//
		// 		foreach (var field in fields)
		// 			if (field.FieldType.IsSubclassOf(typeof(ComponentConverter)) 
		// 			    && CreateInstance(field.FieldType) is ComponentConverter c)
		// 				compiled.Add(c.targetType(toUnity), c);
		// 	}
		// }

		public virtual bool CanConvertToSpeckle(Component @object)
		{
			return converters.Valid() && converters.Any(x => x.CanConvertToSpeckle(@object));
		}

		#region converter properties
		public string Name
		{
			get => name;
		}

		public string Description
		{
			get => description;
		}

		public string Author
		{
			get => author;
		}

		public string WebsiteOrEmail
		{
			get => websiteOrEmail;
		}
		#endregion converter properties

	}
}