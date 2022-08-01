using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Speckle.ConnectorUnity.Converter.PlayArea;
using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Credentials;
using UnityEngine;
using UnityEngine.TestTools;
using Mesh = Objects.Geometry.Mesh;

internal static class SpT
{
	public const string C_CLIENT = "Client";
	public const string C_STREAM = "Stream";
	public const string C_OPS = "Operations";

}

[TestFixture]
public class Integrations
{

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

}