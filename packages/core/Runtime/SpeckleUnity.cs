using Speckle.Core.Kits;
using System;
using System.Collections.Concurrent;
using UnityEngine;
using Co = Speckle.ConnectorUnity.SpeckleUnity.Console;

namespace Speckle.ConnectorUnity
{

  public static class SpeckleUnity
  {
    public const string APP = HostApplications.Unity.Name;

    public const string NAMESPACE = "Speckle/";

    public static class Categories
    {
      public const string COMPS = NAMESPACE + "Components/";

      public const string CONVERTERS = NAMESPACE + "Converters/";
    }

    public static class Folders
    {
      public const string ParentName = "Assets";

      public const string BaseName = "Speckle";

      public const string StreamsName = "Streams";

      public const string BasePath = ParentName + "/" + BaseName + "/";

      public const string StreamsPath = BasePath + StreamsName + "/";


    }



    public static void LogProgress(ConcurrentDictionary<string, int> args)
    {
      // from speckle gh connector
      var total = 0.0f;
      foreach(var kvp in args)
      {
        //NOTE: progress set to indeterminate until the TotalChildrenCount is correct
        total += kvp.Value;
      }

      var progress = total / args.Keys.Count;
      SpeckleUnity.Console.Log(progress.ToString());
    }

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
