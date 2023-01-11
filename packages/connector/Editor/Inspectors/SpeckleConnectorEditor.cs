using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Elements;
using Speckle.ConnectorUnity.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity
{


  [CustomEditor(typeof(SpeckleConnector))]
  public class SpeckleConnectorEditor : SpeckleEditor<SpeckleConnector>
  {


    int _selectedIndex;
    Button _refresh;
    Button _select;
    Toggle _submit;
    ConnectorState _state = ConnectorState.ShowingStreams;

    VisualElement _listContainer;

    ListView _list;

    enum ConnectorState
    {
      ShowingStreams,
      ShowingAccounts
    }

    const bool SHOW_OPEN_IN_NEW = true;
    const bool SHOW_OPERATIONS = false;
    const bool SHOW_DESCRIPTIONS = false;

    protected override string fileName => "connector-card";

    protected override void OnEnable()
    {
      base.OnEnable();
      Obj.OnInitialize += ResetList;
      Obj.OnStreamsLoaded += ResetList;
    }

    protected override VisualElement BuildRoot()
    {
      base.BuildRoot();

      _state = ConnectorState.ShowingStreams;
      Tree.CloneTree(Root);

      _listContainer = Root.Q<VisualElement>(className: SpeckleUss.Classes.Control.LIST);
      _list = _listContainer.Q<ListView>();

      var accountControls = Root.Q<VisualElement>("account-controls");
      _refresh = accountControls.Q<Button>("refresh");
      _refresh.clickable.clicked += ProcessRefresh;

      _submit = accountControls.Q<Toggle>("submit");
      _submit.RegisterValueChangedCallback(ProcessSubmit);

      _select = SpeckleUss.Prefabs.buttonWithIcon;
      _select.name = "select";
      _select.AddToClassList(SpeckleUss.Classes.Control.SELECT);
      _select.clickable.clicked += ProcessCreateReceiver;

      ResetList();

      return Root;
    }

    void ResetList()
    {
      IList source = default;
      string bindingPath;
      Action<VisualElement, int> bindItem;
      Func<VisualElement> makeItem;

      _selectedIndex = -1;

      void BindStreamItem(VisualElement e, int index)
      {
        if(!TryCheckElement(e, out SpeckleStreamListItem element))
        {
          Debug.Log("Element not found");
          return;
        }

        element.SetValueWithoutNotify(Obj.streams[index]);

        // if this is a fresh list no need to worry about checking list buttons
        if(_selectedIndex < 0)
        {
          return;
        }

        if(element.openInNewButton != null) element.openInNewButton.clickable.clicked -= ProcessOpenInNew;

        if(_selectedIndex == index)
        {
          if(_select != null && !element.HasControl(_select))
          {
            element.AddControl(_select);
          }

          element.displayOpenInNew = SHOW_OPEN_IN_NEW;
          element.displayDescription = SHOW_DESCRIPTIONS;
          element.displayOperations = SHOW_OPERATIONS;

          element.openInNewButton.clickable.clicked += ProcessOpenInNew;

        }
        else
        {
          if(_select != null && element.HasControl(_select))
          {
            element.RemoveControl(_select);
          }

          element.displayOpenInNew = false;
          element.displayDescription = false;
          element.displayOperations = false;

          element.displayOpenInNew = element.displayOperations = element.displayDescription = false;
        }

      }

      void BindAccountItem(VisualElement e, int index)
      {
        if(!TryCheckElement(e, out AccountElement element))
        {
          Debug.Log("Element not found");
          return;
        }

        element.SetValueWithoutNotify(Obj.accounts[index]);
      }

      VisualElement MakeAccountItem() => new AccountElement();

      VisualElement MakeStreamItem() => new SpeckleStreamListItem {displayOperations = false, displayOpenInNew = false, displayDescription = false};

      switch(_state)
      {
        case ConnectorState.ShowingAccounts:
          source = Obj.accounts;
          bindingPath = "accounts";
          bindItem = BindAccountItem;
          makeItem = MakeAccountItem;
          break;
        case ConnectorState.ShowingStreams:
          source = Obj.streams;
          bindingPath = "streams";
          bindItem = BindStreamItem;
          makeItem = MakeStreamItem;
          break;
        default:
          Debug.LogWarning("Unhandled state being sent " + _state);
          return;
      }
      _list?.ClearSelection();
      _listContainer.Remove(_list);

      _list = new ListView(source)
      {
        // build stream list 
        makeItem = makeItem,
        bindItem = bindItem,
        bindingPath = bindingPath,
        selectionType = SelectionType.Single,
        itemsSource = source
      };

      _list.onSelectionChange += ProcessSelectionFromList;

      _listContainer.Add(_list);

      _list.Rebuild();
      _list.RefreshItems();

    }


  #region handlers for control events

    void ProcessRefresh()
    {
      switch(_state)
      {

        case ConnectorState.ShowingStreams:
          Obj.Initialize().Forget();
          break;
        case ConnectorState.ShowingAccounts:
          Obj.LoadAccounts();
          break;
        default:
          Debug.LogWarning("Unhandled state being sent " + _state);
          return;
      }

      ResetList();
    }

    void ProcessSubmit(ChangeEvent<bool> evt)
    {
      if(evt.newValue)
      {
        _state = ConnectorState.ShowingAccounts;
      }
      else
      {
        _state = ConnectorState.ShowingStreams;

        // if an account is selected make sure to send that back before rebuilding the list
        if(_selectedIndex >= 0)
        {
          Obj.Initialize(Obj.accounts[_selectedIndex].source).Forget();
          return;
        }
      }

      ResetList();
    }



    void ProcessOpenInNew()
    {
      Debug.Log("Open in New");
    }

    void ProcessCreateSender() => CreateOperation(false);

    void ProcessCreateReceiver() => CreateOperation(true);

    void CreateOperation(bool receive)
    {
      Debug.Log(!receive ? "Create Sender" : "Create Receiver");

      if(receive)
      {
        Obj.CreateReceiver().Forget();
      }
      else
      {
        Obj.CreateSender().Forget();
      }

      _list.ClearSelection();
      _selectedIndex = -1;
    }


    void ProcessSelectionFromList(IEnumerable<object> _)
    {
      _selectedIndex = _list.selectedIndex;

      if(_state == ConnectorState.ShowingStreams)
      {
        Obj.SetSelectedStream(_selectedIndex);
      }
      _list.Rebuild();

    }

  #endregion






  #region utils

    bool TryCheckElement<TElement>(VisualElement e, out TElement element) where TElement : VisualElement
    {
      element = null;

      // is the item we want 
      if(e is TElement cast)
      {
        element = cast;
      }
      else
      {
        element = e.Q<TElement>();
      }

      return element != null;
    }

  #endregion

  }

}
