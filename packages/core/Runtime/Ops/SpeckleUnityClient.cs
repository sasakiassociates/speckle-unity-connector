using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

	public interface ISpeckleUnityClient : ISpeckleProgress
	{

		public Account account { get; }

		public Client source { get; }

		public CancellationToken token { get; }

	}
	

	[Serializable]
	public class SpeckleUnityClient : ISpeckleUnityClient, IDisposable
	{

		AccountWrapper _accountWrapper;

		public Account account
		{
			get => _accountWrapper?.source;
		}

		public CancellationToken token { get; private set; }

		public Client source { get; private set; }

		public event Action<ConcurrentDictionary<string, int>> OnProgressAction;

		public event Action<string, Exception> OnErrorAction;

		public event UnityAction<int> OnTotalChildCountAction;

		public SpeckleUnityClient(Account obj)
		{
			if (obj == null) return;

			_accountWrapper = new AccountWrapper(obj);
			source = new Client(account);
		}

		public void Dispose()
		{
			source?.Dispose();
		}

		/// <summary>
		/// Loads a new speckle stream by fetching the stream with a valid url
		/// </summary>
		/// <param name="url"></param>
		public static async UniTask<Stream> LoadStreamByUrl(string url)
		{
			Stream res = null;
			SpeckleUnityClient c = null;

			try
			{
				var s = new StreamWrapper(url);

				if (s.IsValid)
				{
					c = new SpeckleUnityClient(await s.GetAccount());
					res = await c.StreamGet(url);
				}
			}

			catch (SpeckleException e)
			{
				SpeckleUnity.Console.Warn(e.Message);
				c?.Dispose();
			}

			return res;
		}
	}

}