using Objects.Geometry;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = nameof(ComponentConverterPoint), menuName = "Speckle/Converters/Create Point Converter")]
	public class ComponentConverterPoint : ComponentConverter<Point, SpecklePoint>
	{

		protected override GameObject ConvertBase(Point @base)
		{
			var ptn = BuildGo(@base.speckle_type);
			ptn.pos = @base.ToVector3();
			return ptn.gameObject;
		}

		protected override Base ConvertComponent(SpecklePoint component) => component.pos.ToSpeckle();
	}
}