using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Models;
using Speckle.Core.Transports;

namespace Speckle.ConnectorUnity.Ops
{
	public static class SpeckleOps
	{

		/// <summary>
		/// A unity friendly version of the Receive method from <see cref="Operations"/>
		/// </summary>
		/// <param name="client">The client to use for interacting with the stream</param>
		/// <param name="streamId">Stream ID to use</param>
		/// <param name="referenceObj">Id of the object to receive</param>
		/// <param name="onProgress">Optional action to report progress values</param>
		/// <param name="onError">Optional action to notify about errors</param>
		/// <param name="onChildCount">Optional action to get the total child count of the found object</param>
		/// <returns></returns>
		public static async UniTask<Base> Receive(
			SpeckleUnityClient client,
			string streamId,
			string referenceObj,
			Action<ConcurrentDictionary<string, int>> onProgress = null,
			Action<string, Exception> onError = null,
			Action<int> onChildCount = null
		)
		{
			if (client == null || !client.IsValid())
			{
				SpeckleUnity.Console.Log("Account is not valid");
				return null;
			}

			Base @base = null;
			var watch = Stopwatch.StartNew();

			var transport = new ServerTransport(client.account, streamId);

			try
			{
				// only use Task with any client calls to speckle. Not worth the conversion 
				await Task.Run(async () =>
				{
					SpeckleUnity.Console.Log($"Getting Commit\nstream id:{streamId} object id:{referenceObj}");

					SpeckleUnity.Console.Log($"Now Receiving\n{watch.Elapsed}");

					@base = await Operations.Receive(objectId: referenceObj,
					                                 cancellationToken: client.token,
					                                 remoteTransport: transport,
					                                 onProgressAction: onProgress,
					                                 onErrorAction: onError,
					                                 onTotalChildrenCountKnown: onChildCount);

					SpeckleUnity.Console.Log($"Object Recieved:{@base}\nTotal time:{watch.Elapsed}");
				}, client.token);
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				// clean up 
				transport.Dispose();

				// report
				watch.Stop();
				SpeckleUnity.Console.Log($"Command Complete\n{watch.Elapsed}");
			}

			await UniTask.Yield();

			return @base;
		}
	}
}