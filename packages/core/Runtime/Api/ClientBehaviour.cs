using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

  public abstract class ClientBehaviour : MonoBehaviour, IClientInstance
  {

    [SerializeField, HideInInspector] protected SpeckleAccount account;
    [SerializeField, HideInInspector] protected SpeckleClient client;

    CancellationTokenSource _sourceToken;

    public event Action OnInitialize;

    public Account BaseAccount => account?.Source;

    public Client BaseClient => client?.Source;

    /// <summary>
    /// A Token tied to this game object
    /// </summary>
    public CancellationToken Token
    {
      get
      {
        _sourceToken ??= new CancellationTokenSource();
        return _sourceToken.Token;
      }
    }

    protected virtual void OnEnable()
    {
      _sourceToken = new CancellationTokenSource();
    }

    protected virtual void OnDisable() => Cancel();

    protected virtual void OnDestroy() => Cancel();

    /// <summary>
    /// Initialize with the default Speckle Account
    /// </summary>
    /// <returns></returns>
    public UniTask Initialize()
    {
      return Initialize(SpeckleAccountManager.GetDefaultAccount());
    }

    public virtual UniTask Initialize(Account obj)
    {
      Debug.Log($"Starting {nameof(Initialize)}");
      try
      {
        Cancel();
        ClearData();

        if(obj == null)
        {
          SpeckleUnity.Console.Warn($"Invalid Account being passed into {name}");
        }
        else
        {
          Debug.Log($"Account selected {obj}");
          account = new SpeckleAccount(obj);
          client = new SpeckleClient(obj);
          client.token = new CancellationToken();
          _sourceToken = CancellationTokenSource.CreateLinkedTokenSource(client.token);
        }
      }
      catch(SpeckleException e)
      {
        SpeckleUnity.Console.Warn(e.Message);
      }
      finally
      {
        OnInitialize?.Invoke();
      }

      return UniTask.CompletedTask;

    }

    /// <summary>
    ///  Method for objects to use when <see cref="Initialize()"/> is called 
    /// </summary>
    protected virtual void ClearData()
    { }

    /// <summary>
    ///   Clean up to any client things
    /// </summary>
    public void Cancel()
    {
      client?.Dispose();

      if(_sourceToken != null && _sourceToken.IsCancellationRequested)
      {
        _sourceToken.Cancel();
      }

      _sourceToken?.Dispose();
    }


    /// <summary>
    /// Returns true if <see cref="BaseClient"/> is valid
    /// </summary>
    /// <returns></returns>
    public virtual bool IsValid() => client != null && client.IsValid();

  }

}
