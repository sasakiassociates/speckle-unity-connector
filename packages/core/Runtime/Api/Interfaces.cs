using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Converter;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{
	public interface ISpeckleOps
	{

		public Account account { get; }

		public Stream stream { get; }

		public CancellationToken token { get; }

		public UniTask Initialize(Account account, string streamId);

		public event UnityAction OnClientRefresh;

	}

	public interface IOperationEvents : IHaveProgress
	{
		public event UnityAction<ConcurrentDictionary<string, int>> OnProgressAction;

		public event UnityAction<string, Exception> OnErrorAction;

		public event UnityAction<int> OnTotalChildCountAction;
	}

	public interface IConvert
	{
		/// <summary>
		///   the active converter for this client object
		/// </summary>
		ScriptableSpeckleConverter converter { get; }
	}

}