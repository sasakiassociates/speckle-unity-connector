using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Speckle.ConnectorUnity;
using Speckle.ConnectorUnity.Args;
using Speckle.ConnectorUnity.Models;
using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
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

}

[TestFixture]
public class Integrations
{

	Account _account;
	SpeckleUnityClient _client;
	List<string> _possibleStreamsToDelete;
	List<string> _possibleCommitsToDelete;

	[OneTimeSetUp]
	public void Setup()
	{
		_account = AccountManager.GetDefaultAccount();
		_client = new SpeckleUnityClient(_account);
		_possibleStreamsToDelete = new List<string>();
		_possibleCommitsToDelete = new List<string>();
	}

	[OneTimeTearDown]
	public void Clean()
	{
		if (!_client.IsValid())
			return;

		if (_possibleStreamsToDelete.Any())
			UniTask.Create(async () => { await UniTask.WhenAll(_possibleStreamsToDelete.Select(x => _client.StreamDelete(x))); });

		if (_possibleCommitsToDelete.Any())
			UniTask.Create(async () =>
			{
				await UniTask.WhenAll(
					_possibleCommitsToDelete.Select(x => _client.CommitDelete(new CommitDeleteInput
						                                                          { streamId = SpT.BCHP.streamId, id = x })));
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

		Assert.IsTrue(await _client.StreamUpdate(new StreamUpdateInput { id = res, name = uName, description = uDescription }));

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

		Assert.IsTrue(SpeckleUnity.Valid(res));
		_possibleCommitsToDelete.Add(res);

		var commit = await _client.CommitGet(SpT.BCHP.streamId, res);

		Assert.IsNotNull(commit);
		Assert.IsTrue(commit.branchName.Equals(SpT.BCHP.branchName) && commit.id.Equals(res));

		const string uMsg = "Wow update!";
		Assert.IsTrue(await _client.CommitUpdate(new CommitUpdateInput() { streamId = SpT.BCHP.streamId, id = res, message = uMsg }));

		commit = await _client.CommitGet(SpT.BCHP.streamId, res);
		Assert.IsTrue(commit.message.Equals(uMsg));

		Assert.IsTrue(await _client.CommitDelete(new CommitDeleteInput() { id = res, streamId = SpT.BCHP.streamId }));

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
	public IEnumerator Operations_Receive_Run() => UniTask.ToCoroutine(async () =>
	{
		var client = new GameObject().AddComponent<Receiver>();

		await UniTask.Yield();

		await client.Initialize(AccountManager.GetDefaultAccount(), SpT.SIMPLE.streamId);

		Assert.IsNotNull(client.converter);
		Assert.IsNotNull(client.stream.id == SpT.SIMPLE.streamId);
		Assert.IsTrue(client.branches.Valid());
		Assert.IsNotNull(client.branch);
		Assert.IsTrue(client.commits.Valid().Equals(client.commit != null));

		Assert.IsTrue(client.IsValid());

		var args = (ReceiveWorkArgs)await client.Run();

		Assert.IsNotNull(args);
		Assert.IsTrue(args.success);
		Assert.IsTrue(args.client.Equals(client));
		Assert.IsTrue(!string.IsNullOrEmpty(args.message));
		Assert.IsTrue(!string.IsNullOrEmpty(args.referenceObj));
	});

	[UnityTest, Category(SpT.C_OPS)]
	public IEnumerator Operations_Send_Run() => UniTask.ToCoroutine(async () =>
	{
		var client = new GameObject().AddComponent<Sender>();

		await UniTask.Yield();

		await client.Initialize(AccountManager.GetDefaultAccount(), SpT.SIMPLE.streamId);

		Assert.IsNotNull(client.converter);
		Assert.IsNotNull(client.stream.id == SpT.SIMPLE.streamId);
		Assert.IsTrue(client.branches.Valid());
		Assert.IsNotNull(client.branch);
		Assert.IsTrue(client.commits.Valid().Equals(client.commit != null));

		Assert.IsTrue(client.IsValid());

		// Send using only Base object
		var @base = new Base
		{
			["Prop"] = 10,
			["Child"] = new Base()
		};

		var args = (SendWorkArgs)await client.Run(@base);

		Assert.IsNotNull(args);
		Assert.IsTrue(args.success);
		Assert.IsTrue(args.client.Equals(client));
		Assert.IsTrue(!string.IsNullOrEmpty(args.message));
		Assert.IsTrue(!string.IsNullOrEmpty(args.commitId));
		Assert.IsTrue(!string.IsNullOrEmpty(args.url));

		Assert.IsTrue(await _client.CommitDelete(new CommitDeleteInput() { streamId = client.stream.id, id = args.commitId }));

		// Send using speckle node
		var node = new GameObject("Speckle Node").AddComponent<SpeckleNode>();
		var layer = new GameObject("Speckle Layer").AddComponent<SpeckleLayer>();
		var baseProp = new GameObject("Base").AddComponent<BaseBehaviour>();
		baseProp.Store(@base);
		layer.Add(baseProp.gameObject);
		node.AddLayer(layer);

		args = (SendWorkArgs)await client.Run(node);
		
		Assert.IsNotNull(args);
		Assert.IsTrue(args.success);
		Assert.IsTrue(args.client.Equals(client));
		Assert.IsTrue(!string.IsNullOrEmpty(args.message));
		Assert.IsTrue(!string.IsNullOrEmpty(args.commitId));
		Assert.IsTrue(!string.IsNullOrEmpty(args.url));
		
		Assert.IsTrue(await _client.CommitDelete(new CommitDeleteInput() { streamId = client.stream.id, id = args.commitId }));
	});

}

[TestFixture]
public class Units
{
	[Test, Category(SpT.C_CLIENT)]
	public void Client_IsValid()
	{
		// valid test
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		Assert.IsNotNull(client);
		Assert.IsTrue(client.IsValid());

		client.Dispose();

		// invalid test
		client = new SpeckleUnityClient(null);
		Assert.IsNotNull(client);
		Assert.IsFalse(client.IsValid());
	}

	[Test, Category(SpT.C_CLIENT)]
	public void Client_Account_ByDefault()
	{
		// valid test
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		Assert.IsNotNull(client);
		Assert.IsTrue(client.account.Equals(AccountManager.GetDefaultAccount()));

		client.Dispose();

		// invalid test
		client = new SpeckleUnityClient(null);
		Assert.IsNotNull(client);
		Assert.IsNull(client.account);
	}

	[Test, Category(SpT.C_CLIENT)]
	public void Client_Account_ByName()
	{
		var account = SpeckleUnity.GetAccountByName("David Morgan");
		// valid test
		var client = new SpeckleUnityClient(account);
		Assert.IsNotNull(client);
		Assert.IsTrue(client.account.id.Equals(account.id));
		Assert.IsTrue(client.account.Equals(account));

		client.Dispose();

		account = SpeckleUnity.GetAccountByName("No Name here!");
		// invalid test
		client = new SpeckleUnityClient(account);
		Assert.IsNotNull(client);
		Assert.IsNull(client.account);
	}

	[UnityTest, Category(SpT.C_CLIENT)]
	public IEnumerator Client_StreamGet() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var stream = await client.StreamGet(SpT.BCHP.streamId);
		Assert.IsNotNull(stream);
		Assert.IsTrue(stream.id.Equals(SpT.BCHP.streamId));

		// test for wrapper
		var wrapper = new StreamAdapter(stream);
		Assert.IsTrue(wrapper.Equals(stream));
		Assert.IsTrue(wrapper.branches.Valid());

		Assert.IsNull(wrapper.commit);
		Assert.IsNull(wrapper.branch);
		Assert.IsFalse(wrapper.commits.Valid());
		Assert.IsTrue(wrapper.@object == stream.@object);
	});

	[UnityTest, Category(SpT.C_CLIENT)]
	public IEnumerator Client_BranchGet() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var res = await client.BranchGet(SpT.BCHP.streamId, SpT.BCHP.branchName);

		Assert.IsNotNull(res);
	});

	[UnityTest, Category(SpT.C_CLIENT)]
	public IEnumerator Client_BranchesGet() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var res = await client.BranchesGet(SpT.BCHP.streamId);

		Assert.IsTrue(res.Valid());
	});

	[UnityTest, Category(SpT.C_CLIENT)]
	public IEnumerator Client_CommitGet() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var res = await client.CommitGet(SpT.BCHP.streamId, SpT.BCHP.commitId);

		Assert.IsNotNull(res);
		Assert.IsTrue(res.id.Equals(SpT.BCHP.commitId));
	});

	[UnityTest, Category(SpT.C_CLIENT)]
	public IEnumerator Client_ObjectGet() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var res = await client.ObjectGet(SpT.BCHP.streamId, SpT.BCHP.objectId);

		Assert.IsNotNull(res);
		Assert.IsTrue(res.id.Equals(SpT.BCHP.objectId));
	});

	[UnityTest, Category(SpT.C_STREAM)]
	public IEnumerator Stream_LoadObject() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var wrapper = new StreamAdapter(await client.StreamGet(SpT.BCHP.streamId));

		Assert.IsNotNull(wrapper);
		Assert.IsTrue(wrapper.id.Equals(SpT.BCHP.streamId));

		Assert.IsTrue(await wrapper.LoadObject(client, SpT.BCHP.objectId));
		Assert.IsTrue(wrapper.@object.id.Equals(SpT.BCHP.objectId));
	});

	[UnityTest, Category(SpT.C_STREAM)]
	public IEnumerator Stream_LoadCommit() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var wrapper = new StreamAdapter(await client.StreamGet(SpT.BCHP.streamId));

		Assert.IsNotNull(wrapper);
		Assert.IsTrue(await wrapper.LoadCommit(client, SpT.BCHP.commitId));
		Assert.IsNotNull(wrapper.commit);
		Assert.IsTrue(wrapper.commit.id.Equals(SpT.BCHP.commitId));
	});

	[UnityTest, Category(SpT.C_STREAM)]
	public IEnumerator Stream_LoadCommits() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var wrapper = new StreamAdapter(await client.StreamGet(SpT.BCHP.streamId));

		Assert.IsNotNull(wrapper);
		Assert.IsTrue(await wrapper.LoadCommits(client));
		Assert.IsNotEmpty(wrapper.commits);
		Assert.IsNull(wrapper.commit);
	});

	[UnityTest, Category(SpT.C_STREAM)]
	public IEnumerator Stream_LoadBranch() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var wrapper = new StreamAdapter(await client.StreamGet(SpT.BCHP.streamId));

		Assert.IsNotNull(wrapper);
		Assert.IsTrue(await wrapper.LoadBranch(client, SpT.BCHP.branchName));
		Assert.IsNotNull(wrapper.branch);
		Assert.IsTrue(wrapper.branch.name.Equals(SpT.BCHP.branchName));
		Assert.IsTrue(wrapper.commits.Valid());
		Assert.IsNull(wrapper.commit);
	});

	[UnityTest, Category(SpT.C_STREAM)]
	public IEnumerator Stream_LoadBranches() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var wrapper = new StreamAdapter(await client.StreamGet(SpT.BCHP.streamId));

		Assert.IsNotNull(wrapper);
		Assert.IsTrue(wrapper.id.Equals(SpT.BCHP.streamId));

		Assert.IsTrue(await wrapper.LoadBranches(client));
		Assert.IsTrue(wrapper.branches.Valid());

		Assert.IsNull(wrapper.commit);
		Assert.IsNull(wrapper.branch);
		Assert.IsFalse(wrapper.commits.Valid());
	});

	[UnityTest, Category(SpT.C_STREAM)]
	public IEnumerator Stream_LoadActivity() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var wrapper = new StreamAdapter(await client.StreamGet(SpT.BCHP.streamId));

		Assert.IsNotNull(wrapper);
		Assert.IsNull(wrapper.activity);

		Assert.IsTrue(await wrapper.LoadActivity(client));
		Assert.IsNotNull(wrapper.activity);
		Assert.IsNotEmpty(wrapper.activity.items);
	});

	[UnityTest, Category(SpT.C_STREAM)]
	public IEnumerator Stream_LoadTypes() => UniTask.ToCoroutine(async () =>
	{
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var wrapper = new StreamAdapter(await client.StreamGet(SpT.BCHP.streamId));

		Assert.IsNotNull(wrapper);
		Assert.IsTrue(wrapper.type == StreamWrapperType.Stream);

		Assert.IsTrue(await wrapper.LoadBranch(client, SpT.BCHP.branchName));
		Assert.IsTrue(wrapper.type == StreamWrapperType.Branch);

		Assert.IsTrue(await wrapper.LoadCommit(client, SpT.BCHP.commitId));
		Assert.IsTrue(wrapper.type == StreamWrapperType.Commit);
	});

}