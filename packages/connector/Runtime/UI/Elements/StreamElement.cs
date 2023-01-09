using Speckle.ConnectorUnity.Ops;
using Speckle.ConnectorUnity.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{

  public class StreamElement : BindableElement, INotifyValueChanged<SpeckleStream>
  {
    public new class UxmlFactory : UxmlFactory<StreamElement, UxmlTraits>
    { }

    public new class UxmlTraits : BindableElement.UxmlTraits
    {
      UxmlBoolAttributeDescription _showDescription = new UxmlBoolAttributeDescription {name = "Show Description", defaultValue = false};
      UxmlBoolAttributeDescription _showOpenInNew = new UxmlBoolAttributeDescription {name = "Show Open In New", defaultValue = false};
      UxmlBoolAttributeDescription _showOperations = new UxmlBoolAttributeDescription {name = "Show Operations", defaultValue = false};
      UxmlBoolAttributeDescription _showPreview = new UxmlBoolAttributeDescription {name = "Show Preview", defaultValue = false};


      public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
      {
        get { yield break; }
      }

      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);

        var el = ve as StreamElement;

        el.showDescription = _showDescription.GetValueFromBag(bag, cc);
        el.showUrlButton = _showOpenInNew.GetValueFromBag(bag, cc);
        el.showOperations = _showOperations.GetValueFromBag(bag, cc);
        el.showPreview = _showPreview.GetValueFromBag(bag, cc);
      }
    }

    protected static class Prop
    {
      public const string STREAM_NAME = "name";
      public const string STREAM_ID = "id";
    }

    SpeckleStream _value;

    protected Label streamName;
    protected Label streamId;
    protected Label streamDescription;
    protected VisualElement streamInfoContainer;
    protected VisualElement controlsContainer;
    protected VisualElement previewContainer;
    protected TexturePreviewElement preview;

    /// <summary>
    /// Optional class name to attached to the element 
    /// </summary>
    protected virtual string elementClass => SpeckleUss.Classes.Models.STREAM;

    /// <summary>
    /// Name of <seealso cref="StyleSheet"/> without the extension type
    /// </summary>
    protected virtual string styleSheetName => "stream-card";

    public StreamElement() => InitElement();

    public Button openInNewButton { get; private set; }

    public Button receiveButton { get; private set; }

    public Button sendButton { get; private set; }


    public bool showDescription
    {
      set
      {
        if(value)
        {
          if(streamDescription != null) return;

          streamDescription = new Label("Stream Description") {bindingPath = "description"};
          streamDescription.AddToClassList(SpeckleUss.Classes.Elements.Text.BODY);
          streamInfoContainer.Add(streamDescription);
          return;

        }

        if(streamDescription != null)
        {
          streamInfoContainer.Remove(streamDescription);
          streamDescription = null;
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
            controlsContainer.Add(sendButton);
          }
          if(receiveButton == null)
          {
            receiveButton = SpeckleUss.Prefabs.buttonWithIcon;
            //TODO: edit this so only one type is used 
            receiveButton.AddToClassList(SpeckleUss.Classes.Control.RECEIVE);
            receiveButton.name = SpeckleUss.Names.RECEIVE;

            receiveButton.clickable.clicked += ReceiveAction;
            controlsContainer.Add(receiveButton);
          }
          return;
        }

        if(sendButton != null)
        {
          controlsContainer.Remove(sendButton);
          sendButton = null;
        }

        if(receiveButton != null)
        {
          controlsContainer.Remove(receiveButton);
          receiveButton = null;
        }
      }
    }

    public bool showPreview
    {
      set
      {
        if(value)
        {
          if(preview != null) return;

          preview = SpeckleUss.Prefabs.streamPreview;
          previewContainer.Add(preview);

          return;
        }

        if(preview != null)
        {
          previewContainer.Remove(preview);
          preview = null;
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
          controlsContainer.Insert(0, openInNewButton);

          return;
        }

        if(openInNewButton != null)
        {
          controlsContainer.Remove(openInNewButton);
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

      streamName.text = _value.Name;
      streamId.text = _value.Id;
    }

    void InitElement()
    {
      if(elementClass.Valid())
        AddToClassList(elementClass);

    #if UNITY_EDITOR
      if(styleSheetName.Valid())
        this.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(GUIHelper.Folders.CONNECTOR_USS + styleSheetName + ".uss"));
    #endif


      BuildElement();
    }

    /// <summary>
    /// Function to customize the element.
    /// </summary>
    protected virtual void BuildElement()
    {
      var infoContainer = SpeckleUss.Prefabs.containerRow;
      infoContainer.name = SpeckleUss.Classes.CONTAINER + "__info";

      streamName = new Label(Prop.STREAM_NAME) {name = Prop.STREAM_NAME, bindingPath = Prop.STREAM_NAME};
      streamName.AddToClassList(SpeckleUss.Classes.Elements.Text.TITLE);

      infoContainer.Add(streamName);

      streamId = new Label(Prop.STREAM_ID) {name = Prop.STREAM_ID, bindingPath = Prop.STREAM_ID};
      streamId.AddToClassList(SpeckleUss.Classes.Elements.Text.SUBTITLE);
      infoContainer.Add(streamId);

      streamInfoContainer = SpeckleUss.Prefabs.containerColumn;
      streamInfoContainer.name = SpeckleUss.Classes.Containers.STREAM_INFO;
      streamInfoContainer.AddToClassList(SpeckleUss.Classes.Containers.STREAM_INFO);
      streamInfoContainer.Add(infoContainer);

      Add(streamInfoContainer);

      controlsContainer = SpeckleUss.Prefabs.containerRow;
      controlsContainer.name = SpeckleUss.Classes.Containers.CONTROLS;
      controlsContainer.AddToClassList(SpeckleUss.Classes.Containers.CONTROLS);

      Add(controlsContainer);

      previewContainer = SpeckleUss.Prefabs.containerRow;
      previewContainer.name = SpeckleUss.Classes.CONTAINER + "__preview";

      Add(previewContainer);
    }

    protected virtual void OpenInWebAction()
    {
      SpeckleUnity.OpenStreamInBrowser(value);
    }

    protected virtual void SendAction()
    {
      Debug.Log("Send Action");
    }

    protected virtual void ReceiveAction()
    {
      Debug.Log("Receive Action");
    }



  }

}
