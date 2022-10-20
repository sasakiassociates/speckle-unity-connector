using System;
using Speckle.Core.Api;

namespace Speckle.ConnectorUnity.Models
{
	[Serializable]
	internal class StreamUpdateInputWrapper
	{
		public string name;
		public string description;
		public bool isPublic;

		public StreamUpdateInput Get(string id) => new StreamUpdateInput
		{
			id = id,
			name = this.name,
			description = this.description,
			isPublic = this.isPublic
		};
	}
}