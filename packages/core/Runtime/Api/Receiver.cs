using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
using Speckle.ConnectorUnity.Models;
using Speckle.Core.Api;
using Speckle.Core.Api.SubscriptionModels;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

  /// <summary>
  ///   A Speckle Receiver, it's a wrapper around a basic Speckle Client
  ///   that handles conversions and subscriptions for you
  /// </summary>
  [ExecuteAlways]
  [AddComponentMenu(SpeckleUnity.NAMESPACE + "/Receiver")]
  public class Receiver : OpsBehaviour<ReceiveWorkArgs>
  {
    [SerializeField] ReceiveMode _mode;
    [SerializeField] bool _autoReceive;
    [SerializeField] bool _sendReceive;
    [SerializeField] bool _deleteOld = true;
    [SerializeField] Texture _preview;
    [SerializeField] bool _showPreview = true;
    [SerializeField] bool _renderPreview = true;

    public Texture preview => _preview;

    public bool showPreview
    {
      get => _showPreview;
      set => _showPreview = value;
    }

    protected override async UniTask Execute()
    {
      try
      {
        SpeckleUnity.Console.Log("Receive Started");

        root = new GameObject().AddComponent<SpeckleObjectBehaviour>();

        // NOTE: this might now need to happen
        switch (stream.type)
        {
          case StreamWrapperType.Commit:
            var c = await client.CommitGet(stream.Id, stream.Commit.id);

            // TODO: check if this getting the commit updates the instance
            if (_sendReceive)
              client.CommitReceived(new CommitReceivedInput()
              {
                streamId = stream.Id,
                commitId = c.id,
                message = "Received Commit from Unity",
                sourceApplication = SpeckleUnity.APP
              }).Forget();

            root.source = await client.ObjectGet(stream.Id, c.referencedObject);
            break;
          case StreamWrapperType.Object:
            root.source = await client.ObjectGet(stream.Id, stream.Object.id);
            break;
          case StreamWrapperType.Branch:
          case StreamWrapperType.Stream:
            SpeckleUnity.Console.Warn("A commit or object needs to be set in this stream in order to receive something");
            break;
          case StreamWrapperType.Undefined:
          default:
            SpeckleUnity.Console.Error("Stream is not properly ready to receive");
            break;
        }

        if (!root.IsValid())
        {
          Args.message = "The reference object pulled down from this stream is not valid";
          SpeckleUnity.Console.Warn($"{name}-" + Args.message);
          return;
        }

        Base @base = await SpeckleOps.Receive(client, stream.Id, root.id, HandleProgress, HandleError, HandleChildCount);

        if (@base == null)
        {
          Args.message = "Data from Commit was not valid";
          SpeckleUnity.Console.Warn($"{name}-" + Args.message);
          return;
        }

        Args.referenceObj = root.id;

        // TODO: handle the process for update objects and not just force deleting
        if (_deleteOld)
          root.Purge();

        // TODO: Handle separating the operation call from the conversion
        await root.ConvertToScene(@base, converter, Token);

        Args.success = true;
        Args.message = $"Completed {nameof(Execute)}";
        OnNodeComplete?.Invoke(root);
      }
      catch (SpeckleException e)
      {
        SpeckleUnity.Console.Warn(e.Message);
      }
      finally
      {
        Progress = 0f;
        IsWorking = false;

        await UniTask.Yield();

        HandleRefresh();
      }
    }

    protected override void SetSubscriptions()
    {
      if (client == null || !client.IsValid())
        return;

      client.source.SubscribeCommitCreated(Stream.id);
      client.source.OnCommitCreated += (_, c) => OnCommitCreated?.Invoke(c);
      client.source.SubscribeCommitUpdated(Stream.id);
      client.source.OnCommitUpdated += (_, c) => OnCommitUpdated?.Invoke(c);
    }

    protected override UniTask PostLoadCommit()
    {
      UpdatePreview().Forget();
      return UniTask.CompletedTask;
    }

    async UniTask UpdatePreview()
    {
      if (!IsValid())
        await UniTask.Yield();

      _preview = await SpeckleUnity.GetTexture(stream.GetUrl(true, Account.serverInfo.url));

      OnPreviewSet?.Invoke();

      await UniTask.Yield();
    }

		#region Events

    public event UnityAction<SpeckleObjectBehaviour> OnNodeComplete;

    public event UnityAction OnPreviewSet;

		#endregion

		#region Subscriptions

    public event UnityAction<CommitInfo> OnCommitCreated;

    public event UnityAction<CommitInfo> OnCommitUpdated;

		#endregion

  }
}
