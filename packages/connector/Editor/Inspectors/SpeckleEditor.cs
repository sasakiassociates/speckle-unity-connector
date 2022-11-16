using Speckle.ConnectorUnity;
using Speckle.ConnectorUnity.Models;
using System;
using UnityEditor;
using UnityEngine.UIElements;

public abstract class SpeckleEditor<TObj> : Editor where TObj : UnityEngine.Object
{
  
  protected TObj obj;
  protected VisualTreeAsset tree;
  protected VisualElement root;

  /// <summary>
  /// points to the uxml path 
  /// </summary>
  protected virtual string path => GUIHelper.Folders.CONNECTOR_UXML;

  protected abstract string fileName { get; }

  protected virtual void OnEnable()
  {
    obj = (TObj)target;
    tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + fileName + ".uxml");
  }
}
