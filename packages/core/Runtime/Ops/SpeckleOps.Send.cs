using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Transports;

namespace Speckle.ConnectorUnity.Ops
{
	public static partial class SpeckleOps
	{
		/// <summary>
		/// A unity friendly version of the Send method from <see cref="Operations"/>
		/// </summary>
		/// <param name="data">The <see cref="Base"/> object to send</param>
		/// <param name="client">The client to use for interacting with the stream</param>
		/// <param name="streamId">Stream ID to use</param>
		/// <param name="onProgress">Optional action to report progress values</param>
		/// <param name="onError">Optional action to notify about errors</param>
		/// <param name="useDefaultCache">Optional flag for referencing cached graphs</param>
		/// <returns>The object id of the sent object</returns>
		public static async UniTask<string> Send(
			SpeckleUnityClient client,
			Base data,
			string streamId,
			Action<ConcurrentDictionary<string, int>> onProgress = null,
			Action<string, Exception> onError = null,
			bool useDefaultCache = true
		)
		{
			var objectId = "";
			var watch = Stopwatch.StartNew();

			SpeckleUnity.Console.Log($"{nameof(Send)} start!");

			if (client == null || !client.IsValid())
			{
				SpeckleUnity.Console.Error($"{nameof(Receive)} cannot compelte due to an invalid {typeof(Client)}");

				watch.Stop();
				return objectId;
			}

			if (!streamId.Valid())
			{
				SpeckleUnity.Console.Error(
					$"{nameof(Send)} cannot compelte due to an invalid input\nstream id :{(streamId.Valid() ? streamId : "invalid")}");

				watch.Stop();
				return objectId;
			}

			if (data == null)
			{
				SpeckleUnity.Console.Error(
					$"{nameof(Send)} cannot compelte due to invalid data being sent");

				watch.Stop();
				return objectId;
			}

			var transport = new ServerTransport(client.account, streamId);

			try
			{
				await Task.Run(async () =>
				{
					objectId = await Operations.Send(@object: data,
					                                 cancellationToken: client.token,
					                                 transports: new List<ITransport>() { transport },
					                                 onProgressAction: onProgress,
					                                 onErrorAction: onError,
					                                 useDefaultCache: useDefaultCache,
					                                 disposeTransports: false
					);

					SpeckleUnity.Console.Log($"Object has been sent: {objectId}\nTotal time:{watch.Elapsed}");
				}, client.token);
			}
			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Exception(e);
			}
			finally
			{
				transport.Dispose();

				watch.Stop();
				SpeckleUnity.Console.Log($"{nameof(Send)} command complete!\n{watch.Elapsed}");

				await UniTask.Yield();
				
			}

			return objectId;
		}
	}
}