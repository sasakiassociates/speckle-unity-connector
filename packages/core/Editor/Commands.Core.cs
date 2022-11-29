using Speckle.ConnectorUnity.Converter;
using Speckle.ConnectorUnity.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using UnityEditor;
using UnityEngine;

namespace Speckle.ConnectorUnity
{

  [InitializeOnLoad]
  public static class CoreCommands
  {
    static CoreCommands()
    {
      SafeGetAssetFolder();
      SafeGetConverterManager();
    }
    [MenuItem(SpeckleUnity.NAMESPACE + "Default Speckle Folder", false, 0)]
    public static void CreateSpeckleAssetFolder()
    {
      Selection.activeObject = AssetDatabase.LoadAssetAtPath(SafeGetAssetFolder(), typeof(Object));
    }
#region Converters

    [MenuItem(SpeckleUnity.NAMESPACE + "Scriptable Converter Manager", false, 50)]
    public static void GetScriptableConverterManager()
    {
      SafeGetAssetFolder();

      Selection.activeObject = AssetDatabase.LoadAssetAtPath<ConverterManager>(SafeGetConverterManager());

    }
    [MenuItem(SpeckleUnity.NAMESPACE + "Search and Add Converters", false, 51)]
    public static void SearchAndAddConverters()
    {
      SafeGetAssetFolder();

      var item = AssetDatabase.LoadAssetAtPath<ConverterManager>(SafeGetConverterManager());

      item.activeConverters = FindAllConverters();

      Selection.activeObject = item;

    }

    [MenuItem(SpeckleUnity.NAMESPACE + "Create Stream Object", false, 101)]
    public static void CreateSpeckleStreamObject()
    {
      var item = ScriptableObject.CreateInstance<SpeckleStreamObject>();

      var path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

      if (string.IsNullOrEmpty(path))
      {
        if (!AssetDatabase.IsValidFolder(Folders.StreamsPath))
        {
          AssetDatabase.CreateFolder(Folders.ParentName + "/" + Folders.BaseName, Folders.StreamsName);
        }
        
        path = Folders.StreamsPath;
      }
      else if (path.Contains("."))
      {
        path = path.Remove(path.LastIndexOf('/'));
        path += "/";
      }

      AssetDatabase.CreateAsset(item, path + nameof(SpeckleStreamObject) + ".asset");
      AssetDatabase.SaveAssets();

      Selection.activeObject = item;
    }

#endregion
    public static List<ScriptableConverter> FindAllConverters()
    {
      var items = new List<ScriptableConverter>();

      var guids = AssetDatabase.FindAssets($"t:{typeof(ScriptableConverter)}");

      if (guids == null) return items;

      foreach (var g in guids)
      {
        items.Add(AssetDatabase.LoadAssetAtPath<ScriptableConverter>(AssetDatabase.GUIDToAssetPath(g)));
      }

      return items;
    }

    public static string SafeGetConverterManager()
    {
      var guids = AssetDatabase.FindAssets($"t:{typeof(ConverterManager)}", new[] { Folders.BasePath });
      string path;

      if (guids != null && guids.Any())
      {
        path = AssetDatabase.GUIDToAssetPath(guids.FirstOrDefault());
      }
      else
      {
        path = Folders.BasePath + nameof(ConverterManager) + ".asset";
        var item = ScriptableObject.CreateInstance<ConverterManager>();
        AssetDatabase.CreateAsset(item, path);
        AssetDatabase.SaveAssets();

        item.activeConverters = FindAllConverters();

      }

      return path;
    }

    public static string SafeGetAssetFolder()
    {
      if (!AssetDatabase.IsValidFolder(Folders.BasePath))
      {
        AssetDatabase.CreateFolder(Folders.ParentName, Folders.BaseName);
      }

      return Folders.BasePath;
    }

    static class Folders
    {
      internal const string ParentName = "Assets";

      internal const string BaseName = "Speckle";

      internal const string StreamsName = "Streams";

      public const string BasePath = ParentName + "/" + BaseName + "/";

      public const string StreamsPath = BasePath + StreamsName + "/";



    }
  }

}
