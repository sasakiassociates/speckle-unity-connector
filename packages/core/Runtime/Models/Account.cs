using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using System;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{

  [Serializable]
  public sealed class SpeckleAccount : GenericAdapter<Account>
  {
    [SerializeField] string serverUrl;
    [SerializeField] string serverName;
    [SerializeField] string serverCompany;
    [SerializeField] string userId;
    [SerializeField] string userName;
    [SerializeField] string userEmail;
    [SerializeField] string userAvatar;
    [SerializeField] string userCompany;
    [SerializeField] bool isDefault;

    public Texture2D avatar;

    string id;
    string token;
    string refreshToken;

    public string Id => id;

    public string Token => token;

    public string RefreshToken => refreshToken;

    public bool IsDefault => isDefault;


    public SpeckleAccount(Account value) : base(value)
    {
      if(value == null) return;

      id = value.id;
      token = value.token;
      refreshToken = value.refreshToken;

      isDefault = value.isDefault;
      serverUrl = value.serverInfo.url;
      serverName = value.serverInfo.name;
      serverCompany = value.serverInfo.company;

      userName = value.userInfo.name;
      userEmail = value.userInfo.email;
      userId = value.userInfo.id;
      userAvatar = value.userInfo.avatar;
      userCompany = value.userInfo.company;
      // GetTexture().Forget();
    }

    async UniTaskVoid GetTexture()
    {
      // NOTE: copied from desktop UI 
      //if the user manually uploaded their avatar it'll be a base64 string of the image
      //otherwise if linked the account eg via google, it'll be a link
      if(userAvatar != null && userAvatar.StartsWith("data:"))
      {
        try
        {
          avatar = new Texture2D(28, 28);
          // avatar.LoadRawTextureData(Convert.FromBase64String(userAvatar.Split(',')[1]));
          var colors = new Color32[28 * 28];
          for(int i = 0; i < colors.Length; i++)
          {
            colors[i] = Color.white;
          }
          avatar.SetPixels32(colors);
          avatar.Apply();

          Debug.Log($"Texture valid? {avatar != null && avatar.isReadable}");
          return;
        }
        catch
        {
          userAvatar = null;
        }
      }

      if(userAvatar == null && Id != null)
      {
        avatar = await Utils.GetTexture($"https://robohash.org/{Id}.png?size=28x28");
        avatar.Apply();
      }
    }


    public override string ToString() => "Account (" + this.userInfo.email + " | " + this.serverInfo.url + ")";

    protected override Account Get()
    {
      // NOTE: don't load the account directly from the serialized data. 
      // NOTE: use the manager to load in the account properly 

      return userInfo.GetAccountByUserInfo();

      // TODO: handle other ways of looking for accounts
    }

    public UserInfo userInfo =>
      new UserInfo()
      {
        email = userEmail,
        avatar = userAvatar,
        id = userId,
        name = userName,
        company = userCompany
      };

    public ServerInfo serverInfo =>
      new ServerInfo()
      {
        company = serverCompany,
        name = serverName,
        url = serverUrl
      };

  }

}
