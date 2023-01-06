using System;
using Speckle.ConnectorUnity.Elements;
using System.Collections;
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

    Button _refresh;
    Toggle _submit;

    VisualElement _listContainer;
    ListView _list;
    
    protected override string fileName => "connector-card";

    protected override VisualElement BuildRoot()
    {
      // creates the root 
      base.BuildRoot();
    
      Tree.CloneTree(Root);

      _listContainer = Root.Q<VisualElement>("list-container", "speckle-element-list");
      _list = _listContainer.Q<ListView>();

      var accountControls = Root.Q<VisualElement>("account-controls");
      _refresh = accountControls.Q<Button>("refresh");

      _submit = accountControls.Q<Toggle>("submit");
      _submit.RegisterValueChangedCallback(ToggleAccountSelection);

      ShowStreamList();

      return Root;
    }

    void ToggleAccountSelection(ChangeEvent<bool> evt)
    {
      if(evt.newValue)
      {
        ShowAccountList();
      }
      else
      {
        ShowStreamList();
      }
    }

    void ShowAccountList()
    {


      // populate list with accounts
      ResetList(accountCard, Obj.accounts, "accounts", BindAccountItem);

      Debug.Log($"accounts count = {(Obj.accounts.Valid() ? Obj.accounts.Count : 0)}");


    }

    void ShowStreamList()
    {
      ResetList(streamCard, Obj.streams, "streams", BindStreamItem);

      Debug.Log($"stream count = {(Obj.streams.Valid() ? Obj.streams.Count : 0)}");
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

    void ResetList(VisualTreeAsset item, IList source, string bindingPath, Action<VisualElement, int> bindItem)
    {
      _list.ClearSelection();
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

      _listContainer.Add(_list);

      _list.Rebuild();
      _list.RefreshItems();

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
