using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

  public interface IShouldValidate
  {
    public bool IsValid();
  }

  public interface IHaveProgress
  {
    public float Progress { get; }
  }

  public interface IClientInstance :
    IShouldValidate
  {

    // public SpeckleAccount account { get; }

    // public SpeckleClient client { get; }

    public Account baseAccount { get; }

    public Client baseClient { get; }

    public CancellationToken token { get; }

    public UniTask Initialize(Account obj);

    // public UniTask Initialize(SpeckleAccount obj);

    public void Cancel();

    public event UnityAction OnInitialize;

  }

  public interface IStream : IClientInstance
  {

    public Stream Stream { get; }

    public UniTask LoadStream(string streamId);

    public event UnityAction OnStreamSet;

  }

  public interface IOps : IStream
  {
    public UniTask DoWork();
  }

  public interface IOpsWork<TArgs>
  {

    public event UnityAction<TArgs> OnClientUpdate;
  }

  public interface IOpsEvent : IHaveProgress
  {
    public event UnityAction<ConcurrentDictionary<string, int>> OnProgressAction;

    public event UnityAction<string, Exception> OnErrorAction;

    public event UnityAction<int> OnTotalChildCountAction;
  }

  public interface ISpeckleConnector
  {
    public UniTask CreateSender();

    public UniTask CreateReceiver();

    public void SetSelectedStream(int index);

    public event UnityAction OnSenderAdded;

    public event UnityAction OnReceiverAdded;
  }

}
