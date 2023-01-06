using Speckle.ConnectorUnity;
using UnityEditor;
using UnityEngine.UIElements;

public abstract class SpeckleEditor<TObj> : Editor where TObj : UnityEngine.Object
{

  protected TObj Obj;
  protected VisualTreeAsset Tree;
  protected VisualElement Root;

  /// <summary>
  /// points to the uxml path 
  /// </summary>
  protected virtual string path => GUIHelper.Folders.CONNECTOR_UXML;

  /// <summary>
  /// name of .uxml file to load to the editor 
  /// </summary>
  protected abstract string fileName { get; }

  protected virtual void OnEnable()
  {
    Obj = (TObj)target;
    Tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + fileName + ".uxml");
  }

  public override VisualElement CreateInspectorGUI()
  {
    return Tree == null ? base.CreateInspectorGUI() : BuildRoot();

  }

  /// <summary>
  /// Creates a new visual element to <see cref="Root"/>
  /// </summary>
  /// <returns>returns <see cref="Root"/></returns>
  protected virtual VisualElement BuildRoot()
  {
    Root = new VisualElement();
    return Root;
  }


}
