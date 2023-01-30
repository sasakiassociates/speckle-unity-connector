using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Credentials;
using UnityEngine;
using UnityEngine.Networking;

namespace Speckle.ConnectorUnity
{

  public static partial class Utils
  {


    public static void OpenStreamInBrowser(SpeckleStream stream)
    {
      UniTask.Create(async () =>
      {
        // copied from desktop ui
        await UniTask.Delay(100);

        Application.OpenURL(stream.GetUrl(false));
      });
    }


    public static string GetUrl(bool isPreview, string serverUrl, string streamId) => $"{serverUrl}/{(isPreview ? "preview" : "streams")}/{streamId}";

    public static string GetUrl(bool isPreview, string serverUrl, string streamId, StreamWrapperType type, string value)
    {
      string url = $"{serverUrl}/{(isPreview ? "preview" : "streams")}/{streamId}";

      switch(type)
      {
        case StreamWrapperType.Stream:
          return url;
        case StreamWrapperType.Commit:
          url += $"/commits/{value}";
          break;
        case StreamWrapperType.Branch:
          url += $"/branches/{value}";
          break;
        case StreamWrapperType.Object:
          url += $"objects/{value}";
          break;
        case StreamWrapperType.Undefined:
        default:
          SpeckleUnity.Console.Warn($"{streamId} is not a valid stream for server {serverUrl}, bailing on the preview thing");
          url = null;
          break;
      }

      return url;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async UniTask<Texture2D> GetTexture(string url)
    {
      if(!url.Valid())
        return null;

      var www = await UnityWebRequestTexture.GetTexture(url).SendWebRequest();

      if(www.result != UnityWebRequest.Result.Success)
      {
        SpeckleUnity.Console.Warn(www.error);
        return null;
      }

      return DownloadHandlerTexture.GetContent(www);
    }


  }

}
