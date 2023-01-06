using Cysharp.Threading.Tasks;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Ops
{

  public abstract class ClientBehaviour : MonoBehaviour, IClient
  {

    [SerializeField, HideInInspector] protected SpeckleAccount account;
    [SerializeField, HideInInspector] protected SpeckleUnityClient client;

    CancellationTokenSource sourceToken;

    public event UnityAction OnAccountSet;

    public Account Account => account?.source;

    public Client Client => client?.source;

    /// <summary>
    /// A Token tied to this game object
    /// </summary>
    public CancellationToken Token
    {
      get
      {
        sourceToken ??= new CancellationTokenSource();
        return sourceToken.Token;
      }
    }

    protected virtual void OnEnable()
    {
      sourceToken = new CancellationTokenSource();
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

        if(obj == null)
        {
          SpeckleUnity.Console.Warn($"Invalid Account being passed into {name}");
        }
        else
        {
          Debug.Log($"Account selected {obj}");
          account = new SpeckleAccount(obj);
          client = new SpeckleUnityClient(obj);
          client.token = new CancellationToken();
          sourceToken = CancellationTokenSource.CreateLinkedTokenSource(client.token);
        }
      }
      catch(SpeckleException e)
      {
        SpeckleUnity.Console.Warn(e.Message);
      }
      finally
      {
        OnAccountSet?.Invoke();
      }

      return UniTask.CompletedTask;

    }


    /// <summary>
    ///   Clean up to any client things
    /// </summary>
    public void Cancel()
    {
      client?.Dispose();

      if(sourceToken != null && sourceToken.IsCancellationRequested)
      {
        sourceToken.Cancel();
      }

      sourceToken?.Dispose();
    }


    /// <summary>
    /// Returns true if <see cref="Client"/> is valid
    /// </summary>
    /// <returns></returns>
    public virtual bool IsValid() => client != null && client.IsValid();

  }

}
