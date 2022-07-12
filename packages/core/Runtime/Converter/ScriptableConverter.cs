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
		[SerializeField] protected string description;

		[SerializeField] protected string author;

		[SerializeField] protected string websiteOrEmail;

		[SerializeField] protected ReceiveMode receiveMode;

		[SerializeField] protected List<ComponentConverter> converters;

		public HashSet<Exception> ConversionErrors
		{
			get;
		} = new();

		public List<ApplicationPlaceholderObject> ContextObjects
		{
			get;
			set;
		} = new();

		public ProgressReport Report
		{
			get;
			protected set;
		}

		public IEnumerable<string> GetServicedApplications()
		{
			return new[] { HostApplications.Unity.Name };
		}

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

			return TryGetConverter(@base.speckle_type, out var converter) ? converter.ToNative(@base) : null;
		}

		public virtual List<Base> ConvertToSpeckle(List<object> objects)
		{
			return objects.Select(ConvertToSpeckle).ToList();
		}

		public virtual List<object> ConvertToNative(List<Base> objects)
		{
			return objects.Select(ConvertToNative).ToList();
		}

		public virtual bool CanConvertToSpeckle(object @object)
		{
			return TryGetConverter(@object, out _, out _);
		}

		public virtual bool CanConvertToNative(Base @base)
		{
			return converters.Valid() && converters.Any(x => x.CanConvertToNative(@base));
		}

		protected bool TryGetConverter(string speckleType, out ComponentConverter converter)
		{
			converter = null;
			
			if (!converters.Any())
				return false;

			foreach (var c in converters)
			{
				if (c.speckle_type.Equals(speckleType))
				{
					converter = c;
					break;
				}
			}

			return converter != null;
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

		public ReceiveMode ReceiveMode
		{
			get => receiveMode;
			set
			{
				Debug.Log($"Changing Receive Mode from {receiveMode} to {value}");
				receiveMode = value;
			}
		}

		#endregion converter properties

	}
}