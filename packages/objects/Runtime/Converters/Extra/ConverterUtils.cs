using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Speckle.ConnectorUnity.Mono;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	public static partial class ConverterUtils
	{
		/// <summary>
		///   the default Unity units are meters
		/// </summary>
		public const string ModelUnits = Units.Meters;

		public static Dictionary<string, object> FetchProps<TBase>(this TBase @base) where TBase : Base
		{
			var props = typeof(TBase).GetProperties(
				BindingFlags.Instance | BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.Public).Select(x => x.Name).ToList();
			return @base.GetMembers().Where(x => !props.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
		}

		public static void AttachUnityProperties(this SpeckleProperties props, Base @base, HashSet<string> excludedProps)
		{
			if (@base == null || props?.Data == null)
				return;

			foreach (var key in props.Data.Keys)
				try
				{
					var toggle = false;
					foreach (var prop in excludedProps)
						if (prop.Contains(key))
						{
							SpeckleUnity.Console.Log($"Skipping prop {prop}");
							toggle = true;
							break;
						}

					if (!toggle)
						@base[key] = props.Data[key];
				}
				catch (SpeckleException e)
				{
					SpeckleUnity.Console.Warn(e.Message);
				}
		}

		public static double ScaleToNative(double value, string units) => value * Units.GetConversionFactor(units, ModelUnits);

		public static bool Valid(this string name) => !string.IsNullOrEmpty(name);

		public static int ToIntColor(this Color c) => System.Drawing.Color
			.FromArgb(Convert.ToInt32(c.r * 255), Convert.ToInt32(c.r * 255), Convert.ToInt32(c.r * 255))
			.ToArgb();

		public static Color ToUnityColor(this int c)
		{
			var argb = System.Drawing.Color.FromArgb(c);
			return new Color(argb.R / 255.0f, argb.G / 255.0f, argb.B / 255.0f);
		}
	}
}