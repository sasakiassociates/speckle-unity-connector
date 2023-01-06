using Speckle.ConnectorUnity.UI;
using System;
using UnityEngine;
using UnityEngine.UIElements;


namespace Speckle.ConnectorUnity.Elements
{

  public class TexturePreviewElement : BindableElement, INotifyValueChanged<Texture>
  {
    public new class UxmlTraits : BindableElement.UxmlTraits
    { }

    public new class UxmlFactory : UxmlFactory<TexturePreviewElement, UxmlTraits>
    { }

    Image _preview;
    Texture _value;

    public TexturePreviewElement()
    {
      AddToClassList(SpeckleUss.Classes.Elements.Texture.PREVIEW);
      _preview = new Image();
      Add(_preview);
    }

    public void SetValueWithoutNotify(Texture newValue)
    {
      if(newValue != null)
      {
        _value = newValue;
        _preview.image = _value;
      }
      else throw new ArgumentException($"Expected object of type {typeof(Texture2D)}");
    }

    public Texture value
    {
      get => _value;
      set
      {
        if(value == null) return;

        var previous = this.value;
        SetValueWithoutNotify(value);

        using var evt = ChangeEvent<Texture>.GetPooled(previous, value);
        evt.target = this;
        SendEvent(evt);
      }
    }
  }

}
