using Cysharp.Threading.Tasks;
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

  SearchUrlElement _searchUrl;
  StreamDataElement _streamElement;

  protected override string fileName => "stream-scriptable-card";


  public override VisualElement CreateInspectorGUI()
  {
    if(Tree == null) return base.CreateInspectorGUI();

    Root = new VisualElement();
    Tree.CloneTree(Root);

    _searchUrl = Root.Q<SearchUrlElement>();
    _searchUrl.onSearch += Search;
    _searchUrl.Q<TextField>().bindingPath = "originalUrlInput";

    _streamElement = Root.Q<StreamDataElement>();
    _streamElement.OnBranchInputChange += HandleBranchInputChange;
    _streamElement.OnCommitInputChange += HandleCommitInputChange;
    _streamElement.SetValueWithoutNotify(Obj.Source);
    _streamElement.SetPreviewTexture(Obj.Preview);

    Obj.OnPreviewSet += e => _streamElement.SetPreviewTexture(e);

    return Root;
  }

  void HandleBranchInputChange(string branchName)
  {
    Debug.Log($"Handling new branch change to {branchName}");
    Obj.SetBranch(branchName).Forget();
  }

  void HandleCommitInputChange(string commitId)
  {
    Debug.Log($"Handling new commit change to {commitId}");
    Obj.SetCommit(commitId).Forget();
  }

  void Search(string url)
  {
    Debug.Log($"Searching {url}");
    UniTask.Create(async () =>
    {
      await Obj.Initialize(url);
      _streamElement.SetValueWithoutNotify(Obj.Source);
    });

  }

}
