using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
using Speckle.ConnectorUnity.Converter;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

  public abstract class OpsBehaviour<TArgs> : ClientBehaviour, IOps, IOpsEvent, IOpsWork<TArgs>
    where TArgs : OpsWorkArgs
  {

    [SerializeField] protected SpeckleObjectBehaviour root;
    [SerializeField] protected ScriptableConverter converter;

    [SerializeField, HideInInspector] protected SpeckleStream stream;

    [SerializeField, HideInInspector] float progressAmount;
    [SerializeField, HideInInspector] int childCountTotal;

    public event UnityAction OnStreamSet;

    public Stream Stream => stream?.source;

    public Branch Branch => stream?.Branch;

    public List<Branch> Branches => stream?.Branches ?? new List<Branch>();

    public List<Commit> Commits => stream?.Commits ?? new List<Commit>();

    public Commit Commit => stream?.Commit;

    public bool IsWorking { get; protected set; }

    public float Progress
    {
      get => progressAmount;
      protected set => progressAmount = value;
    }

    public int TotalChildCount
    {
      get => childCountTotal;
      protected set => childCountTotal = value;
    }

    public TArgs Args { get; protected set; }

    public ISpeckleConverter Converter { get; protected set; }

    public async UniTask LoadStream(string streamId)
    {
      try
      {
        if (!client.IsValid())
        {
          SpeckleUnity.Console.Warn($"{name} did not complete {nameof(LoadStream)} properly. Seems like the client is invalid");
          return;
        }

        stream = new SpeckleStream(await client.StreamGet(streamId));

        if (!stream.IsValid())
        {
          SpeckleUnity.Console.Warn($"{name} did not complete {nameof(LoadStream)} properly. Seems like the stream is invalid");
          return;
        }

        SpeckleUnity.Console.Log($"{name} is all ready to go! {Stream}");

        name = this.GetType() + $"-{Stream.id}";

        OnStreamSet?.Invoke();

        await PostLoadStream();
        await PostLoadBranch();
      }
      catch (SpeckleException e)
      {
        SpeckleUnity.Console.Log(e.Message);
      }
      catch (Exception e)
      {
        SpeckleUnity.Console.Log(e.Message);
      }
    }

    public async UniTask Initialize(Account obj, string streamId)
    {
      await Initialize(obj);
      if (IsValid())
      {
        await LoadStream(streamId);
      }

    }
    protected virtual async UniTask PostLoadStream()
    {
      if (!Branches.Valid())
      {
        SpeckleUnity.Console.Log("No Branches on this stream!");
        return;
      }

      await SetBranch("main");
    }

    protected virtual async UniTask PostLoadBranch()
    {
      if (Branch == null)
      {
        SpeckleUnity.Console.Log("No branch set on this stream!");
        return;
      }

      await SetCommit(0);
    }

    protected virtual UniTask PostLoadCommit() => UniTask.CompletedTask;

    protected abstract void SetSubscriptions();

    public void SetDefaultActions(
      UnityAction<ConcurrentDictionary<string, int>> onProgressAction = null,
      UnityAction<string, Exception> onErrorAction = null,
      UnityAction<int> onTotalChildCountAction = null
    )
    {
      OnTotalChildCountAction = onTotalChildCountAction ?? (i => TotalChildCount = i);
      OnErrorAction = onErrorAction ?? ((message, exception) => SpeckleUnity.Console.Log($"Error From Client:{message}\n{exception.Message}"));
      OnProgressAction = onProgressAction
                         ?? (args =>
                         {
                           // from speckle gh connector
                           var total = 0.0f;
                           foreach (var kvp in args)
                           {
                             //NOTE: progress set to indeterminate until the TotalChildrenCount is correct
                             total += kvp.Value;
                           }

                           Progress = total / args.Keys.Count;
                         });
    }

    public async UniTask SetCommit(string commitId)
    {
      if (stream.CommitSet(commitId))
        await LoadCommit(commitId);
    }

    public async UniTask SetCommit(int commitIndex)
    {
      if (stream.CommitSet(commitIndex))
        await LoadCommit(stream.Commits[commitIndex].id);
    }

    public async UniTask SetBranch(string branchName)
    {
      if (stream.BranchSet(branchName))
        await LoadBranch(branchName);
    }

    public async UniTask SetBranch(int branchIndex)
    {
      if (stream.BranchSet(branchIndex))
        await LoadBranch(stream.Branches[branchIndex].name);
    }

    async UniTask LoadCommit(string commitId)
    {
      await stream.LoadCommit(client, commitId);

      OnCommitSet?.Invoke(Commit);

      await PostLoadCommit();
    }

    async UniTask LoadBranch(string branchName)
    {
      await stream.LoadBranch(client, branchName);

      OnBranchSet?.Invoke(Branch);

      await PostLoadBranch();
    }


    /// <inheritdoc/> and <see cref="stream"/> is valid 
    public override bool IsValid() => base.IsValid() && stream != null && stream.IsValid();

    public string GetUrl() => IsValid() ? stream.GetUrl(false, client.Account.serverInfo.url) : string.Empty;

    public async UniTask DoWork()
    {
      Args = Activator.CreateInstance<TArgs>();

      Progress = 0f;

      if (!IsValid())
      {
        Args.message = "Invalid Client";
        SpeckleUnity.Console.Warn($"{name}-" + Args.message);
      }
      else if (Converter == null)
      {
        Args.message = "No active converter found";
        SpeckleUnity.Console.Warn($"{name}-" + Args.message);
      }
      else
      {
        IsWorking = true;
        await Execute();
      }

      OnClientUpdate?.Invoke(Args);
    }

    public async UniTask DoWork(ISpeckleConverter converter)
    {
      if (converter != null)
      {
        Converter = converter;
      }
      else
      {
        // // TODO: during the build process this should compile and store these objects. 
        if (this.converter == null)
        {
					#if UNITY_EDITOR
					converter = SpeckleUnity.GetDefaultConverter();
					#endif
        }

        Converter = this.converter;
      }

      await DoWork();
    }

		#region inherited event handles

    protected void HandleProgress(ConcurrentDictionary<string, int> args) => OnProgressAction?.Invoke(args);

    protected void HandleError(string message, Exception exception) => OnErrorAction?.Invoke(message, exception);

    protected void HandleChildCount(int args) => UniTask.Create(async () =>
    {
      // Necessary for calling to main thread
      await UniTask.Yield();
      OnTotalChildCountAction?.Invoke(args);
      SpeckleUnity.Console.Log($"Data with {TotalChildCount}");
    });

    protected void HandleRefresh() => OnClientRefresh?.Invoke();

		#endregion

    public event UnityAction<Commit> OnCommitSet;

    public event UnityAction<Branch> OnBranchSet;

    public event UnityAction OnClientRefresh;

    public event UnityAction<ConcurrentDictionary<string, int>> OnProgressAction;

    public event UnityAction<string, Exception> OnErrorAction;

    public event UnityAction<int> OnTotalChildCountAction;

    public event UnityAction<TArgs> OnClientUpdate;



    protected abstract UniTask Execute();

  }

}
