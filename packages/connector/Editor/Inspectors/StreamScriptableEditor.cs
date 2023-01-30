using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity;
using Speckle.ConnectorUnity.Elements;
using Speckle.ConnectorUnity.Models;
using System.Collections.Generic;
using System.Linq;
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
  SearchUrlElement _searchUrl;
  StreamElement _streamElement;

  protected override string fileName => "stream-scriptable-card";

  void OnDisable()
  {
    _branchDropField?.UnregisterValueChangedCallback(BranchChangeCallback);
    _commitsDropField?.UnregisterValueChangedCallback(CommitChangeCallback);

  }

  public override VisualElement CreateInspectorGUI()
  {
    if(Tree == null) return base.CreateInspectorGUI();

    Root = new VisualElement();
    Tree.CloneTree(Root);

    _searchUrl = Root.Q<SearchUrlElement>();
    _searchUrl.onSearch += Search;
    _searchUrl.Q<TextField>().bindingPath = "originalUrlInput";

    _streamElement = Root.Q<StreamElement>();
    _streamElement.SetValueWithoutNotify(Obj.SourceStream);
    _streamElement.SetPreviewTexture(Obj.Preview);

    Obj.OnPreviewSet += e => _streamElement.SetPreviewTexture(e);

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

    foreach(var o in values)
    {
      if(o.Valid())
      {
        hashSet.Add(o);
      }
    }

    if(activeValue.Valid())
    {
      hashSet.Add(activeValue);
    }

    field.Clear();
    field.choices = hashSet.ToList();
    field.SetValueWithoutNotify(hashSet.LastOrDefault());
  }

  void Search(string url)
  {
    Debug.Log($"Searching {url}");
    UniTask.Create(async () =>
    {
      await Obj.Initialize(url);
      _streamElement.SetValueWithoutNotify(Obj.SourceStream);
    });

    // Obj.Initialize(url).ContinueWith(() => { Debug.Log("Continue with call"); }).Forget();

  }

}
