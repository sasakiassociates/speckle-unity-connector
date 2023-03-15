using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{

  /// <summary>
  ///   
  /// </summary>
  public interface IComponentConverter
  {

    /// <summary>
    /// 
    /// </summary>
    public string SpeckleType { get; }

    /// <summary>
    /// 
    /// </summary>
    public string UnityType { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool CanConvertToNative(Base type);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool CanConvertToSpeckle(Component type);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="base"></param>
    /// <returns></returns>
    public GameObject ToNative(Base @base);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public Base ToSpeckle(Component component);

  }

}
