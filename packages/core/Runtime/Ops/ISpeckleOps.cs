using System;
using System.Collections.Concurrent;
using System.Threading;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{
	public interface ISpeckleOps
	{
		public SpeckleUnityClient client { get; }

		public CancellationToken token { get; }

		public void Init(Account account);

		public void LoadStream(Stream stream);

		public Stream GetStream();

	}

	public interface IHaveProgress
	{
		public float progress { get; }
	}

	public interface IOperationEvents : IHaveProgress
	{
		public event UnityAction<ConcurrentDictionary<string, int>> OnProgressAction;

		public event UnityAction<string, Exception> OnErrorAction;

		public event UnityAction<int> OnTotalChildCountAction;
	}

}