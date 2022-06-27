using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Models;
using Speckle.Core.Transports;

namespace Speckle.ConnectorUnity.Ops
{
	public static class Commands
	{
		public static async UniTask<Base> GetCommitData(this ISpeckleInstance instance)
		{
			if (instance == null)
			{
				SpeckleUnity.Console.Log("Account is not valid");
				return null;
			}

			var @base = await GetCommitData(instance.stream, instance.client, instance.token);

			SpeckleUnity.Console.Log($"Data with {@base.totalChildrenCount}");

			return @base;
		}

		public static async UniTaskVoid CommitReceived(this ISpeckleInstance instance, string message = null)
		{
			SpeckleUnity.Console.Log($"Posting a received commit: {instance.stream}");
			await instance.client.CommitReceived(instance.token, new CommitReceivedInput
			{
				streamId = instance.stream.Id,
				commitId = instance.commit.id,
				message = message.Valid() ? message : $"received commit from {SpeckleUnity.HostApp} ",
				sourceApplication = SpeckleUnity.HostApp
			});
		}

		public static async UniTask<Base> GetCommitData(SpeckleStream stream, Client client, CancellationToken token)
		{
			if (stream == null || !stream.IsValid())
			{
				SpeckleUnity.Console.Log("Stream is not valid");
				return null;
			}

			if (client == null)
			{
				SpeckleUnity.Console.Log("Account is not valid");
				return null;
			}

			Base @base = null;

			SpeckleUnity.Console.Log("Start");

			var watch = Stopwatch.StartNew();

			var transport = new ServerTransport(client.Account, stream.Id);

			try
			{
				// only use Task with any client calls to speckle. Not worth the conversion 
				await Task.Run(async () =>
				{
					SpeckleUnity.Console.Log($"Getting Commit\nstream id:{stream.Id} commit id:{stream.CommitId}");

					var commit = await client.CommitGet(token, stream.Id, stream.CommitId);

					SpeckleUnity.Console.Log($"Commit Fetch:{commit.referencedObject}\n{watch.Elapsed}");

					SpeckleUnity.Console.Log($"Now Receiving...\n{watch.Elapsed}");

					@base = await Operations.Receive(commit.referencedObject, token, transport);

					SpeckleUnity.Console.Log($"Object Recieved:{@base}");

					SpeckleUnity.Console.Log("Total time:" + watch.Elapsed);

					return @base;
				}, token);
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Log(e.Message);
			}
			finally
			{
				// clean up 
				transport.Dispose();

				// report
				watch.Stop();
				SpeckleUnity.Console.Log($"Command Complete\n{watch.Elapsed}");
			}

			return @base;
		}
	}

}