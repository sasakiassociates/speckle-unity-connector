using Speckle.ConnectorUnity.Ops;
using Speckle.ConnectorUnity.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{

  public class StreamElement : BindableElement, INotifyValueChanged<SpeckleStream>
  {

    protected static class Prop
    {
      public const string STREAM_NAME = "name";
      public const string STREAM_ID = "id";
    }

    SpeckleStream _value;

    protected Label streamName;
    protected Label streamId;
    protected Label streamDescription;
    protected TexturePreviewElement preview;

    protected VisualElement headingContainer;
    protected VisualElement titlesContainer;
    protected VisualElement nameContainer;
    protected VisualElement controlsContainer;
    protected VisualElement viewportContainer;
    protected VisualElement footingContainer;


    public StreamElement()
    {
      InitElement();
    }

    /// <summary>
    /// Optional class name to attached to the element 
    /// </summary>
    protected virtual string elementClass => SpeckleUss.Classes.Models.STREAM;

    /// <summary>
    /// Name of <seealso cref="StyleSheet"/> without the extension type
    /// </summary>
    protected virtual string styleSheetName => "stream-card";

    public Button openInNewButton { get; protected set; }

    public Button receiveButton { get; protected set; }

    public Button sendButton { get; protected set; }

    protected bool AddControlsToHeading { get; set; } = true;

    public bool displayDescription
    {
      set
      {
        if(value)
        {
          if(streamDescription != null) return;

          streamDescription = new Label("Stream Description") {bindingPath = "description"};
          streamDescription.AddToClassList(SpeckleUss.Classes.Elements.Text.BODY);
          nameContainer.Add(streamDescription);
          return;

        }

        if(streamDescription != null)
        {
          nameContainer.Remove(streamDescription);
          streamDescription = null;
        }
      }
    }

    public bool displayOperations
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

    public bool displayPreview
    {
      set
      {
        if(value)
        {
          if(preview != null) return;

          preview = SpeckleUss.Prefabs.streamPreview;
          viewportContainer.Add(preview);

          return;
        }

        if(preview != null)
        {
          viewportContainer.Remove(preview);
          preview = null;
        }
      }
    }

    public bool displayOpenInNew
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


    public virtual void SetValueWithoutNotify(SpeckleStream newValue)
    {
      if(newValue == null)
      {
        Debug.LogWarning($"Invalid Stream to use for {name}");
        return;
      }

      _value = newValue;

      streamName.text = _value.Name;
      streamId.text = _value.Id;

      if(streamDescription != null) streamDescription.text = _value.Description;
    }

    public void SetPreviewTexture(Texture texture)
    {
      if(preview == null)
      {
        SpeckleUnity.Console.Log($"Setting {this} {nameof(displayPreview)} to true");

        displayPreview = true;
        if(preview == null)
        {
          SpeckleUnity.Console.Warn($"Preview object was not created properly when calling {nameof(SetPreviewTexture)}");
          return;
        }
      }

      preview.value = texture;

    }


    void InitElement()
    {
      if(elementClass.Valid()) AddToClassList(elementClass);

    #if UNITY_EDITOR
      if(styleSheetName.Valid())
        this.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(GUIHelper.Folders.CONNECTOR_USS + styleSheetName + ".uss"));
    #endif

      ConstructHeading();
      ConstructTitles();
      ConstructControls();
      ConstructViewport();
      ConstructFooting();
    }

    protected virtual void ConstructHeading()
    {
      headingContainer = SpeckleUss.Prefabs.containerRow;
      headingContainer.name = SpeckleUss.Classes.CONTAINER + "__heading";

      this.Add(headingContainer);
    }

    protected virtual void ConstructTitles()
    {
      titlesContainer = SpeckleUss.Prefabs.containerRow;
      titlesContainer.name = SpeckleUss.Classes.CONTAINER + "__titles";

      var infoContainer = SpeckleUss.Prefabs.containerRow;
      infoContainer.name = SpeckleUss.Classes.CONTAINER + "__info";

      streamName = new Label(Prop.STREAM_NAME) {name = Prop.STREAM_NAME, bindingPath = Prop.STREAM_NAME};
      streamName.AddToClassList(SpeckleUss.Classes.Elements.Text.TITLE);

      infoContainer.Add(streamName);

      streamId = new Label(Prop.STREAM_ID) {name = Prop.STREAM_ID, bindingPath = Prop.STREAM_ID};
      streamId.AddToClassList(SpeckleUss.Classes.Elements.Text.SUBTITLE);
      infoContainer.Add(streamId);

      nameContainer = SpeckleUss.Prefabs.containerColumn;
      nameContainer.name = SpeckleUss.Classes.Containers.STREAM_INFO;
      nameContainer.AddToClassList(SpeckleUss.Classes.Containers.STREAM_INFO);
      nameContainer.Add(infoContainer);


      titlesContainer.Add(nameContainer);
      titlesContainer.Add(controlsContainer);

      Add(titlesContainer);
    }

    protected virtual void ConstructControls()
    {
      controlsContainer = SpeckleUss.Prefabs.containerRow;
      controlsContainer.name = SpeckleUss.Classes.Containers.CONTROLS;
      controlsContainer.AddToClassList(SpeckleUss.Classes.Containers.CONTROLS);

      if(AddControlsToHeading) titlesContainer?.Add(controlsContainer);
      else Add(controlsContainer);
    }

    protected virtual void ConstructViewport()
    {
      viewportContainer = SpeckleUss.Prefabs.containerRow;
      viewportContainer.name = SpeckleUss.Classes.CONTAINER + "__viewport";

      Add(viewportContainer);
    }

    protected virtual void ConstructFooting()
    {
      footingContainer = new VisualElement
      {
        name = SpeckleUss.Classes.CONTAINER + "__footing"
      };

      Add(footingContainer);

    }


    protected virtual void OpenInWebAction()
    {
      Utils.OpenStreamInBrowser(value);
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
