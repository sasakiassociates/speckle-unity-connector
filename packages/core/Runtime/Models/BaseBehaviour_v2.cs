using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Mono;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Models
{
	[AddComponentMenu(SpeckleUnity.NAMESPACE + "/Base")]
	public class BaseBehaviour_v2 : MonoBehaviour, IBase, ISerializationCallbackReceiver
	{
		
		public SpeckleProperties props { get; }

		public object this[string key]
		{
			get => throw new System.NotImplementedException();
			set => throw new System.NotImplementedException();
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

		public UniTask Store(Base @base)
		{
			throw new System.NotImplementedException();
		}

		public void OnBeforeSerialize()
		{
			throw new System.NotImplementedException();
		}

		public void OnAfterDeserialize()
		{
			throw new System.NotImplementedException();
		}
	}
}