using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Serialisation;
using Speckle.Core.Transports;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Speckle.ConnectorUnity
{

	public class OperationTests : MonoBehaviour
	{

		static TestStream RC_1 = new()
		{
			stream = "89ef27f57b",
			commit = "884906295a"
		};

		static TestStream RC_2 = new()
		{
			stream = "89ef27f57b",
			commit = "162ee5f3f5"
		};

		static TestStream Carlos = new()
		{
			stream = "00613d79b2",
			commit = "a852ab79fb"
		};

		bool isWorking;

		public void OnGUI()
		{
			if (GUI.Button(new Rect(10f, 40f, 100f, 20f), "Speckle Test") && !isWorking)
			{
				Debug.Log($"Processor Count {Environment.ProcessorCount}");
				TaskPool.SetMaxPoolSize(3);
				isWorking = true;
				// UniTask.WhenAll(ChainedCommand(Carlos));
				ChainedCommand(Carlos).Forget();
			}

			if (GUI.Button(new Rect(10f, 70f, 100f, 20f), "Empty Test") && !isWorking)
			{
				TaskPool.SetMaxPoolSize(Environment.ProcessorCount);
				isWorking = true;
				Receive().Forget();
			}
		}

		async UniTask ChainedCommand(TestStream stream)
		{
			Debug.Log("Start");
			isWorking = true;
			var watch = Stopwatch.StartNew();

			var token = this.GetCancellationTokenOnDestroy();

			var client = new Client(AccountManager.GetDefaultAccount());

			var commit = await GetCommit(client, stream, token);

			Debug.Log("Completed Commit-" + watch.Elapsed);

			await SpeckleGet(commit.referencedObject, token, new ServerTransport(AccountManager.GetDefaultAccount(), stream.stream));

			// await GetObject(client, commit.referencedObject, stream, token);

			client.Dispose();

			isWorking = false;
			Debug.Log("Object Recieved-" + watch.Elapsed);
		}

		async UniTask<Commit> GetCommit(Client client, TestStream s, CancellationToken token)
		{
			var commit = await client.CommitGet(token, s.stream, s.commit).AsUniTask();

			await UniTask.Yield();

			return commit;
		}

		async UniTask<Base> GetObject(Client client, string refObj, TestStream s, CancellationToken token)
		{
			var transport = new ServerTransport(client.Account, s.stream);

			var obj = await Operations.Receive(refObj, token, transport).AsUniTask();
			await UniTask.Yield();
			transport.Dispose();
			return obj;
		}

		public async UniTask Receive()
		{
			Debug.Log("Empty Call Started");

			var watch = Stopwatch.StartNew();

			await UniTask.Delay(1000);

			watch.Stop();

			Debug.Log("Delay Done" + watch.Elapsed);

			await UniTask.Yield();
		}

		public static async UniTask<Base> SpeckleGet(string objectId, CancellationToken token, ServerTransport remoteTransport)
		{
			var serializerV2 = new BaseObjectDeserializerV2();

			var localTransport = new SQLiteTransport();

			serializerV2.ReadTransport = localTransport;
			serializerV2.CancellationToken = token;

			var objString = localTransport.GetObject(objectId);
			if (objString != null)
			{
				Base localRes = null;

				Debug.Log("Found local object");

				try
				{
					localRes = serializerV2.Deserialize(objString);
					return localRes;
				}
				catch (Exception e)
				{
					if (serializerV2.OnErrorAction == null) throw;

					serializerV2.OnErrorAction.Invoke($"A deserialization error has occurred: {e.Message}", new SpeckleException(
						                                  $"A deserialization error has occurred: {e.Message}", e));
					localRes = null;
				}
			}

			Debug.Log("No Local, looking for Remote");
			objString = await remoteTransport.CopyObjectAndChildren(objectId, localTransport).AsUniTask();
			Base res = null;

			try
			{
				res = serializerV2.Deserialize(objString);
			}
			catch (Exception e)
			{
				if (serializerV2.OnErrorAction == null) throw;

				serializerV2.OnErrorAction.Invoke($"A deserialization error has occurred: {e.Message}", e);
				res = null;
			}

			remoteTransport.Dispose();

			return res;
		}

		struct TestStream
		{
			public string stream;

			public string commit;
		}
	}
}