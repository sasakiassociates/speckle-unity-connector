using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{
  public class AccountElement : BindableElement
  {
    public new class UxmlTraits : BindableElement.UxmlTraits
    {
    }
    public new class UxmlFactory : UxmlFactory<AccountElement, UxmlTraits>
    {
    }

    public static string ussClassName = "account-element";
    public static string ussGroupClass = "speckle-model-object";
    public static string ussContainerClass = "container";
    public static string ussTitleClass = "title";
    public static string ussSubGroupClass = "sub";
    public static string ussTextureClass = "speckle-avatar";

    const string USER_NAME = "userName";
    const string USER_EMAIL = "userEmail";
    const string SERVER_URL = "serverUrl";
    const string SERVER_NAME = "serverName";

    public AccountElement()
    {
      AddToClassList(ussClassName);

      var texture = new TexturePreviewElement();
      texture.AddToClassList(ussTextureClass);
      Add(texture);

      var group = new VisualElement();
      group.AddToClassList(ussGroupClass);

      group.Add(CreateGroup(SERVER_NAME, SERVER_URL));
      group.Add(CreateGroup(USER_NAME, USER_EMAIL));

      Add(group);
    }

    public void SetUserInfo(UserInfo info)
    {
      SetValue(USER_NAME, info.name);
      SetValue(USER_EMAIL, info.email);
    }

    public void SetServerInfo(ServerInfo info)
    {
      SetValue(SERVER_NAME, info.name);
      SetValue(SERVER_URL, info.url);
    }

    public void SetAccount(Account obj)
    {
      if (obj == null)
      {
        Debug.Log($"Invalid {typeof(Account)} for {name} to use");
        return;
      }
      SetServerInfo(obj.serverInfo);
      SetUserInfo(obj.userInfo);
    }

    void SetValue(string label, string value)
    {
      if (this.Q<Label>(label) == null)
      {
        Debug.Log($"{name} does not have a label named {label}");
        return;
      }

      this.Q<Label>(label).text = value;
    }


    VisualElement CreateGroup(string mainProp, string subProp)
    {
      var container = new VisualElement();
      container.AddToClassList(ussContainerClass);

      var mainItem = new Label(mainProp)
      {
        name = mainProp,
        bindingPath = mainProp
      };

      mainItem.AddToClassList(ussTitleClass);

      container.Add(mainItem);

      var sub = new VisualElement();
      sub.AddToClassList(ussSubGroupClass);

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
