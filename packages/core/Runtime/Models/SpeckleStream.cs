using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{

  public static class SpeckleCommander
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isPreview"></param>
    /// <param name="serverUrl"></param>
    /// <returns></returns>
    public static string GetUrl(this IStreamSpeckle obj, bool isPreview, string serverUrl) => $"{serverUrl}/{obj.GetUrl(isPreview)}";

    /// <summary>
    /// Gets a value a url for how this stream is setup
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isPreview"></param>
    /// <returns>returns the url without the server url</returns>
    public static string GetUrl(this IStreamSpeckle obj, bool isPreview)
    {
      string url = $"{(isPreview ? "preview" : "streams")}/{obj.Info.Id}";
      switch(obj.Type)
      {
        case StreamWrapperType.Stream:
          return url;
        case StreamWrapperType.Commit:
          url += $"/commits/{obj.Commit.id}";
          break;
        case StreamWrapperType.Branch:
          url += $"/branches/{obj.Branch.name}";
          break;
        case StreamWrapperType.Object:
          url += $"objects/{obj.Object.id}";
          break;
        case StreamWrapperType.Undefined:
        default:
          SpeckleUnity.Console.Warn($"{obj.Info.Id} is not a valid stream, bailing on the preview thing");
          url = null;
          break;
      }

      Debug.Log($"URL found {url}");

      return url;
    }
  }

  public interface IStreamSpeckle
  {
    public SpeckleCommit Commit { get; }
    public SpeckleBranch Branch { get; }
    public List<SpeckleBranch> Branches { get; }
    public List<SpeckleCommit> Commits { get; }
    public SpeckleObjectAdapter Object { get; }
    public IStreamInfo Info { get; }
    public StreamWrapperType Type { get; }

  }

  public interface IStreamInfo
  {
    public string Name { get; }

    public string Id { get; }

    public string Description { get; }

    public string Role { get; }

    public string CreatedAt { get; }

    public string UpdatedAt { get; }

    public string FavoritedDate { get; }

    public bool IsPublic { get; }

  }

  [Serializable]
  public class SpeckleStreamV2 : IStreamSpeckle
  {
    [field: SerializeField] public SpeckleCommit Commit { get; private set; }
    [field: SerializeField] public SpeckleBranch Branch { get; private set; }
    [field: SerializeField] public List<SpeckleBranch> Branches { get; private set; }
    [field: SerializeField] public List<SpeckleCommit> Commits { get; private set; }
    [field: SerializeField] public SpeckleObjectAdapter Object { get; private set; }
    [field: SerializeField] public StreamWrapperType Type { get; private set; }

    public IStreamInfo Info { get; private set; }

    public Activity Activity { get; set; }

    public SpeckleStreamV2(Stream value)
    {
      if(value == null) return;

      Info = new StreamInfo(value);

      Activity = value.activity;
      Object = new SpeckleObjectAdapter(value.@object);
      Branch = new SpeckleBranch(value.branch);
      Commit = new SpeckleCommit(value.commit);
      Branches = value.branches.items.Valid() ? value.branches.items.Select(x => new SpeckleBranch(x)).ToList() : new List<SpeckleBranch>();
      Commits = value.commits.items.Valid() ? value.commits.items.Select(x => new SpeckleCommit(x)).ToList() : new List<SpeckleCommit>();
    }
  }

  [Serializable]
  public class StreamInfo : IStreamInfo
  {
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public string Role { get; private set; }
    [field: SerializeField] public string CreatedAt { get; private set; }
    [field: SerializeField] public string UpdatedAt { get; private set; }
    [field: SerializeField] public string FavoritedDate { get; private set; }
    [field: SerializeField] public bool IsPublic { get; private set; }

    public StreamInfo(Stream value)
    {
      Id = value.id;
      Name = value.name;
      Description = value.description;
      Role = value.role;
      CreatedAt = value.createdAt;
      UpdatedAt = value.updatedAt;
      FavoritedDate = value.favoritedDate;
      IsPublic = value.isPublic;
    }
  }

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

    public event Action<SpeckleBranch> OnBranchSet;

    public event Action<SpeckleCommit> OnCommitSet;

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

      Activity = value.activity;

      Object = value.@object;
      Branch = value.branch;
      Commit = value.commit;

      Branches = value.branches?.items;
      Commits = value.commits?.items;
    }

    public StreamWrapperType Type
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

    public Activity Activity { get; set; }

    public SpeckleObject Object
    {
      get => speckleObject?.Source;
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
      get => branch?.Source;
      set
      {
        if(value == null)
          return;

        branch = new SpeckleBranch(value);
        SpeckleUnity.Console.Log($"Setting Active {nameof(Branch)} to {branch.Source.name}");

        Commits = value.commits?.items ?? new List<Commit>();

        OnBranchSet?.Invoke(branch);
      }
    }

    /// <summary>
    /// <para>Set only in the case that you've requested this through <see cref="M:Speckle.Core.Api.Client.CommitGet(System.Threading.CancellationToken,System.String,System.String)" />.</para>
    /// Sets the <see cref="Branch"/> name, but does not load all the of existing commits from that branch. Use <see cref="LoadBranch"/> to get all items in a branch 
    /// </summary>
    public Commit Commit
    {
      get => commit?.Source;
      set
      {
        if(value == null)
          return;

        commit = new SpeckleCommit(value);
        Branch = new Branch {name = value.branchName};

        SpeckleUnity.Console.Log($"Setting Active {nameof(Commit)} to {commit.id}");
        OnCommitSet?.Invoke(commit);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public List<Branch> Branches
    {
      get => branches.Valid() ? branches.Select(x => x.Source).ToList() : new List<Branch>();
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
      get => commits.Valid() ? commits.Select(x => x.Source).ToList() : new List<Commit>();
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
    [Obsolete("Only will get obj by name, use " + nameof(BranchGet))]
    public Branch BranchGet(int input) => branches.Valid(input) ? branches[input].Source : null;

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
            res = b.Source;

      return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [Obsolete("Only will set obj by name, use " + nameof(BranchSet))]
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
    [Obsolete("Only will set obj by name, use " +nameof(CommitGet))]
    public Commit CommitGet(int commitId) => commits.Valid(commitId) ? commits[commitId].Source : null;

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
            res = b.Source;

      return res;
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
    /// <param name="input"></param>
    /// <returns></returns>
    [Obsolete("Only will set obj by name, use " +nameof(CommitSet))]
    public bool CommitSet(int input)
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
      switch(Type)
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
    public async UniTask<bool> ModifyInfo(SpeckleClient client, StreamUpdateInput input)
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
    public async UniTask<bool> LoadObject(SpeckleClient client, string value)
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
    public async UniTask<bool> LoadBranches(SpeckleClient client, int branchLimit = 10, int commitLimit = 5)
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
    public async UniTask<bool> LoadBranch(SpeckleClient client, string branchName, int commitLimit = 10)
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
    public async UniTask<bool> LoadCommit(SpeckleClient client, string input)
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
    public async UniTask<bool> LoadCommits(SpeckleClient client, int limit = 10)
    {
      if(client.IsValid())
        Commits = await client.Source.StreamGetCommits(client.token, Id, limit);

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
      SpeckleClient client,
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
          Activity = new Activity()
          {
            cursor = cursor ?? default,
            totalCount = res.Count,
            items = res
          };
        }
      }

      return Activity != null;
    }

    /// <summary>
    /// Simple check if the id is set to this stream
    /// </summary>
    /// <returns></returns>
    public bool IsValid() => Id.Valid();

    public override string ToString()
    {
      return Source != null ? Source.ToString() : "Invalid Stream(check the source!)";
    }

    public bool Equals(SpeckleStream obj) => Equals(obj.Source);

    public bool Equals(Stream obj) => Source != null && obj != null && Id.Equals(obj.id) && Name.Equals(obj.name);

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
      Activity = null;
      Object = null;
    }

  }

}
