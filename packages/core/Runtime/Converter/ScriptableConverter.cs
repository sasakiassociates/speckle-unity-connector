using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Speckle.ConnectorUnity.Converter
{

	public abstract class ScriptableSpeckleConverter : ScriptableObject, ISpeckleConverter
	{

		[SerializeField] string _description;

		[SerializeField] string _author;

		[SerializeField] string _websiteOrEmail;

		[SerializeField] ReceiveMode _receiveMode;

		[SerializeField] List<ComponentConverter> _converters;

		[SerializeField] ScriptableConverterSettings _settings;

		public Dictionary<string, Object> LoadedAssets { get; private set; }
		
		public HashSet<Exception> ConversionErrors { get; } = new();

		public List<ApplicationObject> ContextObjects { get; set; } = new();

		public ProgressReport Report { get; protected set; }

		public IEnumerable<string> GetServicedApplications() => new[] { HostApplications.Unity.Name };

		public List<ComponentConverter> converters { get; private set; }

		protected virtual void OnEnable()
		{
			// Don't override serialized scriptable object lists
			converters = _converters.Valid() ? _converters : StandardConverters();

			if (_settings == null) SetConverterSettings(new ScriptableConverterSettings() { style = ConverterStyle.Queue });
		}

		public virtual async UniTask PostWork()
		{
			if (!_converters.Valid()) return;

			foreach (var c in _converters)
			{
				if (c != null && c.HasWorkToDo)
					await c.PostWorkAsync();
			}
		}

		public abstract List<ComponentConverter> StandardConverters();

		public virtual void SetContextObjects(List<ApplicationObject> objects) => ContextObjects = objects;

		public virtual void SetPreviousContextObjects(List<ApplicationObject> objects) => ContextObjects = objects;

		public virtual void SetContextDocument(object doc)
		{
			if(doc is not Dictionary<string, Object>  loadedAssets) throw new ArgumentException($"Expected {nameof(doc)} to be of type {typeof(Dictionary<string, Object>)}", nameof(doc));
			LoadedAssets = loadedAssets;
		}

		public virtual void SetConverterSettings(object settings)
		{
			if (settings is ScriptableConverterSettings converterSettings)
				_settings = converterSettings;
		}

		public virtual Base ConvertToSpeckle(object @object) =>
			@object != null && TryGetConverter(@object, true, out var comp, out var converter) ? converter.ToSpeckle(comp) : null;

		public virtual object ConvertToNative(Base @base) =>
			@base != null && TryGetConverter(@base, true, out var converter) ? converter.ToNative(@base) : null;

		public virtual List<Base> ConvertToSpeckle(List<object> objects) => objects.Valid() ? objects.Select(ConvertToSpeckle).ToList() : new List<Base>();

		public virtual List<object> ConvertToNative(List<Base> objects) => objects.Valid() ? objects.Select(ConvertToNative).ToList() : new List<object>();

		public virtual bool CanConvertToSpeckle(object @object) => TryGetConverter(@object, false, out _, out _);

		public virtual bool CanConvertToNative(Base @base) => TryGetConverter(@base, false, out _);

		protected virtual bool TryGetConverter(Base speckleType, bool init, out ComponentConverter converter)
		{
			converter = null;

			if (!_converters.Valid()) return false;

			foreach (var c in _converters)
			{
				if (c == null) continue;

				if (c.speckle_type.Equals(speckleType.speckle_type))
				{
					converter = c;
					if (init)
					{
						c.settings = _settings;
					}

					break;
				}
			}

			return converter != null;
		}

		bool TryGetConverter(object @object, bool init, out Component comp, out IComponentConverter converter)
		{
			comp = null;
			converter = default;

			if (!_converters.Any())
				return false;

			switch (@object)
			{
				case GameObject o:
					foreach (var c in _converters)
					{
						if (c == null || o.GetComponent(c.unity_type)) continue;

						converter = c;

						if (init) c.settings = _settings;

						break;
					}

					break;

				case Component o:
					comp = o;
					foreach (var c in _converters)
					{
						if (c == null || c.unity_type != comp.GetType())
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

		#region converter properties

		public string Name
		{
			get => name;
			set => name = value;
		}

		public string Description
		{
			get => _description;
		}

		public string Author
		{
			get => _author;
		}

		public string WebsiteOrEmail
		{
			get => _websiteOrEmail;
		}

		public ReceiveMode ReceiveMode
		{
			get => _receiveMode;
			set
			{
				Debug.Log($"Changing Receive Mode from {_receiveMode} to {value}");
				_receiveMode = value;
			}
		}

		#endregion converter properties

	}
}