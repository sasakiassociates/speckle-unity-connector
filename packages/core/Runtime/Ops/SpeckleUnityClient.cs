using System;
using System.Collections.Concurrent;
using System.Threading;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

	public interface ISpeckleUnityClient : ISpeckleProgress
	{

		public Account account { get; }

		public Client client { get; }

		public CancellationToken token { get; }

		public void Reset();

		public void SetSource(Account obj);

	}

	[Serializable]
	public class SpeckleUnityClient : ISpeckleUnityClient
	{

		AccountWrapper _accountWrapper;

		public Account account
		{
			get => _accountWrapper?.source;
		}

		public CancellationToken token { get; private set; }

		public Client client { get; private set; }

		public event Action<ConcurrentDictionary<string, int>> OnProgressAction;

		public event Action<string, Exception> OnErrorAction;

		public event UnityAction<int> OnTotalChildCountAction;

		public SpeckleUnityClient(Account obj)
		{
			this.SetSource(obj);
		}

		public void SetSource(Account obj)
		{
			_accountWrapper ??= new AccountWrapper();
			_accountWrapper.source = obj;
			Reset();
		}

		public void Destroy()
		{
			client?.Dispose();
			// unsubscribe to all events
		}

		public void Reset()
		{
			Destroy();

			if (account != null)
				client = new Client(account);
		}

	}

}