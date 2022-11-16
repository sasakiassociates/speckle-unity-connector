using Speckle.ConnectorUnity.Ops;
using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Speckle.ConnectorUnity.Elements
{


  public class SpeckleStreamListItem : BindableElement, INotifyValueChanged<SpeckleStream>
  {
    public new class UxmlTraits : BindableElement.UxmlTraits
    {
    }
    public new class UxmlFactory : UxmlFactory<SpeckleStreamListItem, UxmlTraits>
    {
    }

    public static class uss
    {
      public const string ElementClassName = "stream-element";
      public const string GroupClassName = "speckle-object__model";
      public const string ContainerClassName = "speckle-object__container";
      public const string IconButtonClassName = "speckle-icon__button";
      public const string IconButtonName = "openInWeb";
      public const string TitleName = "title";
      public const string SubTitleName = "sub";
      public const string IconName = "icon";
    }


    internal static class prop
    {
      internal const string Stream_Name = "name";
      internal const string Stream_Id = "id";
    }

    Label m_streamName;
    Label m_streamId;


    public SpeckleStreamListItem()
    {
      AddToClassList(uss.ElementClassName);

      var group = new VisualElement();
      group.AddToClassList(uss.GroupClassName);

      var container = new VisualElement();
      container.AddToClassList(uss.ContainerClassName);

      m_streamName = new Label(prop.Stream_Name) { name = uss.TitleName, bindingPath = prop.Stream_Name };

      var sub = new VisualElement() { name = uss.SubTitleName };
      m_streamId = new Label(prop.Stream_Id) { bindingPath = prop.Stream_Id };
      sub.Add(new Label("("));
      sub.Add(m_streamId);
      sub.Add(new Label(")"));

      container.Add(m_streamName);
      container.Add(sub);
      
      group.Add(container);

      var button = new Button() { name = uss.IconButtonName };
      button.clickable.clicked += ButtonClick;
      button.Add(new VisualElement() { name = uss.IconName });

      Add(group);
      Add(button);
    }

    public void SetValueWithoutNotify(SpeckleStream newValue)
    {
      if (newValue == null)
      {
        Debug.LogWarning($"Invalid Stream to use for {name}");
        return;
      }

      m_value = newValue;

      m_streamName.text = m_value.Name;
      m_streamId.text = m_value.Id;
    }

    void ButtonClick()
    {
      SpeckleUnity.OpenStreamInBrowser(value);
    }

    SpeckleStream m_value;
    public SpeckleStream value
    {
      get => m_value;
      set
      {
        if (value.Equals(this.value))
          return;

        var previous = this.value;
        SetValueWithoutNotify(value);

        using var evt = ChangeEvent<SpeckleStream>.GetPooled(previous, value);
        evt.target = this;
        SendEvent(evt);
      }
    }

  }
}
