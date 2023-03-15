using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Speckle.ConnectorUnity;
using Speckle.ConnectorUnity.Converter;
using Speckle.ConnectorUnity.Models;
using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

internal struct StreamRef
{
  public string streamId;
  public string branchName;
  public string commitId;
  public string objectId;
}

internal static class SpT
{
  public const string C_CLIENT = "Client";
  public const string C_STREAM = "Stream";
  public const string C_OPS = "Operations";
  public const string C_MODEL = "Models";

  public static readonly StreamRef BCHP = new StreamRef()
  {
    streamId = "9b692137ca",
    branchName = "viewstudy/all-targets", // active study
    commitId = "0f543d7d92", // result cloud commit
    objectId = "989d0ec67f26fa7889000a3850df8dca", // result cloud
  };

  public static readonly StreamRef SIMPLE = new StreamRef()
  {
    streamId = "4777dea055",
    branchName = "main",
    commitId = "12bb31b6c9", // mesh only
    objectId = "1a60ced098216e2839d2952e33bcdef1", // mesh object
  };

  public static async UniTask<Base> GetMesh()
  {
    const string stream = "4777dea055";
    const string id = "d611a3e8bf64984c50147e3f9238c925";
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var obj = await client.ObjectGet(stream, id);
    return await SpeckleOps.Receive(client, stream, obj.id);
  }

}

[TestFixture]
public class Integrations
{

  Account _account;
  SpeckleClient _client;
  List<string> _possibleStreamsToDelete;
  List<string> _possibleCommitsToDelete;

  [OneTimeSetUp]
  public void Setup()
  {
    _account = AccountManager.GetDefaultAccount();
    _client = new SpeckleClient(_account);
    _possibleStreamsToDelete = new List<string>();
    _possibleCommitsToDelete = new List<string>();
  }

  [OneTimeTearDown]
  public void Clean()
  {
    if(!_client.IsValid())
      return;

    if(_possibleStreamsToDelete.Any())
      UniTask.Create(async () => { await UniTask.WhenAll(_possibleStreamsToDelete.Select(x => _client.StreamDelete(x))); });

    if(_possibleCommitsToDelete.Any())
      UniTask.Create(async () =>
      {
        await UniTask.WhenAll(
          _possibleCommitsToDelete.Select(x => _client.CommitDelete(new CommitDeleteInput
            {streamId = SpT.BCHP.streamId, id = x})));
      });
  }

  [UnityTest, Category(SpT.C_CLIENT)]
  public IEnumerator Client_Stream_CreateUpdateDelete() => UniTask.ToCoroutine(async () =>
  {
    var res = await _client.StreamCreate("Test From Unity", "A throwaway stream for testing in unity", true);
    _possibleStreamsToDelete.Add(res);

    Assert.IsNotNull(res);
    Assert.IsTrue(await _client.StreamExists(res));

    const string uName = "New Fun Name";
    const string uDescription = "Wow Update!";

    Assert.IsTrue(await _client.StreamUpdate(new StreamUpdateInput {id = res, name = uName, description = uDescription}));

    var stream = await _client.StreamGet(res, 0);
    Assert.IsTrue(stream.description.Equals(uDescription) && stream.name.Equals(uName));

    Assert.IsTrue(await _client.StreamDelete(stream.id));
    Assert.IsFalse(await _client.StreamExists(stream.id));

    _possibleStreamsToDelete.Remove(res);
  });

  [UnityTest, Category(SpT.C_CLIENT)]
  public IEnumerator Client_Commit_CreateUpdateDelete() => UniTask.ToCoroutine(async () =>
  {
    var @base = new Base
    {
      ["Prop"] = 10,
      ["Child"] = new Base()
    };

    var res = await _client.CommitCreate(new CommitCreateInput
    {
      objectId = @base.GetId(),
      message = "How Neat!",
      branchName = SpT.BCHP.branchName,
      streamId = SpT.BCHP.streamId,
      sourceApplication = SpeckleUnity.APP,
      parents = new List<string>(),
      totalChildrenCount = (int)@base.GetTotalChildrenCount() // will throw an error if no kiddos are found
    });

    Assert.IsTrue(res.Valid());
    _possibleCommitsToDelete.Add(res);

    var commit = await _client.CommitGet(SpT.BCHP.streamId, res);

    Assert.IsNotNull(commit);
    Assert.IsTrue(commit.branchName.Equals(SpT.BCHP.branchName) && commit.id.Equals(res));

    const string uMsg = "Wow update!";
    Assert.IsTrue(await _client.CommitUpdate(new CommitUpdateInput() {streamId = SpT.BCHP.streamId, id = res, message = uMsg}));

    commit = await _client.CommitGet(SpT.BCHP.streamId, res);
    Assert.IsTrue(commit.message.Equals(uMsg));

    Assert.IsTrue(await _client.CommitDelete(new CommitDeleteInput() {id = res, streamId = SpT.BCHP.streamId}));

    _possibleCommitsToDelete.Remove(res);
  });

  [UnityTest, Category(SpT.C_OPS)]
  public IEnumerator Operations_Receive_FromCommit() => UniTask.ToCoroutine(async () =>
  {
    var commit = await _client.CommitGet(SpT.BCHP.streamId, SpT.BCHP.commitId);
    var res = await SpeckleOps.Receive(_client, SpT.BCHP.streamId, commit.referencedObject);

    Assert.IsNotNull(res);
  });

  [UnityTest, Category(SpT.C_OPS)]
  public IEnumerator Operations_Receive_FromObject() => UniTask.ToCoroutine(async () =>
  {
    var commit = await _client.ObjectGet(SpT.BCHP.streamId, SpT.BCHP.objectId);

    var res = await SpeckleOps.Receive(_client, SpT.BCHP.streamId, commit.id);

    Assert.IsNotNull(res);
  });

  [UnityTest, Category(SpT.C_OPS)]
  public IEnumerator Operations_Receive_RunDirect() => UniTask.ToCoroutine(async () =>
  {
    var client = new GameObject().AddComponent<Receiver>();

    await UniTask.Yield();

    await client.Initialize(AccountManager.GetDefaultAccount(), SpT.SIMPLE.streamId);

    Assert.IsNotNull(client.Converter);
    Assert.IsNotNull(client.Stream.id == SpT.SIMPLE.streamId);
    Assert.IsTrue(client.Branches.Valid());
    Assert.IsNotNull(client.Branch);
    Assert.IsTrue(client.Commits.Valid().Equals(client.Commit != null));

    Assert.IsTrue(client.IsValid());

    client.Converter.SetConverterSettings(new ConverterSettings() {style = ConverterSettings.ConversionStyle.Sync});
    await client.DoWork();

    Assert.IsNotNull(client.Args);
    Assert.IsTrue(client.Args.Success);
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.Message));
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.ReferenceObj));

    client.Converter.SetConverterSettings(new ConverterSettings() {style = ConverterSettings.ConversionStyle.Sync});
    await client.DoWork();

    Assert.IsNotNull(client.Args);
    Assert.IsTrue(client.Args.Success);
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.Message));
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.ReferenceObj));
  });

  [UnityTest, Category(SpT.C_OPS)]
  public IEnumerator Operations_Receive_RunQueue() => UniTask.ToCoroutine(async () =>
  {
    var client = new GameObject().AddComponent<Receiver>();

    await UniTask.Yield();

    await client.Initialize(AccountManager.GetDefaultAccount());


    Assert.IsNotNull(client.Converter);
    Assert.IsNotNull(client.Stream.id == SpT.SIMPLE.streamId);
    Assert.IsTrue(client.Branches.Valid());
    Assert.IsNotNull(client.Branch);
    Assert.IsTrue(client.Commits.Valid().Equals(client.Commit != null));

    Assert.IsTrue(client.IsValid());

    client.Converter.SetConverterSettings(new ConverterSettings {style = ConverterSettings.ConversionStyle.Queue});
    await client.DoWork();
    Assert.IsNotNull(client.Args);

    Assert.IsTrue(client.Args.Success);
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.Message));
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.ReferenceObj));

    client.Converter.SetConverterSettings(new ConverterSettings {style = ConverterSettings.ConversionStyle.Queue});
    await client.DoWork();

    Assert.IsNotNull(client.Args);
    Assert.IsTrue(client.Args.Success);
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.Message));
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.ReferenceObj));
  });

  [UnityTest, Category(SpT.C_OPS)]
  public IEnumerator Operations_Send_Run() => UniTask.ToCoroutine(async () =>
  {
    var client = new GameObject().AddComponent<Sender>();

    await UniTask.Yield();

    await client.Initialize(AccountManager.GetDefaultAccount(), SpT.SIMPLE.streamId);

    Assert.IsNotNull(client.Converter);
    Assert.IsNotNull(client.Stream.id == SpT.SIMPLE.streamId);
    Assert.IsTrue(client.Branches.Valid());
    Assert.IsNotNull(client.Branch);
    Assert.IsTrue(client.Commits.Valid().Equals(client.Commit != null));

    Assert.IsTrue(client.IsValid());

    // Send using only Base object
    var @base = new Base
    {
      ["Prop"] = 10,
      ["Child"] = new Base()
    };

    await client.DoWork(@base);

    Assert.IsNotNull(client.Args);
    Assert.IsTrue(client.Args.Success);
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.Message));
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.CommitId));
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.URL));

    Assert.IsTrue(await _client.CommitDelete(new CommitDeleteInput
      {streamId = client.Stream.id, id = client.Args.CommitId}));

    // Send using speckle node
    var obj = new GameObject("Speckle Object").AddComponent<SpeckleObjectBehaviour>();
    var layer = new GameObject("Speckle Layer").AddComponent<SpeckleLayer>();
    var baseProp = new GameObject("Base").AddComponent<BaseBehaviour>();

    baseProp.Store(@base);
    layer.Add(baseProp.gameObject);
    obj.Hierarchy.Add(layer);

    await client.DoWork(obj);

    Assert.IsNotNull(client.Args);
    Assert.IsTrue(client.Args.Success);
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.Message));
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.CommitId));
    Assert.IsTrue(!string.IsNullOrEmpty(client.Args.URL));

    Assert.IsTrue(await _client.CommitDelete(new CommitDeleteInput() {streamId = client.Stream.id, id = client.Args.CommitId}));
  });

}

[TestFixture]
public class Units
{
  [Test, Category(SpT.C_CLIENT)]
  public void Client_IsValid()
  {
    // valid test
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    Assert.IsNotNull(client);
    Assert.IsTrue(client.IsValid());

    client.Dispose();

    // invalid test
    client = new SpeckleClient(null);
    Assert.IsNotNull(client);
    Assert.IsFalse(client.IsValid());
  }

  [Test, Category(SpT.C_CLIENT)]
  public void Client_Account_ByDefault()
  {
    // valid test
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    Assert.IsNotNull(client);
    Assert.IsTrue(client.Account.Equals(AccountManager.GetDefaultAccount()));

    client.Dispose();

    // invalid test
    client = new SpeckleClient(null);
    Assert.IsNotNull(client);
    Assert.IsNull(client.Account);
  }

  [Test, Category(SpT.C_CLIENT)]
  public void Client_Account_ByName()
  {
    var account = SpeckleAccountManager.GetAccountByName("David Morgan");
    // valid test
    var client = new SpeckleClient(account);
    Assert.IsNotNull(client);
    Assert.IsTrue(client.Account.id.Equals(account.id));
    Assert.IsTrue(client.Account.Equals(account));

    client.Dispose();

    account = SpeckleAccountManager.GetAccountByName("No Name here!");
    // invalid test
    client = new SpeckleClient(account);
    Assert.IsNotNull(client);
    Assert.IsNull(client.Account);
  }

  [UnityTest, Category(SpT.C_CLIENT)]
  public IEnumerator Client_StreamGet() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var stream = await client.StreamGet(SpT.BCHP.streamId);
    Assert.IsNotNull(stream);
    Assert.IsTrue(stream.id.Equals(SpT.BCHP.streamId));

    // test for wrapper
    var wrapper = new SpeckleStream(stream);
    Assert.IsTrue(wrapper.Equals(stream));
    Assert.IsTrue(wrapper.Branches.Valid());

    Assert.IsNull(wrapper.Commit);
    Assert.IsNull(wrapper.Branch);
    Assert.IsFalse(wrapper.Commits.Valid());
    Assert.IsTrue(wrapper.Object == stream.@object);
  });

  [UnityTest, Category(SpT.C_CLIENT)]
  public IEnumerator Client_BranchGet() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var res = await client.BranchGet(SpT.BCHP.streamId, SpT.BCHP.branchName);

    Assert.IsNotNull(res);
  });

  [UnityTest, Category(SpT.C_CLIENT)]
  public IEnumerator Client_BranchesGet() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var res = await client.BranchesGet(SpT.BCHP.streamId);

    Assert.IsTrue(res.Valid());
  });

  [UnityTest, Category(SpT.C_CLIENT)]
  public IEnumerator Client_CommitGet() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var res = await client.CommitGet(SpT.BCHP.streamId, SpT.BCHP.commitId);

    Assert.IsNotNull(res);
    Assert.IsTrue(res.id.Equals(SpT.BCHP.commitId));
  });

  [UnityTest, Category(SpT.C_CLIENT)]
  public IEnumerator Client_ObjectGet() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var res = await client.ObjectGet(SpT.BCHP.streamId, SpT.BCHP.objectId);

    Assert.IsNotNull(res);
    Assert.IsTrue(res.id.Equals(SpT.BCHP.objectId));
  });

  [UnityTest, Category(SpT.C_STREAM)]
  public IEnumerator Stream_LoadObject() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var wrapper = new SpeckleStream(await client.StreamGet(SpT.BCHP.streamId));

    Assert.IsNotNull(wrapper);
    Assert.IsTrue(wrapper.Id.Equals(SpT.BCHP.streamId));

    Assert.IsTrue(await wrapper.LoadObject(client, SpT.BCHP.objectId));
    Assert.IsTrue(wrapper.Object.id.Equals(SpT.BCHP.objectId));
  });

  [UnityTest, Category(SpT.C_STREAM)]
  public IEnumerator Stream_LoadCommit() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var wrapper = new SpeckleStream(await client.StreamGet(SpT.BCHP.streamId));

    Assert.IsNotNull(wrapper);
    Assert.IsTrue(await wrapper.LoadCommit(client, SpT.BCHP.commitId));
    Assert.IsNotNull(wrapper.Commit);
    Assert.IsTrue(wrapper.Commit.id.Equals(SpT.BCHP.commitId));
  });

  [UnityTest, Category(SpT.C_STREAM)]
  public IEnumerator Stream_LoadCommits() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var wrapper = new SpeckleStream(await client.StreamGet(SpT.BCHP.streamId));

    Assert.IsNotNull(wrapper);
    Assert.IsTrue(await wrapper.LoadCommits(client));
    Assert.IsNotEmpty(wrapper.Commits);
    Assert.IsNull(wrapper.Commit);
  });

  [UnityTest, Category(SpT.C_STREAM)]
  public IEnumerator Stream_LoadBranch() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var wrapper = new SpeckleStream(await client.StreamGet(SpT.BCHP.streamId));

    Assert.IsNotNull(wrapper);
    Assert.IsTrue(await wrapper.LoadBranch(client, SpT.BCHP.branchName));
    Assert.IsNotNull(wrapper.Branch);
    Assert.IsTrue(wrapper.Branch.name.Equals(SpT.BCHP.branchName));
    Assert.IsTrue(wrapper.Commits.Valid());
    Assert.IsNull(wrapper.Commit);
  });

  [UnityTest, Category(SpT.C_STREAM)]
  public IEnumerator Stream_LoadBranches() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var wrapper = new SpeckleStream(await client.StreamGet(SpT.BCHP.streamId));

    Assert.IsNotNull(wrapper);
    Assert.IsTrue(wrapper.Id.Equals(SpT.BCHP.streamId));

    Assert.IsTrue(await wrapper.LoadBranches(client));
    Assert.IsTrue(wrapper.Branches.Valid());

    Assert.IsNull(wrapper.Commit);
    Assert.IsNull(wrapper.Branch);
    Assert.IsFalse(wrapper.Commits.Valid());
  });

  [UnityTest, Category(SpT.C_STREAM)]
  public IEnumerator Stream_LoadActivity() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var wrapper = new SpeckleStream(await client.StreamGet(SpT.BCHP.streamId));

    Assert.IsNotNull(wrapper);
    Assert.IsNull(wrapper.Activity);

    Assert.IsTrue(await wrapper.LoadActivity(client));
    Assert.IsNotNull(wrapper.Activity);
    Assert.IsNotEmpty(wrapper.Activity.items);
  });

  [UnityTest, Category(SpT.C_STREAM)]
  public IEnumerator Stream_LoadTypes() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());
    var wrapper = new SpeckleStream(await client.StreamGet(SpT.BCHP.streamId));

    Assert.IsNotNull(wrapper);
    Assert.IsTrue(wrapper.Type == StreamWrapperType.Stream);

    Assert.IsTrue(await wrapper.LoadBranch(client, SpT.BCHP.branchName));
    Assert.IsTrue(wrapper.Type == StreamWrapperType.Branch);

    Assert.IsTrue(await wrapper.LoadCommit(client, SpT.BCHP.commitId));
    Assert.IsTrue(wrapper.Type == StreamWrapperType.Commit);
  });

  [UnityTest, Category(SpT.C_CLIENT)]
  public IEnumerator Object_LoadFromStream() => UniTask.ToCoroutine(async () =>
  {
    var client = new SpeckleClient(AccountManager.GetDefaultAccount());

    var res = await client.ObjectGet(SpT.BCHP.streamId, SpT.BCHP.objectId);

    Debug.Log("Object ID: " + res.id);
    Debug.Log("Object Type: " + res.speckleType);
    Debug.Log("Object Count: " + res.totalChildrenCount);
    Debug.Log("Application ID: " + res.applicationId);
    Debug.Log("Created At: " + res.createdAt);

    Assert.IsNotNull(res);
    Assert.IsTrue(res.id.Equals(SpT.BCHP.objectId));
  });

  [UnityTest, Category(SpT.C_MODEL)]
  public IEnumerator Base_StoreObject() => UniTask.ToCoroutine(async () =>
  {
    var @base = await SpT.GetMesh();

    Assert.IsNotNull(@base);
    Assert.IsTrue(@base.id.Valid());
    Assert.IsTrue(@base.speckle_type.Valid());

    var bb = new GameObject().AddComponent<BaseBehaviour>();
    await bb.Store(@base);

    Assert.IsTrue(bb.ID.Valid() == @base.id.Valid() && bb.ID == @base.id);
    Assert.IsTrue(bb.SpeckleType.Valid() == @base.speckle_type.Valid() && bb.SpeckleType == @base.speckle_type);
    Assert.IsTrue(bb.ApplicationId.Valid() == @base.applicationId.Valid() && bb.ApplicationId == @base.applicationId);
    Assert.IsTrue(bb.TotalChildCount == @base.totalChildrenCount);
  });

}
