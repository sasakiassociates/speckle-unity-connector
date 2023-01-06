using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
using Speckle.ConnectorUnity.Ops;
using Speckle.Core.Credentials;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Speckle.ConnectorUnity
{

  [AddComponentMenu("Speckle/Speckle Connector")]
  [ExecuteAlways]
  public class SpeckleConnector : ClientBehaviour, ISpeckleConnector
  {

    [SerializeField] SpeckleStream selectedStream;

    [field: SerializeField] public List<SpeckleStream> streams { get; private set; } = new List<SpeckleStream>();

    [field: SerializeField] public List<SpeckleAccount> accounts { get; private set; } = new List<SpeckleAccount>();

    public event UnityAction OnSenderAdded;

    public event UnityAction OnReceiverAdded;

    public event UnityAction OnStreamsLoaded;

    public static SpeckleConnector instance { get; set; }

    public static IEnumerable<SpeckleAccount> GetAccounts()
    {
      return AccountManager.GetAccounts().Where(x => x != null).Select(x => new SpeckleAccount(x));
    }

    public void SetSelectedStream(int index)
    {
      if(streams.Valid(index))
      {
        SpeckleUnity.Console.Log($"index value{index} for is out of range from stream list");
        return;
      }
      selectedStream = streams[index];

    }

    public async UniTask CreateSender()
    {
      var ops = await Create<Sender, SendWorkArgs>();

      if(ops == null)
      {
        SpeckleUnity.Console.Log($"{typeof(Sender)} was not created from {nameof(SpeckleConnector)}-{name}");
        return;
      }
      OnSenderAdded?.Invoke();
    }

    public async UniTask CreateReceiver()
    {
      var ops = await Create<Receiver, ReceiveWorkArgs>();

      if(ops == null)
      {
        SpeckleUnity.Console.Log($"{typeof(Receiver)} was not created from {nameof(SpeckleConnector)}-{name}");
        return;
      }
      OnSenderAdded?.Invoke();
    }

    public void OpenStreamInBrowser(EventBase obj)
    {
      UniTask.Create(async () =>
      {
        // copied from desktop ui
        await UniTask.Delay(100);

        // Application.OpenURL(activeStream.GetUrl(false));
      });
    }

    protected override void OnEnable()
    {
      base.OnEnable();

      instance = this;

      OnInitialize += LoadStreams;


      if(!accounts.Valid())
      {
        accounts = GetAccounts().ToList();
      }

      if(!account.Valid())
      {
        Initialize();
      }
      else if(!streams.Valid())
      {
        LoadStreams();
      }

    }

    protected override void OnDisable()
    {
      OnInitialize -= LoadStreams;
    }

    protected override void ClearData()
    {
      streams = new List<SpeckleStream>();
    }

    void LoadStreams()
    {
      Debug.Log("Loading streams");

      UniTask.Create(async () =>
      {
        var accountStreams = await client.StreamsGet();
        streams = new List<SpeckleStream>();

        foreach(var stream in accountStreams)
        {
          streams.Add(new SpeckleStream(stream));
        }

        OnStreamsLoaded?.Invoke();

      });

    }

    async UniTask<TOperator> Create<TOperator, TArgs>()
      where TArgs : OpsWorkArgs
      where TOperator : OpsBehaviour<TArgs>
    {

      if(!IsValid() || selectedStream == null || !selectedStream.IsValid())
      {
        SpeckleUnity.Console.Log("No Active stream ready to be sent to Receiver");
        return null;
      }

      var ops = new GameObject().AddComponent<TOperator>();

      ops.Initialize(Account);
      await ops.LoadStream(selectedStream.Id);

    #if UNITY_EDITOR
      Selection.activeObject = ops;
    #endif

      return ops;
    }
  }



}
