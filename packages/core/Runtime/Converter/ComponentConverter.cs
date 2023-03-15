using Cysharp.Threading.Tasks;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Converter
{

  public abstract class ComponentConverter : ScriptableObject, IComponentConverter, IComponentConverterProcess
  {
    /// <summary>
    /// Simple data container of what the component supports
    /// </summary>
    [SerializeField] protected ComponentInfo info;

    public ScriptableConverter parent { get; set; }

    /// <summary>
    /// Stored queue of args to convert if using <see cref="ConverterSettings.ConversionStyle.Queue"/> settings
    /// </summary>
    protected ConcurrentQueue<ConvertableObjectData> queue;

    /// <summary>
    /// A container for managing the conversion settings
    /// </summary>
    public ConverterSettings Settings { get; set; }

    /// <summary>
    /// Reports true if any items are in the queue
    /// </summary>
    public bool HasWorkToDo => queue.Valid();
    
    /// <summary>
    /// The unity <see cref="Component"/> type targeted for conversion
    /// </summary>
    public string UnityType => info.unityTypeName;

    /// <summary>
    /// The <see cref="Base"/> speckle type targeted for conversion
    /// </summary>
    public string SpeckleType => info.speckleTypeName;

    /// <summary>
    /// <para> Typical speckle conversion called from <seealso cref="ISpeckleConverter"/>
    /// This method is mainly used for setting up the gameobject and component data on the  main thread</para>
    ///
    /// <para>The conversions happen in two styles, one by one on the main thread when using <see cref="ConverterSettings.ConversionStyle.Sync"/>
    /// or they can be in different threads when using <see cref="ConverterSettings.ConversionStyle.Queue"/>.
    /// Conversion style can be changed in the <see cref="Settings"/> object</para>
    /// 
    /// </summary>
    /// <param name="base">The <see cref="Base"/> object to convert</param>
    /// <returns>The scene object with necessary component info</returns>
    public GameObject ToNative(Base @base)
    {
      var comp = CreateComponentInstance();

      switch(Settings.style)
      {
        case ConverterSettings.ConversionStyle.Sync:
          ToNative(@base, ref comp);
          break;
        case ConverterSettings.ConversionStyle.Queue:
          queue.Enqueue(new ConvertableObjectData(@base, comp));
          OnQueueSizeChanged?.Invoke(queue.Count);
          break;
        default:
          SpeckleUnity.Console.Log($"Style {Settings.style} is not supported in native conversions");
          break;
      }

      return comp.gameObject;
    }


    /// <summary>
    /// Actual conversion logic for processing speckle data into the necessary unity component(s)
    /// </summary>
    /// <param name="base">Speckle Object to convert</param>
    /// <param name="obj">Referenced scene object with component to send data to</param>
    public abstract void ToNative(Base @base, ref Component obj);

    /// <summary>
    /// Creates a <see cref="GameObject"/> with the type component attached to it
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    protected abstract Component CreateComponentInstance(string n = null);

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

    // TODO: Figure out where this should be handled.

    /// <summary>
    /// Follow up method for handling all conversion data
    /// Will process speckle data at specific rate set in <see cref="ConverterSettings.queueSpeed"/>
    /// </summary>
    public async UniTask PostWorkAsync()
    {
      SpeckleUnity.Console.Log($"{nameof(PostWorkAsync)} for {name} with {queue.Count}");

      try
      {
        if(queue.Valid())
        {
          var chunk = new List<ConvertableObjectData>();
          while(queue.TryDequeue(out var args))
          {
            chunk.Add(args);

            if(queue.Count <= 0 || chunk.Count >= Settings.queueSpeed)
            {
              await UniTask.WhenAll(chunk.Select(x => ToNativeConversionAsync(x.speckleObj, x.unityObj)));

              chunk = new List<ConvertableObjectData>();

              OnQueueSizeChanged?.Invoke(queue.Count);

              // call for making sure the ui thread doesnt get blocked
              await UniTask.Yield();
            }
          }
        }
      }
      catch(Exception e)
      {
        SpeckleUnity.Console.Warn(e.Message);
      }
      finally
      {
        SpeckleUnity.Console.Log("Post Work Completed");
        OnQueueSizeChanged?.Invoke(0);
      }
    }
    
    /// <summary>
    /// Returns true if <see cref="ComponentConverter.UnityType"/> and <see cref="ComponentConverter.SpeckleType"/> are same
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ComponentConverter other) =>
      other != null
      && other.UnityType != null
      && other.UnityType == UnityType
      && other.SpeckleType.Valid()
      && other.SpeckleType.Equals(SpeckleType);

    /// <summary>
    /// Event for notify queue updates during conversion
    /// </summary>
    public event UnityAction<int> OnQueueSizeChanged;


    [Serializable]
    public struct ComponentInfo
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

    protected bool ValidObjects(Base @base, Component component, out TBase tBase, out TComponent tComp)
    {
      tBase = null;
      tComp = null;

      if(@base is TBase b && component is TComponent c)
      {
        tBase = b;
        tComp = c;
      }

      return tBase != null && tComp != null;
    }

    /// <inheritdoc />
    public override bool CanConvertToNative(Base type) =>
      type != null && type.GetType() == typeof(TBase);

    /// <inheritdoc />
    public override bool CanConvertToSpeckle(Component type) =>
      type != null && type.GetType() == typeof(TComponent);

    /// <inheritdoc />
    public override Base ToSpeckle(Component component) =>
      CanConvertToSpeckle(component) ? ConvertComponent((TComponent)component) : null;

    /// <inheritdoc />
    public override void ToNative(Base @base, ref Component component)
    {
      if(ValidObjects(@base, component, out var converterObj, out var converterComp)) ConvertBase(converterObj, ref converterComp);
    }

    // TODO: Not sure if a async method is really needed here
    /// <inheritdoc />
    public override async UniTask ToNativeConversionAsync(Base @base, Component component) =>
      await UniTask.Create(() =>
        {
          ToNative(@base, ref component);
          return UniTask.CompletedTask;
        }
      );

    /// <inheritdoc />
    protected override Component CreateComponentInstance(string n = null) =>
      new GameObject(n.Valid() ? n : SpeckleType).AddComponent<TComponent>();

    /// <summary>
    /// Nested method from <see cref="ToNative"/> that sets the types for conversion 
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public abstract Base ConvertComponent(TComponent component);

    /// <summary>
    /// Nested method from <see cref="ToSpeckle"/> that sets the types for conversion
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="instance"></param>
    protected abstract void ConvertBase(TBase obj, ref TComponent instance);

    protected virtual void OnEnable()
    {
      info = new ComponentInfo(typeof(TComponent).ToString(), Activator.CreateInstance<TBase>().speckle_type);
    }


  }

}
