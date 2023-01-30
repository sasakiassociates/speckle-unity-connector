using Speckle.ConnectorUnity.Models;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Speckle.ConnectorUnity
{

  public static partial class Utils
  {

    public static void Add(this SpeckleLayer layer, Base obj, ISpeckleConverter converter)
    {
      if(converter.ConvertToNative(obj) is GameObject o)
        layer.Add(o);
      else
        Debug.Log("Checking for layers");
    }

    public static SpeckleLayer ListToLayer(
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
          var obj = converter.ConvertToNative(@base);

          switch(obj)
          {
            case GameObject o:
              layer.Add(o);
              break;
            case MonoBehaviour o:
              layer.Add(o.gameObject);
              break;
            case Component o:
              layer.Add(o.gameObject);
              break;
            default:
              SpeckleUnity.Console.Warn($"Object converted to unity from speckle is not supported {obj.GetType()}");
              break;
          }
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


  }

}
