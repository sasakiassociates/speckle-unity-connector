using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Speckle.ConnectorUnity.Ops;
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

		public static bool IsList(this object @object) => @object != null && @object.GetType().IsList();

		public static bool IsList(this Type type)
		{
			if (type == null) return false;

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

			if (!value.GetType().IsSimpleType() && value is Base o)
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

				if (item.IsBase(out var @base))
				{
					if (converter.CanConvertToNative(@base) && converter.ConvertToNative(@base) is GameObject o) layer.Add(o);
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

			layer.SetObjectParent(layer.transform);

			return layer;
		}
	}
}