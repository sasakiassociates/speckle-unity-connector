using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Objects.Geometry;
using Speckle.ConnectorUnity;
using Speckle.ConnectorUnity.Converter.PlayArea;
using Speckle.ConnectorUnity.Mono;
using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.TestTools;
using Mesh = Objects.Geometry.Mesh;

internal static class SpT
{
	public const string C_CLIENT = "Client";
	public const string C_STREAM = "Stream";
	public const string C_OPS = "Operations";
	public const string C_OBJ = "Objects";

}

[TestFixture]
public class Integrations
{

	async UniTask<Mesh> GetMesh()
	{
		const string stream  = "4777dea055";
		const string id  = "d611a3e8bf64984c50147e3f9238c925";
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());
		var obj = await client.ObjectGet(stream, id);
		return(Objects.Geometry.Mesh)await SpeckleOps.Receive(client, stream, obj.id);
	}

	[UnityTest, Category(SpT.C_OPS)]
	public IEnumerator Convert_Async() => UniTask.ToCoroutine(async () =>
	{
		const string streamId = "4777dea055";
		const string objectId = "d611a3e8bf64984c50147e3f9238c925";
		var client = new SpeckleUnityClient(AccountManager.GetDefaultAccount());

		var obj = await client.ObjectGet(streamId, objectId);
		var @base = (Objects.Geometry.Mesh)await SpeckleOps.Receive(client, streamId, obj.id);

		Assert.IsNotNull(@base);

		var converter = new ConverterAsync();

		converter.Step1_SetupOnThread(@base.id);

		var filter = await converter.Step2_RunJob(@base);

		Assert.IsNotNull(filter);
		Assert.IsTrue(filter.mesh.name.Contains(@base.id));
		Assert.IsTrue(filter.mesh.subMeshCount == 1);
	});

	[UnityTest, Category(SpT.C_OBJ)]
	public IEnumerator Object_Serialize() => UniTask.ToCoroutine(async () =>
	{

		var point = new Point(10, 01, 10);
		var mesh =await GetMesh();
		mesh["SomeInt"] = 25;
		mesh["SomeString"] = "hellooooo";
		mesh["SomeObject"] = point;

		// test how the conversion process can be broken down into parts
		// important to know what needs to be single threaded and what can be thrown into multi-threads
		
		// part 1 - create hierarchy from objects and props
		var props = new SpeckleProperties();
		
		await props.Test_Serialize(mesh);

		Assert.IsNotNull(props.Data);
		Assert.IsNotEmpty(props.Data);

		Assert.IsTrue(props.Data["SomeString"].Equals("hellooooo"));
		Assert.IsTrue(props.Data["SomeObject"].Equals(point));

		props.Data["SomeInt"] = 10;
		Assert.IsTrue(props.Data["SomeInt"].Equals(10));

		// part 2 - store object data in speckle props
		// part 3 - convert all objects
	});

}