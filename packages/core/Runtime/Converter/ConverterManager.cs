using Speckle.ConnectorUnity.Converter;
using System.Collections.Generic;
using UnityEngine;


namespace Speckle.ConnectorUnity
{

  public class ConverterManager : ScriptableObject
  {
    public List<ScriptableConverter> activeConverters;
    
    public ConverterManager Instance { get; private set; }


    void OnEnable()
    {
      Instance = this;
    }



    public static List<ScriptableConverter> GetAllConverters()
    {
      var items = new List<ScriptableConverter>();

      return items;
    }

    public static TType GetConverter<TType>() where TType : ScriptableConverter
    {
      return null;
    }

  }

}
