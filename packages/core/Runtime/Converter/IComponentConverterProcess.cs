using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Converter
{

  public interface IComponentConverterProcess
  {
    public event UnityAction<int> OnQueueSizeChanged;

  }

}
