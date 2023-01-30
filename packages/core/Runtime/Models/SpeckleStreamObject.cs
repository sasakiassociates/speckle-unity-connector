using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Models
{

  /// <summary>
  /// A unity asset object for interacting with a <see cref="SpeckleStream"/>.
  /// Only use this type of object if you would like editor based capabilities with a stream.
  /// If no editor functionality is needed, try using <see cref="SpeckleStream"/> instead.
  /// </summary>
  [CreateAssetMenu(menuName = "Speckle/Speckle Stream", fileName = "SpeckleStream", order = 0)]
  public class SpeckleStreamObject : ScriptableObject, IStream
  {

    [SerializeField] SpeckleStream stream;
    [SerializeField] SpeckleAccount account;
    [SerializeField] SpeckleClient client;
    [SerializeField] Texture preview;
    [SerializeField] bool show;
    [SerializeField] string originalUrlInput;

    CancellationTokenSource sourceToken;

    public event UnityAction OnInitialize;
    public event UnityAction OnStreamSet;
    public event UnityAction<Texture> OnPreviewSet;
    public event UnityAction<Commit> OnCommitSet;
    public event UnityAction<Branch> OnBranchSet;

    public Client baseClient => client?.source;

    public SpeckleStream SourceStream => stream;

    public Stream Stream => stream?.source;

    public CancellationToken token
    {
      get
      {
        sourceToken ??= new CancellationTokenSource();
        return sourceToken.Token;
      }
    }

    public string Id => stream?.Id;

    public string Name => stream?.Name;

    public string Description => stream?.Description;

    public bool IsPublic => stream?.IsPublic ?? false;

    public string Object => stream?.Object?.id;

    public Branch Branch => stream?.Branch;

    public List<Branch> Branches => stream?.Branches;

    public Commit Commit => stream?.Commit;

    public List<Commit> Commits => stream?.Commits;

    public string OriginalUrlInput => originalUrlInput;

    public Account baseAccount => account?.source;

    public Texture Preview => preview;

    public async UniTask SetCommit(string commitId)
    {
      if(stream.CommitSet(commitId))
        await LoadCommit(commitId);
    }

    public async UniTask SetBranch(string branchName)
    {
      if(stream.BranchSet(branchName))
        await LoadBranch(branchName);
    }

    public async UniTask SetObject(string objectId)
    {
      if(stream.IsValid() && objectId.Valid())
        await LoadObject(objectId);
    }

    /// <summary>
    /// Checks if <see cref="baseClient"/> and <see cref="Stream"/> are both valid 
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
      return client != null && client.IsValid() && stream != null && stream.IsValid();
    }


    public async UniTask Initialize(string streamUrl)
    {
      try
      {
        var wrapper = new StreamWrapper(streamUrl);

        originalUrlInput = streamUrl;

        if(!wrapper.IsValid)
        {
          SpeckleUnity.Console.Warn("Stream url is not valid!" + (streamUrl.Valid() ? $"Input={streamUrl}" : "Invalid Input"));
        }
        else
        {
          var obj = await wrapper.GetAccount();
          Initialize(obj);

          if(await TryLoadStream(wrapper.StreamId))
          {
            switch(wrapper.Type)
            {
              case StreamWrapperType.Undefined:
                SpeckleUnity.Console.Warn("Stream Input type is undefined");
                break;
              case StreamWrapperType.Commit:
                await LoadCommit(wrapper.CommitId);
                break;
              case StreamWrapperType.Branch:
                await LoadBranch(wrapper.BranchName);
                break;
              case StreamWrapperType.Stream:
                await LoadBranch("main");
                break;
              case StreamWrapperType.Object:
                await LoadObject(wrapper.ObjectId);
                break;
            }
          }
        }
      }
      catch(SpeckleException e)
      {
        SpeckleUnity.Console.Warn(e.Message);
      }

      finally
      {
        OnInitialize?.Invoke();
      }
    }

    public UniTask Initialize(Account obj)
    {

      try
      {
        Cancel();

        if(obj == null)
        {
          SpeckleUnity.Console.Warn($"Invalid Account being passed into {name}");
        }
        else
        {
          account = new SpeckleAccount(obj);
          client = new SpeckleClient(obj);
          client.token = new CancellationToken();
          sourceToken = CancellationTokenSource.CreateLinkedTokenSource(client.token);
        }
      }
      catch(SpeckleException e)
      {
        SpeckleUnity.Console.Warn(e.Message);
      }
      finally
      {
        OnInitialize?.Invoke();
      }

      return UniTask.CompletedTask;

    }

    /// <summary>
    ///   Clean up to any client things
    /// </summary>
    public void Cancel()
    {
      sourceToken?.Cancel();
      sourceToken?.Dispose();
      client?.Dispose();
    }


    public async UniTask LoadStream(string streamId)
    {
      try
      {
        await TryLoadStream(streamId);
        await LoadBranch("main");
      }
      catch(Exception e)
      {
        SpeckleUnity.Console.Warn(e.Message);
      }
      finally
      {
        OnStreamSet?.Invoke();
      }
    }

    // /// <summary>
    // /// Sets new stream details to be applied to this <see cref="Core.Api.Stream"/> 
    // /// </summary>
    // /// <param name="updatedDescription"></param>
    // /// <param name="updatedPublic"></param>
    // /// <param name="immediatelyUpdate">Will push the changes automatically</param>
    // /// <param name="updatedName"></param>
    // public void Modify(bool updatedPublic, string updatedName = null, string updatedDescription = null, bool immediatelyUpdate = false)
    // {
    // 	if (stream == null)
    // 	{
    // 		SpeckleUnity.Console.Warn($"{this.name} does not have a valid stream associated with it.");
    // 		return;
    // 	}
    //
    // 	// store any changes being made
    // 	_update = new StreamUpdateInputWrapper
    // 	{
    // 		isPublic = updatedPublic,
    // 		name = updatedName.Valid() ? updatedName : this.Name,
    // 		description = updatedDescription.Valid() ? updatedDescription : this.Description
    // 	};
    //
    // 	if (!immediatelyUpdate)
    // 		return;
    //
    // 	UniTask.Create(async () =>
    // 	{
    // 		if (await client.StreamUpdate(_update.Get(this.Id)))
    // 		{
    // 			SpeckleUnity.Console.Log("Updates added");
    // 		}
    // 	});
    // }

    void OnDisable()
    {
      // TODO: should client be disposed here?
      client?.Dispose();
    }

    void OnDestroy()
    {
      client?.Dispose();
    }

    async UniTask<bool> TryLoadStream(string streamId)
    {
      if(sourceToken != null && sourceToken.Token.CanBeCanceled)
      {
        sourceToken.Cancel();
        sourceToken.Dispose();
      }

      sourceToken = new CancellationTokenSource();

      if(baseAccount == null)
      {
        SpeckleUnity.Console.Warn($"Invalid {nameof(Core.Credentials.Account)}\n" + $"{(baseAccount != null ? baseAccount.ToString() : "invalid")}");
        return false;
      }

      client = new SpeckleClient(account.source);

      if(!client.IsValid())
      {
        SpeckleUnity.Console.Warn($"{name} did not complete {nameof(LoadStream)} properly. Seems like the client is invalid");
        return false;
      }

      client.token = sourceToken.Token;

      if(!streamId.Valid())
      {
        SpeckleUnity.Console.Warn("Invalid Stream input\n" + $"stream :{(streamId.Valid() ? streamId : "invalid")}");
        return false;
      }

      stream = new SpeckleStream(await client.StreamGet(streamId));

      if(!stream.IsValid())
      {
        SpeckleUnity.Console.Warn($"{name} did not complete {nameof(LoadStream)} properly. Seems like the stream is invalid");
        return false;
      }

      return true;
    }

    async UniTask LoadObject(string objectId, bool updatePreview = true)
    {
      await stream.LoadObject(client, objectId);

      if(updatePreview)
        await UpdatePreview();
    }

    async UniTask LoadCommit(string commitId, bool updatePreview = true)
    {
      await stream.LoadCommit(client, commitId);

      OnCommitSet?.Invoke(Commit);

      if(updatePreview)
        await UpdatePreview();
    }

    async UniTask LoadBranch(string branchName, bool updatePreview = true)
    {
      await stream.LoadBranch(client, branchName);

      OnBranchSet?.Invoke(Branch);

      if(updatePreview)
        await UpdatePreview();
    }

    async UniTask UpdatePreview()
    {
      if(!IsValid())
      {
        Debug.Log("Invalid for Preview");
        await UniTask.Yield();
      }

      preview = await Utils.GetTexture(stream.GetUrl(true, baseAccount.serverInfo.url));

      OnPreviewSet?.Invoke(preview);

      await UniTask.Yield();
    }

  }

}
