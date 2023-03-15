using Speckle.ConnectorUnity.Ops;
using Speckle.ConnectorUnity.UI;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{

  public class AccountElement : BindableElement, INotifyValueChanged<SpeckleAccount>
  {
    SpeckleAccount _value;

    public new class UxmlTraits : BindableElement.UxmlTraits
    { }

    public new class UxmlFactory : UxmlFactory<AccountElement, UxmlTraits>
    { }

    public AccountElement()
    {
      AddToClassList(SpeckleUss.Classes.Models.ACCOUNT);

    #if UNITY_EDITOR
      this.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(GUIHelper.Folders.CONNECTOR_USS + "account-card.uss"));
    #endif

      var texture = new TexturePreviewElement();
      texture.AddToClassList(SpeckleUss.Classes.Elements.Texture.AVATAR);
      Add(texture);

      var group = SpeckleUss.Prefabs.containerColumn;
      group.Add(CreateGroup(SERVER_NAME, SERVER_URL));
      group.Add(CreateGroup(USER_NAME, USER_EMAIL));

      Add(group);
    }

    const string USER_NAME = "userName";
    const string USER_EMAIL = "userEmail";
    const string SERVER_URL = "serverUrl";
    const string SERVER_NAME = "serverName";


    public SpeckleAccount value
    {
      get => _value;
      set
      {
        if(value.Equals(this.value))
          return;

        var previous = this.value;
        SetValueWithoutNotify(value);

        using var evt = ChangeEvent<SpeckleAccount>.GetPooled(previous, value);
        evt.target = this;
        SendEvent(evt);
      }
    }

    public UserInfo userInfo
    {
      set
      {
        SafeSetValue(USER_NAME, value.name);
        SafeSetValue(USER_EMAIL, value.email);
      }
    }

    public ServerInfo serverInfo
    {
      set
      {
        SafeSetValue(SERVER_NAME, value.name);
        SafeSetValue(SERVER_URL, value.url);
      }
    }

    public void SetValueWithoutNotify(SpeckleAccount newValue)
    {
      if(newValue == null)
      {
        Debug.LogWarning($"Invalid Stream to use for {name}");
        return;
      }

      _value = newValue;

      serverInfo = _value.ServerInfo;
      userInfo = _value.UserInfo;
    }

    void SafeSetValue(string label, string input)
    {
      if(this.Q<Label>(label) == null)
      {
        Debug.Log($"{name} does not have a label named {label}");
        return;
      }

      this.Q<Label>(label).text = input;
    }


    VisualElement CreateGroup(string mainProp, string subProp)
    {
      var container = SpeckleUss.Prefabs.containerRow;

      var mainItem = new Label(mainProp)
      {
        name = mainProp,
        bindingPath = mainProp
      };

      mainItem.AddToClassList(SpeckleUss.Classes.Elements.Text.TITLE);

      container.Add(mainItem);

      var sub = SpeckleUss.Prefabs.containerRow;
      sub.AddToClassList(SpeckleUss.Classes.Elements.Text.SUBTITLE);

      var subItem = new Label(subProp)
      {
        name = subProp,
        bindingPath = subProp
      };

      sub.Add(new Label("("));
      sub.Add(subItem);
      sub.Add(new Label(")"));

      container.Add(sub);

      return container;
    }


  }

}
