using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Mono;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Speckle.ConnectorUnity.Models
{
	[AddComponentMenu(SpeckleUnity.NAMESPACE + "/Base")]
	public class BaseBehaviour_v2 : MonoBehaviour, IBase, ISerializationCallbackReceiver
	{

		[SerializeField, HideInInspector] SpeckleProperties _props;
		[SerializeField, HideInInspector] bool _hasChanged;

		public SpeckleProperties props
		{
			get => _props;
			protected set
			{
				if (value == null) return;

				_props = value;
				_props.OnCollectionChange += (_) =>
				{
					_hasChanged = true;
					OnPropsChanged?.Invoke();
				};
			}
		}

		public event UnityAction OnPropsChanged;

		public string speckle_type { get; protected set; }

		public string applicationId { get; protected set; }

		public long totalChildCount { get; protected set; }

		public string id { get; protected set; }

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

		public virtual UniTask Store(Base @base)
		{
			id = @base.id;
			speckle_type = @base.speckle_type;
			applicationId = @base.applicationId;
			totalChildCount = @base.totalChildrenCount;

			props = new SpeckleProperties();
			props.SimpleStore(@base);

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
	}
}