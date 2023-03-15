using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.Core.Models;

namespace Speckle.ConnectorUnity.Models
{

	public interface IBase : IBaseDynamic
	{

		public UniTask Store(Base @base);

		public string ID { get; }

		public string SpeckleType { get; }

		public string ApplicationId { get; }

		public long TotalChildCount { get; }

	}
	public interface IBaseDynamic
	{
		public HashSet<string> Excluded { get; }

		SpeckleProperties Props { get; }

		#region copy pasta from speckle core models

		public object this[string key] { get; set; }
		#endregion

	}
}