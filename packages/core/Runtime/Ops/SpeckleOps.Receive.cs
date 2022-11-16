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
	public static partial class SpeckleOps
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
		/// <returns>The unconverted base object</returns>
		public static async UniTask<Base> Receive(
			SpeckleUnityClient client,
			string streamId,
			string referenceObj,
			Action<ConcurrentDictionary<string, int>> onProgress = null,
			Action<string, Exception> onError = null,
			Action<int> onChildCount = null
		)
		{
			var watch = Stopwatch.StartNew();
			return await Receive(client, streamId, referenceObj, watch, onProgress, onError, onChildCount);
		}

		/// <summary>
		/// A unity friendly version of the Receive method from <see cref="Operations"/>
		/// </summary>
		/// <param name="client">The client to use for interacting with the stream</param>
		/// <param name="streamId">Stream ID to use</param>
		/// <param name="referenceObj">Id of the object to receive</param>
		/// <param name="watch">A stopwatch for monitor time things</param>
		/// <param name="onProgress">Optional action to report progress values</param>
		/// <param name="onError">Optional action to notify about errors</param>
		/// <param name="onChildCount">Optional action to get the total child count of the found object</param>
		/// <returns>The unconverted base object</returns>
		public static async UniTask<Base> Receive(
			SpeckleUnityClient client,
			string streamId,
			string referenceObj,
			Stopwatch watch,
			Action<ConcurrentDictionary<string, int>> onProgress = null,
			Action<string, Exception> onError = null,
			Action<int> onChildCount = null
		)
		{
			Base @base = null;
			watch ??= Stopwatch.StartNew();

			SpeckleUnity.Console.Log($"{nameof(Receive)} intializing!");

			if (client == null || !client.IsValid())
			{
				SpeckleUnity.Console.Error($"{nameof(Receive)} cannot compelte due to an invalid {typeof(Client)}");

				watch.Stop();
				return null;
			}

			if (!streamId.Valid() || !referenceObj.Valid())
			{
				SpeckleUnity.Console.Error(
					$"{nameof(Receive)} cannot compelte due to an invalid input."
					+ $"\nstream id :{(streamId.Valid() ? streamId : "invalid")}"
					+ $"\nobject id :{(referenceObj.Valid() ? referenceObj : "invalid")}");

				watch.Stop();
				return null;
			}

			// NOTE: could a unity transporter help with object creation? 
			var transport = new ServerTransport(client.Account, streamId);

			SpeckleUnity.Console.Log($"Starting recieve!\nstream id:{streamId} object id:{referenceObj}\n{watch.Elapsed}");

			try
			{
				// NOTE: this is probably where a unity based transporter could be handy. For now this works just fine

				@base = await UniTask.Create(async () =>
				{
					await UniTask.SwitchToThreadPool();

					var b = await Operations.Receive(objectId: referenceObj,
					                                 cancellationToken: client.token,
					                                 remoteTransport: transport,
					                                 onProgressAction: onProgress,
					                                 onErrorAction: onError,
					                                 onTotalChildrenCountKnown: onChildCount);

					await UniTask.SwitchToMainThread();

					return b;
				});
				

				// only use Task with any client calls to speckle. Not worth the conversion 
				// await Task.Run(async () =>
				// {
				// 	@base = await Operations.Receive(objectId: referenceObj,
				// 	                                 cancellationToken: client.token,
				// 	                                 remoteTransport: transport,
				// 	                                 onProgressAction: onProgress,
				// 	                                 onErrorAction: onError,
				// 	                                 onTotalChildrenCountKnown: onChildCount);
				//
				// 	await UniTask.Yield();
				// 	SpeckleUnity.Console.Log($"Object Recieved:{@base}\nTotal time:{watch.Elapsed}");
				// }, client.token);
			}
			catch (Exception e)
			{
				SpeckleUnity.Console.Warn(e.Message);
			}
			finally
			{
				transport.Dispose();

				watch.Stop();
				SpeckleUnity.Console.Log($"{nameof(Receive)} command complete!\n{watch.Elapsed}");
				
				SpeckleUnity.Console.Log($"Object Recieved:{@base}\nTotal time:{watch.Elapsed}");
				
				await UniTask.Yield();

			}
			return @base;
		}
	}

}