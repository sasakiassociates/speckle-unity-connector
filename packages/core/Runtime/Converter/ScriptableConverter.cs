using Cysharp.Threading.Tasks;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

  public abstract class ScriptableConverter : ScriptableObject, ISpeckleConverter
  {

    [SerializeField] string description;

    [SerializeField] string author;

    [SerializeField] string websiteOrEmail;

    [SerializeField] ReceiveMode receiveMode;

    [SerializeField] List<ComponentConverter> storedConverters;

    [SerializeField] ScriptableConverterSettings settings;

    public Dictionary<string, UnityEngine.Object> LoadedAssets { get; private set; }

    public HashSet<Exception> ConversionErrors { get; } = new();

    public List<ApplicationObject> ContextObjects { get; set; } = new();

    public ProgressReport Report { get; protected set; }

    public IEnumerable<string> GetServicedApplications() => new[] {HostApplications.Unity.Name};

    public List<ComponentConverter> converters { get; private set; }

    protected abstract List<ComponentConverter> StandardConverters();

    protected virtual void OnEnable()
    {
      Report = new ProgressReport();
      // Don't override serialized scriptable object lists
      converters = storedConverters.Valid() ? storedConverters : StandardConverters();

      foreach(var cc in converters)
      {
        cc.Parent = this;
      }

      if(settings == null)
        SetConverterSettings(new ScriptableConverterSettings() {style = ConverterStyle.Queue});
    }

    public virtual void SetContextObjects(List<ApplicationObject> objects) => ContextObjects = objects;

    public virtual void SetPreviousContextObjects(List<ApplicationObject> objects) => ContextObjects = objects;

    public virtual void SetContextDocument(object doc)
    {
      if(doc is not Dictionary<string, UnityEngine.Object> loadedAssets)
        throw new ArgumentException($"Expected {nameof(doc)} to be of type {typeof(Dictionary<string, UnityEngine.Object>)}", nameof(doc));

      LoadedAssets = loadedAssets;
    }

    public virtual void SetConverterSettings(object settings)
    {
      if(settings is ScriptableConverterSettings converterSettings)
        this.settings = converterSettings;
    }

    public virtual Base ConvertToSpeckle(object @object) =>
      TryGetConverter(@object, true, out var comp, out var converter) ? converter.ToSpeckle(comp) : null;

    public virtual object ConvertToNative(Base @base) =>
      TryGetConverter(@base, true, out var converter) ? converter.ToNative(@base) : null;

    public virtual List<Base> ConvertToSpeckle(List<object> objects) => objects.Valid() ? objects.Select(ConvertToSpeckle).ToList() : new List<Base>();

    public virtual List<object> ConvertToNative(List<Base> objects) => objects.Valid() ? objects.Select(ConvertToNative).ToList() : new List<object>();

    public virtual bool CanConvertToSpeckle(object @object) => TryGetConverter(@object, false, out _, out _);

    public virtual bool CanConvertToNative(Base @base) => TryGetConverter(@base, false, out _);

    public async UniTask PostWork()
    {
      if(!storedConverters.Valid())
        return;

      foreach(var c in storedConverters)
      {
        if(c != null && c.HasWorkToDo)
          await c.PostWorkAsync();
      }
    }


    bool TryGetConverter(Base obj, bool init, out ComponentConverter converter)
    {
      converter = null;

      if(!storedConverters.Valid())
      {
        SpeckleUnity.Console.Log($"No valid Converters found in {name}");
        return false;
      }

      if(obj == null)
      {
        SpeckleUnity.Console.Log($"Object is null, please check the source of the object you are trying to pass into {name}");
        return false;
      }

      foreach(var c in storedConverters)
      {
        if(c == null)
          continue;

        if(c.speckle_type.Equals(obj.speckle_type))
        {
          converter = c;

          if(init)
            c.Settings = settings;

          break;
        }
      }

      return converter != null;
    }

    bool TryGetConverter(object obj, bool init, out Component comp, out IComponentConverter converter)
    {
      comp = null;
      converter = default(IComponentConverter);

      if(!storedConverters.Valid())
      {
        SpeckleUnity.Console.Log($"No valid Converters found in {name}");
        return false;
      }

      if(obj == null)
      {
        SpeckleUnity.Console.Log($"Object is null, please check the source of the object you are trying to pass into {name}");
        return false;
      }

      switch(obj)
      {
        case GameObject o:
          foreach(var c in storedConverters)
          {
            if(c == null || o.GetComponent(c.unity_type))
              continue;

            converter = c;

            if(init)
              c.Settings = settings;

            break;
          }

          break;

        case Component o:
          comp = o;
          foreach(var c in storedConverters)
          {
            if(c == null || c.unity_type != comp.GetType())
              continue;

            converter = c;
            break;
          }

          break;
        default:
          Debug.LogException(new SpeckleException($"Native unity object {obj.GetType()} is not supported"));
          break;
      }

      return converter != default(object) && comp != null;
    }

  #region converter properties

    public string Name
    {
      get => name;
      set => name = value;
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
