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
  public interface IShouldValidate
  {
    public bool IsValid();
  }

  public interface IHaveProgress
  {
    public float Progress { get; }
  }

  public interface IClient :
    IShouldValidate
  {

    public Account Account { get; }

    public Client Client { get; }

    public CancellationToken Token { get; }

    public UniTask Initialize(Account obj);

    public void Cancel();

    public event UnityAction OnAccountSet;

  }

  public interface IStream : IClient
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
