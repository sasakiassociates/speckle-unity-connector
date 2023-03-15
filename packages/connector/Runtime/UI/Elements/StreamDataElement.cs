using Speckle.ConnectorUnity.Ops;
using Speckle.ConnectorUnity.UI;
using Speckle.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{

  public class StreamDataElement : StreamElement
  {

    public new class UxmlFactory : UxmlFactory<StreamDataElement, StreamDataElementTrait>
    { }

    DropdownField _branchesDrop;
    DropdownField _commitsDrop;
    VisualElement _dataContainer;

    public event Action<string> OnBranchInputChange;
    public event Action<string> OnCommitInputChange;
    // public event Action<SpeckleBranch> OnBranchSet;
    // public event Action<SpeckleCommit> OnCommitSet;

    (bool branches, bool commits) _show;

    /// <summary>
    /// Optional class name to attached to the element 
    /// </summary>
    protected override string elementClass => SpeckleUss.Classes.Models.STREAM_DATA;

    /// <summary>
    /// Name of <seealso cref="StyleSheet"/> without the extension type
    /// </summary>
    protected override string styleSheetName => "stream-data-card";


    protected override void ConstructControls()
    {
      base.ConstructControls();
      _dataContainer = SpeckleUss.Prefabs.containerRow;
      _dataContainer.name = SpeckleUss.Classes.CONTAINER + "__data";

      this.Add(_dataContainer);
    }

    public override void SetValueWithoutNotify(SpeckleStream newValue)
    {
      base.SetValueWithoutNotify(newValue);

      value.OnBranchSet += HandleNewBranch;
      value.OnCommitSet += HandleNewCommit;

      UpdateBranch();
      UpdateCommits();
    }

    void HandleNewBranch(SpeckleBranch branch)
    {
      if(_commitsDrop == null) return;

      var commits = branch.commits.Valid() ? branch.commits.Where(x => x != null).Select(x => x.id).ToList() : new List<string>(){"nada"};
      Debug.Log($"Trying to set commits now from branch change of {branch.name} with total commits found={commits.Count}");
      _commitsDrop.index = 0;

      _commitsDrop.choices = commits;
    }

    void HandleNewCommit(SpeckleCommit commit)
    {
      Debug.Log("Handling new commit for stream data");
    }

    void UpdateBranch()
    {
      if(_branchesDrop != null)
      {
        if(value.Branches.Valid())
        {
          _branchesDrop.choices =
            value.Branches.Where(x => x != null).Select(x => x.name).ToList();
        }
        else if(value.Branch != null)
        {
          _branchesDrop.choices = new List<string> {value.Branch.name};
        }

        _branchesDrop.index = 0;
      }
    }


    void UpdateCommits()
    {

      Debug.Log($"Updating commits for data element");
      if(_commitsDrop != null)
      {
        if(value.Commits.Valid())
        {
          _commitsDrop.choices =
            value.Commits.Where(x => x != null).Select(x => x.id).ToList();
        }
        else if(value.Commit != null)
        {
          _commitsDrop.choices = new List<string> {value.Commit.id};
        }

        _commitsDrop.index = 0;
      }

    }

    public bool ShowBranches
    {
      get => _show.branches;
      set
      {
        if(value)
        {
          if(_branchesDrop != null) return;

          _branchesDrop = new DropdownField {name = "branches"};
          _branchesDrop.AddToClassList(SpeckleUss.Classes.Control.DROPDOWN);
          _branchesDrop.RegisterValueChangedCallback(evt => OnBranchInputChange?.Invoke(evt.newValue));
          _dataContainer.Insert(0, _branchesDrop);

        }
        else if(_branchesDrop != null)
        {
          _dataContainer.Remove(_branchesDrop);
          _branchesDrop = null;
        }

        _show.branches = value;
      }

    }

    public bool ShowCommits
    {
      get => _show.commits;
      set
      {
        if(value)
        {

          _commitsDrop = new DropdownField {name = "commits"};
          _commitsDrop.AddToClassList(SpeckleUss.Classes.Control.DROPDOWN);
          _commitsDrop.RegisterValueChangedCallback(evt => OnCommitInputChange?.Invoke(evt.newValue));
          _dataContainer.Add(_commitsDrop);


        }
        else if(_commitsDrop != null)
        {
          _dataContainer.Remove(_commitsDrop);
          _commitsDrop = null;
        }

        _show.commits = value;
      }
    }



  }

}
