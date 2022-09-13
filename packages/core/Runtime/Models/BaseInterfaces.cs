using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Speckle.Core.Models;

namespace Speckle.ConnectorUnity.Models
{

	public interface IBase : IBaseDynamic
	{

		public UniTask Store(Base @base);

		public string id { get; }

		public string speckle_type { get; }

		public string applicationId { get; }

		public long totalChildCount { get; }

	}
	public interface IBaseDynamic
	{
		public HashSet<string> excluded { get; }

		SpeckleProperties props { get; }

		#region copy pasta from speckle core models

		public object this[string key] { get; set; }
		#endregion

	}
}