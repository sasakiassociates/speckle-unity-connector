using Objects.BuiltElements;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = nameof(View3DComponentConverter), menuName = SpeckleUnity.Categories.CONVERTERS + "Create View3d Converter")]
	public class View3DComponentConverter : ComponentConverter<View3D, Camera>
	{

		/// <summary>
		///   Converts a Speckle View3D to a GameObject
		/// </summary>
		/// <param name="base"></param>
		/// <param name="instance"></param>
		protected override void ConvertBase(View3D @base, ref Camera instance)
		{
			instance.transform.position = ConverterUtils.VectorByCoordinates(
				@base.origin.x, @base.origin.y, @base.origin.z, @base.origin.units);

			instance.transform.forward = ConverterUtils.VectorByCoordinates(
				@base.forwardDirection.x, @base.forwardDirection.y, @base.forwardDirection.z, @base.forwardDirection.units);

			instance.transform.up = ConverterUtils.VectorByCoordinates(
				@base.upDirection.x, @base.upDirection.y, @base.upDirection.z, @base.upDirection.units);
		}

		protected override Base ConvertComponent(Camera component) => new View3D
		{
			origin = component.transform.position.ToPoint(),
			forwardDirection = component.transform.forward.ToSpeckle(),
			upDirection = component.transform.up.ToSpeckle(),
			isOrthogonal = component.orthographic
		};
	}
}