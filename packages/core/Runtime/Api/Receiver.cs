using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
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
    [SerializeField] ReceiveMode mode;
    [SerializeField] bool autoReceive;
    [SerializeField] bool sendReceive;
    [SerializeField] bool deleteOld = true;
    [SerializeField] Texture preview;
    [SerializeField] bool showPreview = true;
    [SerializeField] bool renderPreview = true;

    public Texture Preview => preview;

    public bool ShowPreview
    {
      get => showPreview;
      set => showPreview = value;
    }

    protected override async UniTask Execute()
    {
      try
      {
        SpeckleUnity.Console.Log("Receive Started");

        root = new GameObject().AddComponent<SpeckleObjectBehaviour>();

        // NOTE: this might now need to happen
        switch(stream.Type)
        {
          case StreamWrapperType.Commit:
            var c = await client.CommitGet(stream.Id, stream.Commit.id);

            // TODO: check if this getting the commit updates the instance
            if(sendReceive)
              client.CommitReceived(new CommitReceivedInput()
              {
                streamId = stream.Id,
                commitId = c.id,
                message = "Received Commit from Unity",
                sourceApplication = SpeckleUnity.APP
              }).Forget();

            root.Source = await client.ObjectGet(stream.Id, c.referencedObject);
            break;
          case StreamWrapperType.Object:
            root.Source = await client.ObjectGet(stream.Id, stream.Object.id);
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

        if(!root.IsValid())
        {
          Args.Message = "The reference object pulled down from this stream is not valid";
          SpeckleUnity.Console.Warn($"{name}-" + Args.Message);
          return;
        }

        Base @base = await SpeckleOps.Receive(client, stream.Id, root.id, HandleProgress, HandleError, HandleChildCount);

        if(@base == null)
        {
          Args.Message = "Data from Commit was not valid";
          SpeckleUnity.Console.Warn($"{name}-" + Args.Message);
          return;
        }

        Args.ReferenceObj = root.id;

        // TODO: handle the process for update objects and not just force deleting
        if(deleteOld)
          root.Purge();

        // TODO: Handle separating the operation call from the conversion
        await root.ConvertToScene(@base, Converter, Token);

        Args.Success = true;
        Args.Message = $"Completed {nameof(Execute)}";
        OnNodeComplete?.Invoke(root);
      }
      catch(SpeckleException e)
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
      if(client == null || !client.IsValid())
        return;

      client.Source.SubscribeCommitCreated(Stream.id);
      client.Source.OnCommitCreated += (_, c) => OnCommitCreated?.Invoke(c);
      client.Source.SubscribeCommitUpdated(Stream.id);
      client.Source.OnCommitUpdated += (_, c) => OnCommitUpdated?.Invoke(c);
    }

    protected override UniTask PostLoadCommit()
    {
      UpdatePreview().Forget();
      return UniTask.CompletedTask;
    }

    async UniTask UpdatePreview()
    {
      if(!IsValid())
        await UniTask.Yield();

      preview = await Utils.GetTexture(stream.GetUrl(true, BaseAccount.serverInfo.url));

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
