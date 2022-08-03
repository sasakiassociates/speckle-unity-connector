using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Mono
{
	/// <summary>
	///   A simple version of the object Base from Speckle that contains the speckle properties type
	/// </summary>
	[AddComponentMenu(SpeckleUnity.NAMESPACE + "/Base")]
	public class BaseBehaviour_v1 : MonoBehaviour
	{

		[SerializeField] [HideInInspector]
		protected SpeckleProperties _properties;
		

		protected virtual HashSet<string> excludedProps
		{
			get
			{
				return new HashSet<string>(
					typeof(Base).GetProperties(
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
					).Select(x => x.Name));
			}
		}

		public SpeckleProperties properties
		{
			get => _properties;
			set => _properties = value;
		}

		public virtual void SetProps(Base @base, HashSet<string> props = null)
		{
			_properties = new SpeckleProperties();
			_properties.Store(@base, props ?? excludedProps);
		}

	}
}