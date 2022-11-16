using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Speckle.ConnectorUnity.Models;
using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Speckle.ConnectorUnity
{
  public static partial class SpeckleUnity
  {
    public static int Check(this IList list, int index) => list.Valid(index) ? index : 0;

    public static bool Valid(this IList list) => list.Valid(0);

    public static bool Valid(this IList list, int count) => list != null && count >= 0 && count < list.Count;

    public static bool Valid(this ICollection list) => list.Valid(0);

    public static bool Valid(this ICollection list, int count) => list != null && count >= 0 && count < list.Count;

    public static bool Valid(this string name) => !string.IsNullOrEmpty(name);

    public static void SafeDestroy(Object obj)
    {
      if (Application.isPlaying)
        Object.Destroy(obj);

      else
        Object.DestroyImmediate(obj);
    }

    public static bool IsList(this object obj, out List<object> list)
    {
      list = new List<object>();

      if (obj.IsList())
        list = ((IEnumerable)obj).Cast<object>().ToList();

      return list.Any();
    }

    public static bool IsList(this object obj) => obj != null && obj.GetType().IsList();

    public static bool IsList(this Type type)
    {
      if (type == null)
        return false;

      return typeof(IEnumerable).IsAssignableFrom(type) && !typeof(IDictionary).IsAssignableFrom(type) && type != typeof(string);
    }

    public static bool TryGetElements(this Base @base, out List<Base> items)
    {
      items = null;

      if (@base["elements"] is List<Base> l && l.Any())
        items = l;

      return items != null;
    }

    public static bool IsBase(this object value, out Base @base)
    {
      @base = null;

      if (value != null && !value.GetType().IsSimpleType() && value is Base o)
        @base = o;

      return @base != null;
    }

    public static bool IsBase<TBase>(this object value, out TBase @base) where TBase : Base
    {
      @base = default;

      if (value != null && !value.GetType().IsSimpleType() && value is TBase o)
        @base = o;

      return @base != null;
    }

    public static void Add(this SpeckleLayer layer, Base obj, ISpeckleConverter converter)
    {
      if (converter.ConvertToNative(obj) is GameObject o)
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

      foreach (var item in data)
      {
        if (token.IsCancellationRequested)
          return layer;

        if (item == null)
          continue;

        if (item.IsBase(out var @base) && converter.CanConvertToNative(@base))
        {
          var obj = converter.ConvertToNative(@base);

          switch (obj)
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
        else if (item.IsList())
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

      if (parent != null)
        layer.transform.SetParent(parent);

      layer.ParentObjects(layer.transform);

      return layer;
    }

    /// <summary>
    /// Returns true if <paramref name="obj"/> is not null along with <see cref="SpeckleObject.id"/> and <see cref="SpeckleObject.speckleType"/>  are not empty
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Valid(this SpeckleObject obj) => obj != null && obj.id.Valid() && obj.speckleType.Valid();

    /// <summary>
    /// Returns true if <paramref name="obj"/> is not null and  <see cref="Stream.id"/> contains something  
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Valid(this Stream obj) => obj != null && obj.id.Valid();

    /// <summary>
    /// Returns true if <paramref name="obj"/> is not null and  <see cref="Commit.id"/> contains something  
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Valid(this Commit obj) => obj != null && obj.id.Valid();

    /// <summary>
    /// Returns true if <paramref name="obj"/> is not null and  <see cref="Branch.id"/> and <see cref="Branch.name"/> contains something  
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Valid(this Branch obj) => obj != null && obj.id.Valid() && obj.name.Valid();


    /// <summary>
    /// Returns true if <paramref name="obj"/> is not null along with <see cref="Account.id"/>, <see cref="Account.token"/>,and <see cref="Account.serverInfo.url"/> are not empty
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Valid(this Account obj) => obj != null && obj.id.Valid() && obj.token.Valid() && obj.serverInfo.url.Valid();

    public static bool Valid(this SpeckleAccount obj) => obj?.source != null && obj.source.Valid();


    public static class Console
    {
      public const string title = "speckle-connector:";

      public static void Exception(Exception exception) => Debug.LogException(exception);

      public static void Log(string msg) => Debug.Log(title + " " + msg);

      public static void Warn(string message) => Debug.LogWarning(title + message);

      public static void Error(string msg) => Debug.LogError(title + " " + msg);
    }
  }
}
