using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Elements;
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


    [SerializeField] VisualTreeAsset streamCard;
    [SerializeField] VisualTreeAsset accountCard;

    int _selectedIndex;
    Button _refresh;
    Toggle _submit;
    ConnectorState _state = ConnectorState.ShowingStreams;

    VisualElement _listContainer;

    ListView _list;

    enum ConnectorState
    {
      ShowingStreams,
      ShowingAccounts
    }

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

      _listContainer = Root.Q<VisualElement>("list-container", "speckle-element-list");
      _list = _listContainer.Q<ListView>();

      var accountControls = Root.Q<VisualElement>("account-controls");
      _refresh = accountControls.Q<Button>("refresh");

      _submit = accountControls.Q<Toggle>("submit");
      _submit.RegisterValueChangedCallback(ProcessSubmitAction);

      ResetList();

      return Root;
    }

    void ProcessSubmitAction(ChangeEvent<bool> evt)
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

    void ProcessSelectionFromList(IEnumerable<object> _)
    {
      _selectedIndex = _list.selectedIndex;

      switch(_state)
      {
        case ConnectorState.ShowingAccounts:

          break;
        case ConnectorState.ShowingStreams:
          break;
        default:
          return;
      }
    }


    void BindStreamItem(VisualElement e, int index)
    {
      if(!TryCheckElement(e, out SpeckleStreamListItem element))
      {
        Debug.Log("Element not found");
        return;
      }

      element.SetValueWithoutNotify(Obj.streams[index]);

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

    void ResetList()
    {
      IList source = default;
      VisualTreeAsset item = default;
      string bindingPath;
      Action<VisualElement, int> bindItem;
      _selectedIndex = -1;

      switch(_state)
      {
        case ConnectorState.ShowingAccounts:
          source = Obj.accounts;
          item = accountCard;
          bindingPath = "accounts";
          bindItem = BindAccountItem;
          break;
        case ConnectorState.ShowingStreams:
          source = Obj.streams;
          item = streamCard;
          bindingPath = "streams";
          bindItem = BindStreamItem;
          break;
        default:
          return;
      }
      _list?.ClearSelection();
      _listContainer.Remove(_list);

      _list = new ListView(source)
      {
        // build stream list 
        makeItem = item.CloneTree,
        bindItem = bindItem,
        bindingPath = bindingPath,
        selectionType = SelectionType.Single,
        itemsSource = source
      };

      _list.onSelectionChange += ProcessSelectionFromList;

      _listContainer.Add(_list);

      _list.Rebuild();
      _list.RefreshItems();
      Debug.Log("Rebuilding List");


    }


    //
    // void SetupList()
    // {
    //   VisualElement makeItem()
    //   {
    //     var card = new VisualElement();
    //     streamCard.CloneTree(card);
    //     return card;
    //   }
    //
    //   void bindItem(VisualElement e, int i)
    //   {
    //     var stream = obj.Streams[i];
    //
    //     // e.Q<Label>("title").text = stream.Name;
    //     // e.Q<Label>("id").text = stream.Id;
    //     // e.Q<Label>("description").text = stream.Description;
    //
    //     var isActive = FindInt(_fields.streamIndex) == i;
    //
    //     e.style.backgroundColor = isActive ? new StyleColor(Color.white) : new StyleColor(Color.clear);
    //
    //     var ops = e.Q<VisualElement>("operation-container");
    //     if (ops == null)
    //     {
    //       Debug.Log("No container found");
    //       return;
    //     }
    //
    //     ops.visible = isActive;
    //
    //     // unbind all objects first just in case
    //     ops.Q<Button>("open-button").clickable.clickedWithEventInfo -= obj.OpenStreamInBrowser;
    //     ops.Q<Button>("send-button").clickable.clickedWithEventInfo -= obj.CreateSender;
    //     ops.Q<Button>("receive-button").clickable.clickedWithEventInfo -= obj.CreateReceiver;
    //
    //     if (isActive)
    //     {
    //       ops.Q<Button>("open-button").clickable.clickedWithEventInfo += obj.OpenStreamInBrowser;
    //       ops.Q<Button>("send-button").clickable.clickedWithEventInfo += obj.CreateSender;
    //       ops.Q<Button>("receive-button").clickable.clickedWithEventInfo += obj.CreateReceiver;
    //     }
    //   }
    //
    //   streamList = root.Q<ListView>("stream-list");
    //
    //   streamList.bindItem = bindItem;
    //   streamList.makeItem = makeItem;
    //   streamList.fixedItemHeight = 50f;
    //
    //   SetAndRefreshList();
    //
    //   // Handle updating all list view items after selection 
    //   streamList.RegisterCallback<ClickEvent>(_ => streamList.RefreshItems());
    //
    //   // Pass new selection back to object
    //   streamList.onSelectedIndicesChange += i => obj.SetStream(i.FirstOrDefault());
    // }
    //
  }

}
