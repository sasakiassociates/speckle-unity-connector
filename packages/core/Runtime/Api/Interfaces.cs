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

		public Account Account { get; }

		public Stream Stream { get; }

		public UniTask Initialize(Account account, string streamId);

		public event UnityAction OnClientRefresh;

	}

	public interface ISpeckleOpsEvent : IHaveProgress
	{
		public event UnityAction<ConcurrentDictionary<string, int>> OnProgressAction;

		public event UnityAction<string, Exception> OnErrorAction;

		public event UnityAction<int> OnTotalChildCountAction;
	}

}