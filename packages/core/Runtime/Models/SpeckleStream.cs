using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{

  [Serializable]
  public class SpeckleStream : GenericAdapter<Stream>
  {
    [SerializeField] string id;
    [SerializeField] string name;
    [SerializeField] string description;
    [SerializeField] string role;
    [SerializeField] string createdAt;
    [SerializeField] string updatedAt;
    [SerializeField] string favoritedDate;
    [SerializeField] bool isPublic;

    [SerializeField] SpeckleCommit commit;
    [SerializeField] SpeckleBranch branch;
    [SerializeField] List<SpeckleBranch> branches;
    [SerializeField] List<SpeckleCommit> commits;
    [SerializeField] SpeckleObjectAdapter speckleObject;

    public string Name => name;

    public string Id => id;

    public string Description => description;

    public string Role => role;

    public string CreatedAt => createdAt;

    public string UpdatedAt => updatedAt;

    public string FavoritedDate => favoritedDate;

    public bool IsPublic => isPublic;

    public event Action<Branch> OnBranchSet;

    public event Action<Commit> OnCommitSet;

    public SpeckleStream(Stream value) : base(value)
    {
      if(value == null)
        return;

      id = value.id;
      name = value.name;
      description = value.description;
      role = value.role;
      createdAt = value.createdAt;
      updatedAt = value.updatedAt;
      favoritedDate = value.favoritedDate;
      isPublic = value.isPublic;

      activity = value.activity;

      Object = value.@object;
      Branch = value.branch;
      Commit = value.commit;

      Branches = value.branches?.items;
      Commits = value.commits?.items;
    }

    public StreamWrapperType type
    {
      get
      {
        var res = StreamWrapperType.Undefined;

        if(Object.Valid())
          res = StreamWrapperType.Object;
        else if(Commit.Valid())
          res = StreamWrapperType.Commit;
        else if(Branch.Valid())
          res = StreamWrapperType.Branch;
        else if(Id.Valid())
          res = StreamWrapperType.Stream;

        return res;
      }
    }

    public Activity activity { get; set; }

    public SpeckleObject Object
    {
      get => speckleObject?.source;
      set
      {
        if(value == null)
          return;

        speckleObject = new SpeckleObjectAdapter(value);
      }
    }

    /// <summary>
    /// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.BranchGet(System.Threading.CancellationToken,System.String,System.String,System.Int32)" />.
    /// </summary>
    public Branch Branch
    {
      get => branch?.source;
      set
      {
        if(value == null)
          return;

        branch = new SpeckleBranch(value);
        SpeckleUnity.Console.Log($"Setting Active {nameof(Branch)} to {branch.source.name}");

        Commits = value.commits?.items ?? new List<Commit>();

        OnBranchSet?.Invoke(branch.source);
      }
    }

    /// <summary>
    /// <para>Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.CommitGet(System.Threading.CancellationToken,System.String,System.String)" />.</para>
    /// Sets the <see cref="Branch"/> name, but does not load all the of existing commits from that branch. Use <see cref="LoadBranch"/> to get all items in a branch 
    /// </summary>
    public Commit Commit
    {
      get => commit?.source;
      set
      {
        if(value == null)
          return;

        commit = new SpeckleCommit(value);
        Branch = new Branch {name = value.branchName};

        SpeckleUnity.Console.Log($"Setting Active {nameof(Commit)} to {commit.id}");
        OnCommitSet?.Invoke(commit.source);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public List<Branch> Branches
    {
      get => branches.Valid() ? branches.Select(x => x.source).ToList() : new List<Branch>();
      set
      {
        if(value == null || !value.Valid())
          return;

        branches = value.Select(x => new SpeckleBranch(x)).ToList();
      }
    }

    /// <summary>
    /// Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.StreamGetCommits(System.Threading.CancellationToken,System.String,System.Int32)" />
    /// </summary>
    public List<Commit> Commits
    {
      get => commits.Valid() ? commits.Select(x => x.source).ToList() : new List<Commit>();
      set
      {
        if(value == null || !value.Valid())
          return;

        commits = value.Select(x => new SpeckleCommit(x)).ToList();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public Branch BranchGet(int input) => branches.Valid(input) ? branches[input].source : null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public Branch BranchGet(string input)
    {
      Branch res = null;

      if(branches.Valid() && input.Valid())
        foreach(var b in branches)
          if(b.name.Valid() && b.name.Equals(input))
            res = b.source;

      return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool BranchSet(int input)
    {
      Branch = BranchGet(input);
      return Branch != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool BranchSet(string input)
    {
      Branch = BranchGet(input);
      return Branch != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="commitId"></param>
    /// <returns></returns>
    public Commit CommitGet(int commitId) => commits.Valid(commitId) ? commits[commitId].source : null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="commitId"></param>
    /// <returns></returns>
    public Commit CommitGet(string commitId)
    {
      Commit res = null;

      if(commits.Valid() && commitId.Valid())
        foreach(var b in commits)
          if(b.id.Valid() && b.id.Equals(commitId))
            res = b.source;

      return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool CommitSet(int input)
    {
      Commit = CommitGet(input);
      return Commit != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool CommitSet(string input)
    {
      Commit = CommitGet(input);
      return Commit != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isPreview"></param>
    /// <param name="serverUrl"></param>
    /// <returns></returns>
    public string GetUrl(bool isPreview, string serverUrl) => $"{serverUrl}/{GetUrl(isPreview)}";

    /// <summary>
    /// Gets a value a url for how this stream is setup
    /// </summary>
    /// <param name="isPreview"></param>
    /// <returns>returns the url without the server url</returns>
    public string GetUrl(bool isPreview)
    {
      string url = $"{(isPreview ? "preview" : "streams")}/{Id}";
      switch(type)
      {
        case StreamWrapperType.Stream:
          return url;
        case StreamWrapperType.Commit:
          url += $"/commits/{Commit.id}";
          break;
        case StreamWrapperType.Branch:
          url += $"/branches/{Branch.name}";
          break;
        case StreamWrapperType.Object:
          url += $"objects/{Object.id}";
          break;
        case StreamWrapperType.Undefined:
        default:
          SpeckleUnity.Console.Warn($"{Id} is not a valid stream, bailing on the preview thing");
          url = null;
          break;
      }

      Debug.Log($"URL found {url}");

      return url;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public async UniTask<bool> ModifyInfo(SpeckleUnityClient client, StreamUpdateInput input)
    {
      var res = false;
      if(client.IsValid() && input != null)
        res = await client.StreamUpdate(input);

      return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async UniTask<bool> LoadObject(SpeckleUnityClient client, string value)
    {
      if(client.IsValid())
        Object = await client.ObjectGet(Id, value);

      return Object != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="branchLimit"></param>
    /// <param name="commitLimit"></param>
    /// <returns></returns>
    public async UniTask<bool> LoadBranches(SpeckleUnityClient client, int branchLimit = 10, int commitLimit = 5)
    {
      if(client.IsValid())
        Branches = await client.BranchesGet(Id, branchLimit, commitLimit);

      return Branches.Valid();
    }

    /// <summary>
    /// Loads a stream branch and commits
    /// </summary>
    /// <param name="client"></param>
    /// <param name="branchName"></param>
    /// <param name="commitLimit"></param>
    /// <returns></returns>
    public async UniTask<bool> LoadBranch(SpeckleUnityClient client, string branchName, int commitLimit = 10)
    {
      if(client.IsValid())
      {
        Branch = await client.BranchGet(Id, branchName, commitLimit);
      }

      return Branch != null && Branch.name.Equals(branchName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public async UniTask<bool> LoadCommit(SpeckleUnityClient client, string input)
    {
      if(client.IsValid())
        Commit = await client.CommitGet(Id, input);

      return Commit != null && Commit.id == input;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public async UniTask<bool> LoadCommits(SpeckleUnityClient client, int limit = 10)
    {
      if(client.IsValid())
        Commits = await client.source.StreamGetCommits(client.token, Id, limit);

      return Commits.Valid();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <param name="cursor"></param>
    /// <param name="actionType"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public async UniTask<bool> LoadActivity(
      SpeckleUnityClient client,
      DateTime? before = null, DateTime? after = null, DateTime? cursor = null,
      string actionType = null,
      int limit = 10
    )
    {
      if(client.IsValid())
      {
        var res = await client.StreamActivity(Id, before, after, cursor, actionType, limit);
        if(res.Valid())
        {
          activity = new Activity()
          {
            cursor = cursor ?? default,
            totalCount = res.Count,
            items = res
          };
        }
      }

      return activity != null;
    }

    public bool Equals(SpeckleStream obj) => Equals(obj.source);

    public bool Equals(Stream obj) => source != null && obj != null && Id.Equals(obj.id) && Name.Equals(obj.name);

    protected override Stream Get() => new Stream
    {
      id = this.Id,
      name = this.Name,
      description = this.Description,
      role = this.Role,
      createdAt = this.CreatedAt,
      updatedAt = this.UpdatedAt,
      favoritedDate = this.FavoritedDate,
      isPublic = this.IsPublic
    };

    void Clear()
    {
      id = string.Empty;
      name = string.Empty;
      description = string.Empty;
      role = string.Empty;
      createdAt = string.Empty;
      updatedAt = string.Empty;
      favoritedDate = string.Empty;
      isPublic = false;

      Branches = null;
      Commits = null;
      activity = null;
      Object = null;
    }

    /// <summary>
    /// Simple check if the id is set to this stream
    /// </summary>
    /// <returns></returns>
    public bool IsValid() => Id.Valid();

  }

}
