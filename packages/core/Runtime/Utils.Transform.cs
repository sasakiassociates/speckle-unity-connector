using Speckle.Core.Kits;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Speckle.ConnectorUnity
{

  public static partial class Utils
  {
    public static double ScaleToNative(double value, string units)
      => value * Units.GetConversionFactor(units, Units.Meters);

    public static IEnumerable<Vector3> ToVectorArray(IReadOnlyCollection<double> arr, string units)
    {
      if(arr == null)
        throw new Exception("point array is not valid ");

      if(arr.Count % 3 != 0)
        throw new Exception("Array malformed: length%3 != 0.");

      var points = new Vector3[arr.Count / 3];
      var asArray = arr.ToArray();

      for(int i = 2, k = 0; i < arr.Count; i += 3)
        points[k++] = ToVector3(asArray[i - 2], asArray[i - 1], asArray[i], units);

      return points;
    }

    public static Vector3 ToVector3(double x, double y, double z, string units) => new(
      (float)Utils.ScaleToNative(x, units),
      (float)Utils.ScaleToNative(y, units),
      (float)Utils.ScaleToNative(z, units)
    );
  }

}
