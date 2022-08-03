using System.Linq;
using Objects.Geometry;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = nameof(PolylineConverter), menuName = SpeckleUnity.Categories.CONVERTERS + "Create Polyline Converter")]
	public class PolylineConverter : ComponentConverter<Polyline, LineRenderer>
	{
		public float diameter;

		/// <summary>
		///   Converts a Speckle curve to a GameObject with a line renderer
		/// </summary>
		/// <param name="base"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		protected override void ConvertBase(Polyline @base, ref LineRenderer instance) =>
			instance.SetupLineRenderer(@base.GetPoints().ArrayToVector3(@base.units).ToArray(), diameter);

		protected override Base ConvertComponent(LineRenderer component)
		{
			// TODO: check if this should use world or local scale
			var points = new Vector3[component.positionCount];
			component.GetPositions(points);

			return new Polyline(points.ToSpeckle());
		}
	}
}