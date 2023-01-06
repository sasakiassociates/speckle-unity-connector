using Speckle.ConnectorUnity.Ops;
using Speckle.ConnectorUnity.UI;
using System;
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
      UxmlBoolAttributeDescription m_showDescription = new UxmlBoolAttributeDescription {name = "Show Description", defaultValue = false};
      UxmlBoolAttributeDescription m_showOpenUrlButton = new UxmlBoolAttributeDescription {name = "Show Url Button", defaultValue = false};


      public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
      {
        get { yield break; }
      }

      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);

        var el = ve as SpeckleStreamListItem;

        el.showDescription = m_showDescription.GetValueFromBag(bag, cc);
        el.showUrlButton = m_showOpenUrlButton.GetValueFromBag(bag, cc);

      }
    }

    internal static class prop
    {
      internal const string Stream_Name = "name";
      internal const string Stream_Id = "id";
    }



    Label _streamName;
    Label _streamId;
    Label _streamDescription;
    SpeckleStream _value;
    VisualElement _infoContainer;
    VisualElement _controlsContainer;
    Button _showUrlButton;


    public SpeckleStreamListItem()
    {
      AddToClassList(SpeckleUss.Classes.Models.STREAM);

      _infoContainer = new VisualElement
      {
        name = SpeckleUss.Names.INFO,
        style = {flexGrow = 1}
      };
      _infoContainer.AddToClassList(SpeckleUss.Classes.Containers.ROWS);

      _streamName = new Label(prop.Stream_Name) {name = prop.Stream_Name, bindingPath = prop.Stream_Name};
      _streamName.AddToClassList(SpeckleUss.Classes.Elements.Text.TITLE);
      
      _infoContainer.Add(_streamName);

      _streamId = new Label(prop.Stream_Id) {name = prop.Stream_Id, bindingPath = prop.Stream_Id};
      _streamId.AddToClassList(SpeckleUss.Classes.Elements.Text.SUBTITLE);

      var sub = SpeckleUss.Prefabs.subTitle;
      sub.Add(new Label("("));
      sub.Add(_streamId);
      sub.Add(new Label(")"));

      _infoContainer.Add(sub);

      _controlsContainer = new VisualElement() {name = SpeckleUss.Names.CONTROLS};
      _controlsContainer.AddToClassList(SpeckleUss.Classes.CONTAINER);

      var group = SpeckleUss.Prefabs.containerRow;
      group.Add(_infoContainer);
      group.Add(_controlsContainer);

      Add(group);
    }


    public bool showDescription
    {
      set
      {
        if(value)
        {
          if(_streamDescription != null) return;

          _streamDescription = new Label("Stream Description") {bindingPath = "description"};
          _streamDescription.AddToClassList(SpeckleUss.Classes.Elements.Text.BODY);
          _infoContainer.Add(_streamDescription);
          return;

        }

        if(_streamDescription != null)
        {
          _infoContainer.Remove(_streamDescription);
          _streamDescription = null;
        }
      }
    }

    public bool showUrlButton
    {
      set
      {
        if(value)
        {
          if(_showUrlButton != null) return;

          _showUrlButton = SpeckleUss.Prefabs.buttonWithIcon;
          _showUrlButton.name = SpeckleUss.Names.OPEN_IN_WEB_BUTTON;
          _showUrlButton.clickable.clicked += ButtonClick;
          _controlsContainer.Add(_showUrlButton);

          return;
        }

        if(_showUrlButton != null)
        {
          _controlsContainer.Remove(_showUrlButton);
          _showUrlButton = null;
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

    void ButtonClick()
    {
      SpeckleUnity.OpenStreamInBrowser(value);
    }


  }

}
