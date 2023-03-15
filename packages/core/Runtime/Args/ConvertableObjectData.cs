using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

  /// <summary>
  /// A simple object for holding speckle data wanting to be lil unity objects
  /// </summary>
  public class ConvertableObjectData
  {
    public Component unityObj;
    public readonly Base speckleObj;

    public ConvertableObjectData(Base speckleObj, Component unityObj)
    {
      this.speckleObj = speckleObj;
      this.unityObj = unityObj;
    }

  }

}
