using Speckle.ConnectorUnity.Ops;
using Speckle.ConnectorUnity.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{

  public class SpeckleStreamListItem : BindableElement, INotifyValueChanged<SpeckleStream>
  {
    public new class UxmlFactory : UxmlFactory<SpeckleStreamListItem, UxmlTraits>
    { }

    public new class UxmlTraits : BindableElement.UxmlTraits
    {
      UxmlBoolAttributeDescription _showDescription = new UxmlBoolAttributeDescription {name = "Show Description", defaultValue = false};
      UxmlBoolAttributeDescription _showOpenInNew = new UxmlBoolAttributeDescription {name = "Show Open In New", defaultValue = false};
      UxmlBoolAttributeDescription _showOperations = new UxmlBoolAttributeDescription {name = "Show Operations", defaultValue = false};


      public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
      {
        get { yield break; }
      }

      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);

        var el = ve as SpeckleStreamListItem;

        el.showDescription = _showDescription.GetValueFromBag(bag, cc);
        el.showUrlButton = _showOpenInNew.GetValueFromBag(bag, cc);
        el.showOperations = _showOperations.GetValueFromBag(bag, cc);

      }
    }

    internal static class Prop
    {
      internal const string STREAM_NAME = "name";
      internal const string STREAM_ID = "id";
    }



    SpeckleStream _value;

    Label _streamName;
    Label _streamId;
    Label _streamDescription;
    VisualElement _streamInfoContainer;
    VisualElement _controlsContainer;


    public SpeckleStreamListItem()
    {
      AddToClassList(SpeckleUss.Classes.Models.STREAM_LIST_ITEM);
      AddToClassList(SpeckleUss.Classes.Containers.ROWS);

    #if UNITY_EDITOR
      this.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(GUIHelper.Folders.CONNECTOR_USS + "stream-list-item-card.uss"));
    #endif

      var infoContainer = SpeckleUss.Prefabs.containerRow;

      _streamName = new Label(Prop.STREAM_NAME) {name = Prop.STREAM_NAME, bindingPath = Prop.STREAM_NAME};
      _streamName.AddToClassList(SpeckleUss.Classes.Elements.Text.TITLE);

      infoContainer.Add(_streamName);

      _streamId = new Label(Prop.STREAM_ID) {name = Prop.STREAM_ID, bindingPath = Prop.STREAM_ID};
      _streamId.AddToClassList(SpeckleUss.Classes.Elements.Text.SUBTITLE);
      infoContainer.Add(_streamId);

      _streamInfoContainer = SpeckleUss.Prefabs.containerColumn;
      _streamInfoContainer.AddToClassList(SpeckleUss.Classes.Containers.STREAM_INFO);
      _streamInfoContainer.Add(infoContainer);

      Add(_streamInfoContainer);

      _controlsContainer = SpeckleUss.Prefabs.containerRow;
      _controlsContainer.AddToClassList(SpeckleUss.Classes.Containers.CONTROLS);

      Add(_controlsContainer);


    }

    public Button openInNewButton { get; private set; }

    public Button receiveButton { get; private set; }

    public Button sendButton { get; private set; }


    public bool showDescription
    {
      set
      {
        if(value)
        {
          if(_streamDescription != null) return;

          _streamDescription = new Label("Stream Description") {bindingPath = "description"};
          _streamDescription.AddToClassList(SpeckleUss.Classes.Elements.Text.BODY);
          _streamInfoContainer.Add(_streamDescription);
          return;

        }

        if(_streamDescription != null)
        {
          _streamInfoContainer.Remove(_streamDescription);
          _streamDescription = null;
        }
      }
    }

    public bool showOperations
    {
      set
      {
        if(value)
        {
          if(sendButton == null)
          {
            sendButton = SpeckleUss.Prefabs.buttonWithIcon;

            //TODO: edit this so only one type is used 
            sendButton.name = SpeckleUss.Names.SEND;
            sendButton.AddToClassList(SpeckleUss.Classes.Control.SEND);

            sendButton.clickable.clicked += SendAction;
            _controlsContainer.Add(sendButton);
          }
          if(receiveButton == null)
          {
            receiveButton = SpeckleUss.Prefabs.buttonWithIcon;
            //TODO: edit this so only one type is used 
            receiveButton.AddToClassList(SpeckleUss.Classes.Control.RECEIVE);
            receiveButton.name = SpeckleUss.Names.RECEIVE;

            receiveButton.clickable.clicked += ReceiveAction;
            _controlsContainer.Add(receiveButton);
          }
          return;
        }

        if(sendButton != null)
        {
          _controlsContainer.Remove(sendButton);
          sendButton = null;
        }

        if(receiveButton != null)
        {
          _controlsContainer.Remove(receiveButton);
          receiveButton = null;
        }
      }
    }

    public bool showUrlButton
    {
      set
      {
        if(value)
        {
          if(openInNewButton != null) return;

          openInNewButton = SpeckleUss.Prefabs.buttonWithIcon;
          openInNewButton.AddToClassList(SpeckleUss.Classes.Control.OPEN_NEW);
          openInNewButton.name = SpeckleUss.Names.OPEN_IN_WEB_BUTTON;
          openInNewButton.clickable.clicked += OpenInWebAction;
          _controlsContainer.Insert(0, openInNewButton);

          return;
        }

        if(openInNewButton != null)
        {
          _controlsContainer.Remove(openInNewButton);
          openInNewButton = null;
        }
      }
    }

    public SpeckleStream value
    {
      get => _value;
      set
      {
        if(value.Equals(this.value))
          return;

        var previous = this.value;
        SetValueWithoutNotify(value);

        using var evt = ChangeEvent<SpeckleStream>.GetPooled(previous, value);
        evt.target = this;
        SendEvent(evt);
      }
    }

    public void SetValueWithoutNotify(SpeckleStream newValue)
    {
      if(newValue == null)
      {
        Debug.LogWarning($"Invalid Stream to use for {name}");
        return;
      }

      _value = newValue;

      _streamName.text = _value.Name;
      _streamId.text = _value.Id;
    }

    void OpenInWebAction()
    {
      SpeckleUnity.OpenStreamInBrowser(value);
    }

    void SendAction()
    {
      Debug.Log("Send Action");
    }

    void ReceiveAction()
    {
      Debug.Log("Receive Action");
    }

    public bool HasControl(Button b)
    {
      if(b == null)
      {
        SpeckleUnity.Console.Warn($"Invalid button value passed for {nameof(HasControl)} action");
        return false;
      }

      var hashToFind = b.GetHashCode();
      foreach(var e in _controlsContainer.Children())
      {
        if(e is Button child && child.GetHashCode() == hashToFind)
          return true;
      }

      return false;
    }

    public void AddControl(Button b)
    {
      if(b == null)
      {
        SpeckleUnity.Console.Warn($"Invalid button value passed for {nameof(AddControl)} action");
        return;
      }
      _controlsContainer.Insert(0, b);
    }

    public void RemoveControl(Button b)
    {
      if(b == null)
      {
        SpeckleUnity.Console.Warn($"Invalid button value passed for {nameof(RemoveControl)} action");
        return;
      }

      _controlsContainer.Remove(b);

    }
  }

}
