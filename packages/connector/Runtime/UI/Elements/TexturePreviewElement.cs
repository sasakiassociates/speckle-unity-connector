using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{
	 public class TexturePreviewElement : BindableElement, INotifyValueChanged<Texture>
    {
        public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<TexturePreviewElement, UxmlTraits> { }

        public const string ussClassName = "texture-preview-element";

        Image m_Preview;
        Texture m_Value;

        public TexturePreviewElement()
        {
            AddToClassList(ussClassName);
            m_Preview = new Image();
            Add(m_Preview);
        }
        
        public void SetValueWithoutNotify(Texture newValue)
        {
            Debug.Log("Seting value without notifying");

            if (newValue != null)
            {
                m_Value = newValue;
                m_Preview.image = m_Value;
            }
            else throw new ArgumentException($"Expected object of type {typeof(Texture2D)}");
        }

        public Texture value
        {
            get => m_Value;
            set
            {
                Debug.Log("Setting Texture");
                
                if (value == null)
                    return;
                
                Debug.Log("Setting new texture");

                var previous = this.value;
                SetValueWithoutNotify(value);

                using (var evt = ChangeEvent<Texture>.GetPooled(previous, value))
                {
                    evt.target = this;
                    SendEvent(evt);
                }
            }
        }
    }
}