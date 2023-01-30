using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Converter;
using Speckle.ConnectorUnity.Models;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{

  public static partial class SpeckleOps
  {
    /// <summary>
    ///   Setup the hierarchy for the commit coming in
    /// </summary>
    /// <param name="parent">Object to attach data to</param>
    /// <param name="data">The object to convert</param>
    /// <param name="converter">Speckle Converter to parse objects with</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public static async UniTask ConvertToScene(this SpeckleObjectBehaviour parent, Base data, ISpeckleConverter converter, CancellationToken token)
    {
      if(parent == null)
      {
        SpeckleUnity.Console.Warn("No valid Parent to use during conversion ");
        return;
      }

      parent.hierarchy = new SpeckleObjectHierarchy(parent.transform);

      await ConvertToScene(parent.hierarchy, data, converter, token);
    }

    /// <summary>
    ///   Setup the hierarchy for the commit coming in
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="data">The object to convert</param>
    /// <param name="converter">Speckle Converter to parse objects with</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public static async UniTask<SpeckleObjectHierarchy> ConvertToScene(Transform parent, Base data, ISpeckleConverter converter, CancellationToken token)
    {
      if(parent == null)
      {
        SpeckleUnity.Console.Warn("No valid Parent to use during conversion ");
        return null;
      }

      var hierarchy = new SpeckleObjectHierarchy(parent);

      await ConvertToScene(hierarchy, data, converter, token);

      return hierarchy;
    }

    /// <summary>
    ///   Setup the hierarchy for the commit coming in
    /// </summary>
    /// <param name="hierarchy"></param>
    /// <param name="data">The object to convert</param>
    /// <param name="converter">Speckle Converter to parse objects with</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public static async UniTask ConvertToScene(SpeckleObjectHierarchy hierarchy, Base data, ISpeckleConverter converter, CancellationToken token)
    {
      if(data == null)
      {
        SpeckleUnity.Console.Warn($"No valid {typeof(Base)} to use during conversion ");
        return;
      }

      if(converter == null)
      {
        SpeckleUnity.Console.Warn("No valid converter to use during conversion ");
        return;
      }

      SpeckleUnity.Console.Log("Starting To Convert");

      var layer = hierarchy.parent.GetComponent<SpeckleLayer>();

      if(layer == null)
      {
        layer = hierarchy.parent.gameObject.AddComponent<SpeckleLayer>();
      }

      layer.Parent = hierarchy.parent;

      hierarchy.SetDefault(layer);

      await DeconstructObject(hierarchy.DefaultLayer, data, converter, token);

      if(hierarchy.DefaultLayer.Layers.Any())
      {
        hierarchy.DefaultLayer.ParentObjects(hierarchy.parent);
      }
      else
      {
        Utils.SafeDestroy(hierarchy.DefaultLayer);
      }

      if(converter is ScriptableConverter sc)
      {
        await sc.PostWork();
      }

      if(converter.Report == null) return;

      if(converter.Report.ConversionErrors.Any())
      {
        foreach(var errors in converter.Report.ConversionErrors)
        {
          SpeckleUnity.Console.Warn(errors.Message);
        }
      }
    }

    static async UniTask DeconstructObject(SpeckleLayer layer, Base @base, ISpeckleConverter converter, CancellationToken token)
    {
      if(token.IsCancellationRequested)
      {
        return;
      }

      // 1: Object is supported, so lets convert the object and it's data 
      if(converter.CanConvertToNative(@base))
      {
        var obj = CheckConvertedFormat(converter.ConvertToNative(@base));

        if(obj != null)
        {
          layer.Add(obj);
        }

        await UniTask.Yield();
      }

      // 2: Check for the properties of an object. There might be additional objects that we want to convert as well
      foreach(var member in @base.GetDynamicMembers())
      {
        if(token.IsCancellationRequested)
        {
          return;
        }

        // NOTE: we should only have to check for certain props and make sure to not duplicate any
        var propObject = @base[member];

        if(propObject.IsBase(out var probBase))
        {
          // 2a: Member is a speckle object
          await DeconstructObject(layer, probBase, converter, token);
        }
        else if(propObject.IsList(out var nestedObjects))
        {
          // 2b: A prop is a list, we create a special list object to store those items in
          var nestedLayer = new GameObject(member).AddComponent<SpeckleLayer>();

          nestedLayer.transform.SetParent(layer.transform);

          foreach(var nestedObj in nestedObjects)
          {
            if(token.IsCancellationRequested)
            {
              return;
            }

            if(nestedObj == null)
            {
              continue;
            }

            if(nestedObj.IsBase(out var nestedBase))
            {
              await DeconstructObject(nestedLayer, nestedBase, converter, token);
            }
          }

          nestedLayer.ParentObjects();
          layer.Add(nestedLayer);
        }
        else
        {
          converter.Report.LogConversionError(new Exception($"Unhandled type {@base.speckle_type} was not converted"));
        }

        await UniTask.Yield();
      }
    }

    public static Base SceneToData(this SpeckleObjectBehaviour speckleObj, ISpeckleConverter converter, CancellationToken token)
    {
      var data = new Base();

      foreach(var layer in speckleObj.hierarchy.layers)
      {
        if(token.IsCancellationRequested)
          return data;

        data[layer.LayerName] = LayerToBase(layer, converter, token);
      }

      speckleObj.totalChildrenCount = (int)data.GetTotalChildrenCount();
      return data;
    }

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


    public static TBase SearchForTypeSync<TBase>(this Base obj, bool recursive) where TBase : Base
    {
      if(obj is TBase simpleCast) return simpleCast;

      foreach(var member in obj.GetMemberNames())
      {
        var nestedObj = obj[member];

        // 1. Direct cast for object type 
        if(nestedObj.IsBase(out TBase memberCast))
          return memberCast;

        // 2. Check if member is base type
        if(nestedObj.IsBase(out var nestedBase))
        {
          var objectToFind = nestedBase.SearchForTypeSync<TBase>(recursive);

          if(objectToFind != default(object)) return objectToFind;
        }
        else if(nestedObj.IsList(out List<object> nestedList))
        {
          foreach(var listObj in nestedList)
          {
            if(listObj.IsBase(out TBase castedListObjectType))
              return castedListObjectType;

            // if not set to recursive we dont look through any other objects
            if(!recursive) continue;

            // if its not a base object we turn around
            if(!listObj.IsBase(out Base nestedListBase)) continue;

            var objectToFind = nestedListBase.SearchForTypeSync<TBase>(true);

            if(objectToFind != default(object)) return objectToFind;
          }
        }
      }
      return null;
    }

    public static async UniTask<TBase> SearchForType<TBase>(this Base obj, bool recursive, CancellationToken token) where TBase : Base
    {
      if(obj is TBase simpleCast) return simpleCast;

      if(token.IsCancellationRequested) return null;

      foreach(var member in obj.GetMemberNames())
      {
        if(token.IsCancellationRequested) return null;

        var nestedObj = obj[member];

        // 1. Direct cast for object type 
        if(nestedObj.IsBase(out TBase memberCast))
          return memberCast;

        // 2. Check if member is base type
        if(nestedObj.IsBase(out var nestedBase))
        {
          var objectToFind = await nestedBase.SearchForType<TBase>(recursive, token);

          if(objectToFind != default)
            return objectToFind;
        }
        else if(nestedObj.IsList(out List<object> nestedList))
        {
          foreach(var listObj in nestedList)
          {
            if(listObj.IsBase(out TBase castedListObjectType))
              return castedListObjectType;

            // if not set to recursive we dont look through any other objects
            if(!recursive)
              continue;

            // if its not a base object we turn around
            if(!listObj.IsBase(out Base nestedListBase))
              continue;

            var objectToFind = await nestedListBase.SearchForType<TBase>(true, token);

            if(objectToFind != default)
              return objectToFind;
          }
        }

        await UniTask.Yield();
      }

      return null;
    }

  }

}
