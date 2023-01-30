using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Speckle.ConnectorUnity.Models
{

  public class SpeckleLayer : MonoBehaviour
  {


    [SerializeField] Transform parent;

    [SerializeField] List<GameObject> data = new List<GameObject>();

    [SerializeField] List<SpeckleLayer> layers;

    /// <summary>
    ///   Active parent for all layer objects
    /// </summary>
    public Transform Parent
    {
      get => parent;
      set => parent = value;
    }

    /// <summary>
    ///   Converted object data within layer
    /// </summary>
    public List<GameObject> Data
    {
      get => data.Valid() ? data : new List<GameObject>();
    }

    /// <summary>
    ///   Layer Name
    /// </summary>
    public string LayerName
    {
      get => name;
      set => name = value;
    }

    /// <summary>
    ///   Nested Layers
    /// </summary>
    public List<SpeckleLayer> Layers
    {
      get => layers.Valid() ? layers : new List<SpeckleLayer>();
    }

    /// <summary>
    ///   Set parent for all objects in a layer
    /// </summary>
    /// <param name="t"></param>
    /// <param name="recursive"></param>
    public void ParentObjects(Transform t, bool recursive = false)
    {
      if(t == null)
      {
        return;
      }

      parent = t;

      if(Data.Any())
      {
        Data.ForEach(x => x.transform.SetParent(parent));
      }

      if(!recursive)
      {
        return;
      }

      foreach(var l in layers)
      {
        l.ParentObjects(t, true);
      }
    }

    /// <summary>
    /// Sets the <see cref="Parent"/> object to the object hosted by this component
    /// </summary>
    public void ParentObjects()
    {
      ParentObjects(parent != null ? parent : transform);
    }

    public void Add(SpeckleLayer layer)
    {
      layers ??= new List<SpeckleLayer>();
      layers.Add(layer);
    }

    public void Add(GameObject go)
    {
      data ??= new List<GameObject>();
      data.Add(go);
      go.transform.SetParent(parent);
    }

    void Awake() => parent = this.transform;

  }

}
