using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

	public abstract class ScriptableSpeckleConverter : ScriptableObject, ISpeckleConverter
	{

		[SerializeField] string _description;

		[SerializeField] string _author;

		[SerializeField] string _websiteOrEmail;

		[SerializeField] ReceiveMode _receiveMode;

		[SerializeField] List<ComponentConverter> _converters;

		[SerializeField] List<ConverterCrewMember> _converterCrew;

		[SerializeField] ScriptableSpeckleConverterSettings _settings;

		public HashSet<Exception> ConversionErrors { get; } = new();

		public List<ApplicationPlaceholderObject> ContextObjects { get; set; } = new();

		public ProgressReport Report { get; protected set; }

		public IEnumerable<string> GetServicedApplications() => new[] { HostApplications.Unity.Name };

		public ComponentConverter defaultConverter { get; private set; }

		public void SetDefaultConverter(ComponentConverter converter)
		{
			foreach (var c in converters)
			{
				// what happens if there are two types of converters of the same type? 
				if (c.unity_type == converter.unity_type && c.speckle_type == converter.speckle_type && c.name == converter.name)
				{
					defaultConverter = c;
					return;
				}
			}

			// no converter was found, so add set to default 
			converters.Add(converter);
			defaultConverter = converters.Last();
		}

		public List<ComponentConverter> converters
		{
			get
			{
				if (!_converters.Valid())
					_converters = StandardConverters();

				return _converters;
			}
			set => _converters = value;
		}

		protected virtual void OnEnable()
		{
			_converters ??= StandardConverters();
			if (_settings == null) SetConverterSettings(new ScriptableSpeckleConverterSettings() { style = ConverterStyle.Queue });
		}

		public virtual async UniTask PostToNative()
		{
			// NOTE: probably need to check the settings values as wall
			if (_converterCrew.Valid())
			{
				await UniTask.WhenAll(_converterCrew.Select(x => x.PostWork()));
			}
		}

		public abstract List<ComponentConverter> StandardConverters();

		public virtual void SetContextObjects(List<ApplicationPlaceholderObject> objects) => ContextObjects = objects;

		public virtual void SetPreviousContextObjects(List<ApplicationPlaceholderObject> objects) => ContextObjects = objects;

		public virtual void SetContextDocument(object doc)
		{ }

		public virtual void SetConverterSettings(object settings)
		{
			if (settings is ScriptableSpeckleConverterSettings converterSettings)
			{
				_settings = converterSettings;
				if (_converters.Valid()) _converters.ForEach(x => x.settings = _settings);
			}
		}

		public virtual Base ConvertToSpeckle(object @object) =>
			@object != null && TryGetConverter(@object, out var comp, out var converter) ? converter.ToSpeckle(comp) : null;

		public virtual object ConvertToNative(Base @base) =>
			@base != null && TryGetConverter(@base.speckle_type, out var converter) ? converter.ToNative(@base) : null;

		public virtual List<Base> ConvertToSpeckle(List<object> objects) => objects.Valid() ? objects.Select(ConvertToSpeckle).ToList() : new List<Base>();

		public virtual List<object> ConvertToNative(List<Base> objects) => objects.Valid() ? objects.Select(ConvertToNative).ToList() : new List<object>();

		public virtual bool CanConvertToSpeckle(Component @object) => _converters.Valid() && _converters.Any(x => x.CanConvertToSpeckle(@object));

		public virtual bool CanConvertToSpeckle(object @object) => TryGetConverter(@object, out _, out _);

		public virtual bool CanConvertToNative(Base @base) => _converters.Valid() && _converters.Any(x => x.CanConvertToNative(@base));

		protected bool TryGetConverter(string speckleType, out ComponentConverter converter)
		{
			converter = null;

			if (!_converters.Any()) return false;

			foreach (var c in _converters)
			{
				if (c.speckle_type.Equals(speckleType))
				{
					converter = c;

					_converterCrew ??= new List<ConverterCrewMember>();

					if (!_converterCrew.Any(x => x.Equals(c)))
					{
						var crew = new GameObject().AddComponent<ConverterCrewMember>();
						crew.Initialize(converter);
						_converterCrew.Add(crew);
					}

					break;
				}
			}

			return converter != null;
		}

		protected bool TryGetConverter(object @object, out Component comp, out IComponentConverter converter)
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
						comp = o.GetComponent(c.unity_type);
						if (comp == null)
							continue;

						converter = c;
						break;
					}

					break;

				case Component o:
					comp = o;
					foreach (var c in _converters)
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