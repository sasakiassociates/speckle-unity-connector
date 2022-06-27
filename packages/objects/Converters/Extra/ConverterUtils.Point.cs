using System;
using System.Collections.Generic;
using System.Linq;
using Objects.Geometry;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	public static partial class ConverterUtils

	{
		/// <summary>
		///   switch y and z
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="units"></param>
		/// <returns></returns>
		public static Vector3 VectorByCoordinates(double x, double y, double z, string units) =>
			new((float)ScaleToNative(x, units), (float)ScaleToNative(z, units), (float)ScaleToNative(y, units));

		public static Vector ToSpeckle(this Vector3 pos, bool flipYZ = true) => flipYZ ? new Vector(pos.x, pos.z, pos.y) : new Vector(pos.x, pos.y, pos.z);

		public static List<double> ToSpeckle(this IEnumerable<Vector3> points)
		{
			var res = new List<double>();

			if (points == null)
			{
				Debug.LogException(new Exception("point array is not valid "));
				return res;
			}

			foreach (var point in points)
			{
				res.Add(point.x);
				res.Add(point.y);
				res.Add(point.z);
			}

			return res;
		}

		public static Point ToPoint(this Vector3 pos, bool flipYZ = true) => flipYZ ? new Point(pos.x, pos.z, pos.y) : new Point(pos.x, pos.y, pos.z);

		public static IEnumerable<Vector3> ArrayToPoints(this IEnumerable<double> arr, string units)
		{
			if (arr == null)
				throw new Exception("point array is not valid ");

			if (arr.Count() % 3 != 0)
				throw new Exception("Array malformed: length%3 != 0.");

			var points = new Vector3[arr.Count() / 3];
			var asArray = arr.ToArray();
			for (int i = 2, k = 0; i < arr.Count(); i += 3)
				points[k++] = VectorByCoordinates(asArray[i - 2], asArray[i - 1], asArray[i], units);

			return points;
		}

		public static Vector3[] ArrayToPoints(this IEnumerable<Point> arr, string units)
		{
			if (arr == null)
				throw new Exception("point array is not valid ");

			var points = new Vector3[arr.Count()];
			var asArray = arr.ToArray();

			for (var i = 0; i < points.Count(); i++)
				points[i] = asArray[i].ToVector3();

			return points;
		}
		/// <summary>
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="units"></param>
		/// <param name="flipYZ"></param>
		/// <returns></returns>
		public static Vector3 ToVector3(double x, double y, double z, string units, bool flipYZ = true) =>
			// switch y and z
			flipYZ ?
				new Vector3(
					(float)ScaleToNative(x, units),
					(float)ScaleToNative(z, units),
					(float)ScaleToNative(y, units)) :
				new Vector3(
					(float)ScaleToNative(x, units),
					(float)ScaleToNative(y, units),
					(float)ScaleToNative(z, units));

		public static Vector3 ToVector3(this Point p, bool flipYZ = true) => flipYZ ?
			new Vector3(
				(float)ScaleToNative(p.x, p.units),
				(float)ScaleToNative(p.z, p.units),
				(float)ScaleToNative(p.y, p.units))
			:
			new Vector3(
				(float)ScaleToNative(p.x, p.units),
				(float)ScaleToNative(p.y, p.units),
				(float)ScaleToNative(p.z, p.units));
	}
}