using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Speckle.ConnectorUnity
{

  public static partial class Utils
  {
    public static int Check(this IList list, int index) => list.Valid(index) ? index : 0;
    public static bool Valid(this IList list) => list.Valid(0);
    public static bool Valid(this IList list, int count) => list != null && count >= 0 && count < list.Count;
    public static bool Valid(this ICollection list) => list.Valid(0);
    public static bool Valid(this ICollection list, int count) => list != null && count >= 0 && count < list.Count;
    public static bool Valid(this string name) => !string.IsNullOrEmpty(name);

    public static void SafeDestroy(UnityEngine.Object obj)
    {
      if(Application.isPlaying)
        UnityEngine.Object.Destroy(obj);

      else
        UnityEngine.Object.DestroyImmediate(obj);
    }

    public static bool IsList(this object obj, out List<object> list)
    {
      list = new List<object>();

      if(obj.IsList())
        list = ((IEnumerable)obj).Cast<object>().ToList();

      return list.Any();
    }

    public static bool IsList(this object obj) => obj != null && obj.GetType().IsList();

    public static bool IsList(this Type type)
    {
      if(type == null)
        return false;

      return typeof(IEnumerable).IsAssignableFrom(type) && !typeof(IDictionary).IsAssignableFrom(type) && type != typeof(string);
    }

    public static bool TryGetElements(this Base @base, out List<Base> items)
    {
      items = null;

      if(@base["elements"] is List<Base> l && l.Any())
        items = l;

      return items != null;
    }

    public static bool IsBase(this object value, out Base @base)
    {
      @base = null;

      if(value != null && !value.GetType().IsSimpleType() && value is Base o)
        @base = o;

      return @base != null;
    }

    public static bool IsBase<TBase>(this object value, out TBase @base) where TBase : Base
    {
      @base = default;

      if(value != null && !value.GetType().IsSimpleType() && value is TBase o)
        @base = o;

      return @base != null;
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

    public static bool Valid(this SpeckleAccount obj) => obj?.Source != null && Valid(obj.Source);
  }

}
