using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Models
{
	
	[AddComponentMenu(SpeckleUnity.NAMESPACE + "/Base")]
	public class BaseBehaviour : MonoBehaviour, IBase, ISerializationCallbackReceiver
	{
		[SerializeField, HideInInspector] SpeckleProperties _props;
		[SerializeField, HideInInspector] bool _hasChanged;

		public event UnityAction OnPropsChanged;

		public string speckle_type { get; protected set; }

		public string applicationId { get; protected set; }

		public long totalChildCount { get; protected set; }

		public string id { get; protected set; }

		public SpeckleProperties props
		{
			get => _props;
			protected set
			{
				if (value == null) return;

				_props = value;
				_props.OnCollectionChange += _ =>
				{
					_hasChanged = true;
					OnPropsChanged?.Invoke();
				};
			}
		}

		public virtual HashSet<string> excluded
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
				if (props.Data.ContainsKey(key))
					return props.Data[key];

				var prop = GetType().GetProperty(key);

				return prop == null ? null : prop.GetValue(this);
			}
			set
			{
				if (!this.IsPropNameValid(key, out string reason)) SpeckleUnity.Console.Warn("Invalid prop name: " + reason);

				if (props.Data.ContainsKey(key))
				{
					props.Data[key] = value;
					return;
				}

				var prop = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(p => p.Name == key);

				if (prop == null)
				{
					props.Data[key] = value;
					return;
				}

				try
				{
					prop.SetValue(this, value);
				}
				catch (Exception ex)
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
			if (!_hasChanged) return;

			props.Serialize();
			_hasChanged = false;
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
			speckle_type = @base.speckle_type;
			applicationId = @base.applicationId;
			totalChildCount = @base.totalChildrenCount;
		}

		/// <summary>
		/// Method for modifying how
		/// </summary>
		protected virtual void HandleTypeProps(Base @base)
		{
			props = new SpeckleProperties();
			props.Serialize(@base);
		}

	}
}