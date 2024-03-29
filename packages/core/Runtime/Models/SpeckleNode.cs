﻿using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Converter;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Speckle.ConnectorUnity.Models
{

  /// <summary>
  ///   A speckle node is pretty much the reference object that is first pulled from a commit
  /// </summary>
  [AddComponentMenu(SpeckleUnity.NAMESPACE + "/Node")]
  public class SpeckleNode : MonoBehaviour
  {
    [SerializeField][HideInInspector] string id;

    [SerializeField][HideInInspector] string appId;

    [SerializeField][HideInInspector] long childCount;

    [SerializeField] SpeckleObjectHierarchy hierarchy;

    /// <summary>
    ///   Reference object id
    /// </summary>
    public string Id => id;

    /// <summary>
    ///   Reference to application ID
    /// </summary>
    public string AppId => appId;

    /// <summary>
    ///   Total child count
    /// </summary>
    public long ChildCount => childCount;

    public List<GameObject> GetObjects() => hierarchy.GetObjects();

    public void AddLayer(SpeckleLayer layer) => hierarchy.Add(layer);

    /// <summary>
    ///   Setup the hierarchy for the commit coming in
    /// </summary>
    /// <param name="data">The object to convert</param>
    /// <param name="converter">Speckle Converter to parse objects with</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public async UniTask DataToScene(Base data, ISpeckleConverter converter, CancellationToken token)
    {
      if(data == null) return;

      id = data.id;
      appId = data.applicationId;
      childCount = (int)data.totalChildrenCount;
      name = $"Node: {id}";

      if(converter == null)
      {
        SpeckleUnity.Console.Warn("No valid converter to use during conversion ");
        return;
      }

      hierarchy = new SpeckleObjectHierarchy();
      var defaultLayer = new GameObject("Default").AddComponent<SpeckleLayer>();

      DeconstructObject(data, defaultLayer, converter, token);

      Debug.Log("Speckle Node Complete");

      if(defaultLayer.Layers.Any())
      {
        defaultLayer.ParentObjects(transform);
        hierarchy.Add(defaultLayer);
      }
      else
        Utils.SafeDestroy(defaultLayer.gameObject);

      if(converter is ScriptableConverter sc)
      {
        Debug.Log("Doing post work");
        await sc.PostWork();
      }
    }

    void DeconstructObject(Base data, SpeckleLayer defaultLayer, ISpeckleConverter converter, CancellationToken token)
    {
      if(token.IsCancellationRequested) return;

      // 1: Object is supported, so lets convert the object and it's data 
      if(converter.CanConvertToNative(data))
      {
        // NOTE: Object created in scene
        // NOTE: This is where the game object instance could be captured and meta stored in each object
        // NOTE: The converter should spawn a series of mono objects that will handle going through the list of objects to convert and send data to
        var obj = CheckConvertedFormat(converter.ConvertToNative(data));

        if(obj != null)
          defaultLayer.Add(obj);
      }

      // 2: Check for the properties of an object. There might be additional objects that we want to convert as well
      foreach(var member in data.GetMemberNames())
      {
        if(token.IsCancellationRequested)
          return;

        // NOTE: we should only have to check for certain props and make sure to not duplicate any
        var obj = data[member];

        // 2a: A prop is a list! We create a special list object to store those items in
        if(obj.IsList())
        {
          // create the list container 
          var layer = ListToLayer(member, ((IEnumerable)obj).Cast<object>(), converter, token, transform, Debug.Log);
          // handle the object conversion
          // code...

          // add to hierarchy 
          hierarchy.Add(layer);
          continue;
        }

        // 3: Member is a speckle object
        if(obj.IsBase(out var @base))
        {
          Debug.Log("stepping into objects");
          DeconstructObject(@base, defaultLayer, converter, token);
          continue;
        }

        Debug.LogWarning("Unhandled");
      }
    }

    public Base SceneToData(ISpeckleConverter converter, CancellationToken token)
    {
      var data = new Base();

      foreach(var layer in hierarchy.layers)
      {
        if(token.IsCancellationRequested)
          return data;

        data[layer.LayerName] = LayerToBase(layer, converter, token);
      }

      childCount = data.GetTotalChildrenCount();
      return data;
    }

    void OnEnable() => hierarchy ??= new SpeckleObjectHierarchy();

    static GameObject CheckConvertedFormat(object obj)
    {
      switch(obj)
      {
        case GameObject o:
          return o;
        case MonoBehaviour o:
          return o.gameObject;
        case Component o:
          return o.gameObject;
        default:
          SpeckleUnity.Console.Warn($"Object converted to unity from speckle is not supported {obj.GetType()}");
          return null;
      }
    }

    static SpeckleLayer ListToLayer(
      string member,
      IEnumerable<object> data,
      ISpeckleConverter converter,
      CancellationToken token,
      Transform parent = null,
      Action<string> onError = null
    )
    {
      var layer = new GameObject(member).AddComponent<SpeckleLayer>();

      foreach(var item in data)
      {
        if(token.IsCancellationRequested)
          return layer;

        if(item == null)
          continue;

        if(item.IsBase(out var @base) && converter.CanConvertToNative(@base))
        {
          // NOTE: Object created in scene
          // NOTE: this should be thrown back into the deconstruct object phase
          var obj = CheckConvertedFormat(converter.ConvertToNative(@base));

          if(obj != null)
            layer.Add(obj);
        }
        else if(item.IsList())
        {
          var list = ((IEnumerable)item).Cast<object>().ToList();
          var childLayer = ListToLayer(list.Count.ToString(), list, converter, token, layer.transform, onError);
          layer.Add(childLayer);
        }
        else
        {
          onError?.Invoke($"Cannot handle this type of object {item.GetType()}");
        }
      }

      if(parent != null)
        layer.transform.SetParent(parent);

      layer.ParentObjects(layer.transform);

      return layer;
    }

    static Base LayerToBase(SpeckleLayer layer, ISpeckleConverter converter, CancellationToken token)
    {
      var layerBase = new Base();
      try
      {
        var layerObjects = new List<Base>();

        foreach(var item in layer.Data)
        {
          if(token.IsCancellationRequested)
            return layerBase;

          var @base = ConvertRecursively(item, converter, token);

          if(@base != null)
            layerObjects.Add(@base);
        }

        layerBase["@data"] = layerObjects;
      }

      catch(SpeckleException e)
      {
        SpeckleUnity.Console.Warn(e.Message);
        return layerBase;
      }

      try
      {
        foreach(var nestedLayer in layer.Layers)
        {
          if(token.IsCancellationRequested)
            return layerBase;

          layerBase[nestedLayer.LayerName] = LayerToBase(nestedLayer, converter, token);
        }
      }
      catch(SpeckleException e)
      {
        SpeckleUnity.Console.Warn(e.Message);
        return layerBase;
      }

      return layerBase;
    }

    static Base ConvertRecursively(GameObject item, ISpeckleConverter converter, CancellationToken token)
    {
      var @base = new Base();

      if(token.IsCancellationRequested || item == null)
        return @base;

      if(converter.CanConvertToSpeckle(item))
        @base = converter.ConvertToSpeckle(item);

      if(CheckForChildren(item, converter, token, out var objs))
        @base["@Objects"] = objs;

      return @base;
    }

    static bool CheckForChildren(GameObject go, ISpeckleConverter converter, CancellationToken token, out List<Base> objs)
    {
      objs = new List<Base>();

      if(go != null && go.transform.childCount > 0)
        foreach(Transform child in go.transform)
        {
          if(token.IsCancellationRequested)
            return false;

          var converted = ConvertRecursively(child.gameObject, converter, token);
          if(converted != null)
            objs.Add(converted);
        }

      return objs.Any();
    }

  }

}
