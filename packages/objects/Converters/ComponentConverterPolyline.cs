using Objects.Geometry;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = nameof(ComponentConverterPolyline), menuName = "Speckle/Converters/Create Polyline Converter")]
	public class ComponentConverterPolyline : ComponentConverter<Polyline, LineRenderer>
	{
		public float diameter;

		/// <summary>
		///   Converts a Speckle curve to a GameObject with a line renderer
		/// </summary>
		/// <param name="base"></param>
		/// <returns></returns>
		protected override GameObject ConvertBase(Polyline @base)
		{
			var line = BuildGo(@base.speckle_type);

			line.SetupLineRenderer(@base.GetPoints().ArrayToPoints(@base.units), diameter);

			return line.gameObject;
		}

		protected override Base ConvertComponent(LineRenderer component)
		{
			// TODO: check if this should use world or local scale
			var points = new Vector3[component.positionCount];
			component.GetPositions(points);

			return new Polyline(points.ToSpeckle());
		}
	}
}