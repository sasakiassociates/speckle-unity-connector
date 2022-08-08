using Objects.Geometry;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = nameof(PointComponentConverter), menuName = SpeckleUnity.Categories.CONVERTERS + "Create Point Converter")]
	public class PointComponentConverter : ComponentConverter<Point, SpecklePoint>
	{

		protected override void ConvertBase(Point @base, ref SpecklePoint instance) => instance.pos = @base.ToVector3();

		protected override Base ConvertComponent(SpecklePoint component) => component.pos.ToSpeckle();
	}
}