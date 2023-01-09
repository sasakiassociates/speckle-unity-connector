using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
using Speckle.ConnectorUnity.Models;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

  /// <summary>
  ///   A Speckle Sender, it's a wrapper around a basic Speckle Client
  ///   that handles conversions for you
  /// </summary>
  [AddComponentMenu(SpeckleUnity.NAMESPACE + "/Sender")]
  [ExecuteAlways]
  public class Sender : OpsBehaviour<SendWorkArgs>
  {
    [SerializeField] string _message;

    Base _data;

    /// <summary>
    /// Ignores any speckle node values and sends the loaded <paramref name="@base"/>
    /// </summary>
    /// <param name="base"></param>
    /// <param name="message">Commit message to add</param>
    /// <param name="tokenSource">Cancellation token. Will default to attached source token</param>
    /// <returns></returns>
    public async UniTask DoWork(Base @base, string message = null, CancellationTokenSource tokenSource = null)
    {
      _data = @base;
      await DoWork(message, tokenSource);
    }

    /// <summary>
    /// Converts and sends the speckle node to the active stream of this client
    /// </summary>
    /// <param name="obj">Top level object to send</param>
    /// <param name="message">Commit message</param>
    /// <param name="tokenSource">Cancellation token</param>
    /// <returns></returns>
    public async UniTask DoWork(SpeckleObjectBehaviour obj, string message = null, CancellationTokenSource tokenSource = null)
    {
      if(obj != null)
        root = obj;

      await DoWork(message, tokenSource);
    }

    /// <summary>
    /// Converts all objects attached to the speckle node and sends it to the active stream in this client object
    /// </summary>
    /// <param name="message">Commit message to add</param>
    /// <param name="tokenSource">Cancellation token. Will default to attached source token</param>
    /// <returns></returns>
    public async UniTask DoWork(string message, CancellationTokenSource tokenSource = null)
    {
      _message = message.Valid() ? message : string.Empty;
      await base.DoWork();
    }

    protected override void SetSubscriptions()
    { }

    protected override async UniTask Execute()
    {
      try
      {
        SpeckleUnity.Console.Log("Send started");

        if(root == null && _data == null)
        {
          Args.message = $"No objects were found in {nameof(SpeckleNode)} to send. Stopping call";
          SpeckleUnity.Console.Warn(Args.message);
          return;
        }

        _data ??= root.SceneToData(converter, token);

        if(_data == null)
        {
          Args.message = "There is no data in this commit to send. Stopping call";
          SpeckleUnity.Console.Warn(Args.message);
          return;
        }

        var objectId = await SpeckleOps.Send(client, _data, stream.Id, HandleProgress, HandleError);
        var count = _data.GetTotalChildrenCount();

        var commitId = await client.CommitCreate(
          new CommitCreateInput
          {
            objectId = objectId,
            streamId = stream.Id,
            branchName = Branch.name,
            message = _message.Valid() ? _message : $"Objects from Unity {count}",
            sourceApplication = SpeckleUnity.APP,
            totalChildrenCount = (int)count
          });

        Args.success = true;
        Args.commitId = commitId;
        Args.message = $"Commit sent to {Branch}! ({objectId})";
        Args.url = SpeckleUnity.GetUrl(false, client.Account.serverInfo.url, stream.Id, StreamWrapperType.Commit, commitId);

        onDataSent?.Invoke(objectId);
      }
      catch(SpeckleException e)
      {
        SpeckleUnity.Console.Warn(e.Message);
      }
      finally
      {
        _data = null;

        await UniTask.Yield();

        HandleRefresh();
      }
    }

    public event UnityAction<string> onDataSent;
  }

}
