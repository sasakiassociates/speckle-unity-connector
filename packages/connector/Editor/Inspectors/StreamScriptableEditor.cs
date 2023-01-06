using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity;
using Speckle.ConnectorUnity.Elements;
using Speckle.ConnectorUnity.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(SpeckleStreamObject))]
public class StreamScriptableEditor : SpeckleEditor<SpeckleStreamObject>
{
  VisualElement _streamContainer;

  Button _searchButton;
  DropdownField _branchDropField;
  DropdownField _commitsDropField;

  protected override string fileName => "stream-scriptable-card";


  void OnDisable()
  {
    _branchDropField?.UnregisterValueChangedCallback(BranchChangeCallback);
    _commitsDropField?.UnregisterValueChangedCallback(CommitChangeCallback);

    if (_searchButton != null)
      _searchButton.clickable.clicked -= Search;
  }

  public override VisualElement CreateInspectorGUI()
  {
    if (Tree == null)
      return base.CreateInspectorGUI();

    Root = new VisualElement();
    Tree.CloneTree(Root);

    _searchButton = Root.Q<Button>("search-button");
    _searchButton.clickable.clicked += Search;

    _streamContainer = Root.Q<VisualElement>("stream-info-container");

    var _showToggle = Root.Q<Toggle>("show-stream");

    _showToggle.RegisterValueChangedCallback(e => { _streamContainer.visible = e.newValue; });

    var streamPreview = Root.Q<TexturePreviewElement>("stream-preview");
    Obj.OnPreviewSet += e => streamPreview.value = e;

    return Root;
  }

  void BranchChangeCallback(ChangeEvent<string> e)
  {
    Obj.SetBranch(e.newValue).Forget();
  }

  void CommitChangeCallback(ChangeEvent<string> e)
  {
    Obj.SetCommit(e.newValue).Forget();
  }

  static void CompileDropDown(DropdownField field, List<string> values, string activeValue)
  {
    var hashSet = new HashSet<string>();

    foreach (var o in values)
    {
      if (o.Valid())
      {
        hashSet.Add(o);
      }
    }

    if (activeValue.Valid())
    {
      hashSet.Add(activeValue);
    }

    field.Clear();
    field.choices = hashSet.ToList();
    field.SetValueWithoutNotify(hashSet.LastOrDefault());
  }

  void Search()
  {
    Debug.Log("Starting Search");
    Obj.Initialize(Obj.OriginalUrlInput).ContinueWith(() => { Debug.Log("Continue with call"); }).Forget();
  }

}
