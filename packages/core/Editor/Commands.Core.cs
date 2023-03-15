using Speckle.ConnectorUnity.Converter;
using Speckle.ConnectorUnity.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Su = Speckle.ConnectorUnity.SpeckleUnity;

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
    [MenuItem(Su.NAMESPACE + "Default Speckle Folder", false, 0)]
    public static void CreateSpeckleAssetFolder()
    {
      Selection.activeObject = AssetDatabase.LoadAssetAtPath(SafeGetAssetFolder(), typeof(Object));
    }

  #region Converters

    [MenuItem(Su.NAMESPACE + "Scriptable Converter Manager", false, 50)]
    public static void GetScriptableConverterManager()
    {
      SafeGetAssetFolder();

      Selection.activeObject = AssetDatabase.LoadAssetAtPath<ConverterManager>(SafeGetConverterManager());

    }
    [MenuItem(Su.NAMESPACE + "Search and Add Converters", false, 51)]
    public static void SearchAndAddConverters()
    {
      SafeGetAssetFolder();

      var item = AssetDatabase.LoadAssetAtPath<ConverterManager>(SafeGetConverterManager());

      item.activeConverters = FindAllConverters();

      Selection.activeObject = item;

    }

    [MenuItem(Su.NAMESPACE + "Create Stream Object", false, 101)]
    public static void CreateSpeckleStreamObject()
    {
      var item = ScriptableObject.CreateInstance<SpeckleStreamObject>();

      var path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

      if (string.IsNullOrEmpty(path))
      {
        if (!AssetDatabase.IsValidFolder(Su.Folders.STREAMS_PATH))
        {
          AssetDatabase.CreateFolder(Su.Folders.PARENT_NAME + "/" + Su.Folders.BASE_NAME, Su.Folders.STREAMS_NAME);
        }

        path = Su.Folders.STREAMS_PATH;
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
      var guids = AssetDatabase.FindAssets($"t:{typeof(ConverterManager)}", new[] { Su.Folders.BASE_PATH });
      string path;

      if (guids != null && guids.Any())
      {
        path = AssetDatabase.GUIDToAssetPath(guids.FirstOrDefault());
      }
      else
      {
        path = Su.Folders.BASE_PATH + nameof(ConverterManager) + ".asset";
        var item = ScriptableObject.CreateInstance<ConverterManager>();
        AssetDatabase.CreateAsset(item, path);
        AssetDatabase.SaveAssets();

        item.activeConverters = FindAllConverters();

      }

      return path;
    }

    public static string SafeGetAssetFolder()
    {
      if (!AssetDatabase.IsValidFolder(Su.Folders.BASE_PATH))
      {
        AssetDatabase.CreateFolder(Su.Folders.PARENT_NAME, Su.Folders.BASE_NAME);
      }

      return Su.Folders.BASE_PATH;
    }

 
  }

}
