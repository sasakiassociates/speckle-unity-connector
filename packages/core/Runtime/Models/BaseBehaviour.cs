using Cysharp.Threading.Tasks;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Models
{

  [AddComponentMenu(SpeckleUnity.NAMESPACE + "/Base")]
  public class BaseBehaviour : MonoBehaviour, IBase
  {

    [SerializeField, ReadOnly] string speckleType;
    [SerializeField, ReadOnly] string applicationId;
    [SerializeField, ReadOnly] string id;
    [SerializeField, ReadOnly] long totalChildCount;

    [SerializeField] SpeckleProperties props;

    [SerializeField, HideInInspector] bool hasChanged;

    public event UnityAction OnPropsChanged;

    public string ID => id;

    public string SpeckleType => speckleType;

    public string ApplicationId => applicationId;

    public long TotalChildCount => totalChildCount;

    public SpeckleProperties Props
    {
      get => props;
      protected set
      {
        if(value == null) return;

        props = value;
        props.OnCollectionChange += _ =>
        {
          hasChanged = true;
          OnPropsChanged?.Invoke();
        };
      }
    }

    public virtual HashSet<string> Excluded
    {
      get
      {
        return new HashSet<string>(
          typeof(Base).GetProperties(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
          ).Select(x => x.Name));
      }
    }

    public object this[string key]
    {
      get
      {
        if(Props.Data.ContainsKey(key))
          return Props.Data[key];

        var prop = GetType().GetProperty(key);

        return prop == null ? null : prop.GetValue(this);
      }
      set
      {
        if(!this.IsPropNameValid(key, out string reason)) SpeckleUnity.Console.Warn("Invalid prop name: " + reason);

        if(Props.Data.ContainsKey(key))
        {
          Props.Data[key] = value;
          return;
        }

        var prop = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(p => p.Name == key);

        if(prop == null)
        {
          Props.Data[key] = value;
          return;
        }

        try
        {
          prop.SetValue(this, value);
        }
        catch(Exception ex)
        {
          SpeckleUnity.Console.Error(ex.Message);
        }
      }
    }

    public UniTask Store(Base @base)
    {
      HandleBaseProps(@base);
      HandleTypeProps(@base);
      return UniTask.CompletedTask;
    }

    public void OnBeforeSerialize()
    {
      if(!hasChanged) return;

      Props.Serialize();
      hasChanged = false;
    }

    public void OnAfterDeserialize()
    { }

    /// <summary>
    /// Method for populating the standard <see cref="Base"/> Props
    /// </summary>
    /// <param name="base"></param>
    protected virtual void HandleBaseProps(Base @base)
    {
      id = @base.id;
      speckleType = @base.speckle_type;
      applicationId = @base.applicationId;
      totalChildCount = @base.totalChildrenCount;
    }

    /// <summary>
    /// Method for modifying how
    /// </summary>
    protected virtual void HandleTypeProps(Base @base)
    {
      Props = new SpeckleProperties();
      Props.Serialize(@base);
    }

  }

}
