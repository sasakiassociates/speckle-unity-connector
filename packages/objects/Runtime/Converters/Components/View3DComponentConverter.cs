using Objects.BuiltElements;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = nameof(View3DComponentConverter), menuName = SpeckleUnity.Categories.CONVERTERS + "Create View3d Converter")]
	public class View3DComponentConverter : ComponentConverter<View3D, Camera>
	{

		/// <summary>
		///  Converts a Speckle View3D to a GameObject
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="instance"></param>
		protected override void ConvertBase(View3D obj, ref Camera instance)
		{
			instance.transform.position = ConverterUtils.VectorByCoordinates(
				obj.origin.x, obj.origin.y, obj.origin.z, obj.origin.units);

			instance.transform.forward = ConverterUtils.VectorByCoordinates(
				obj.forwardDirection.x, obj.forwardDirection.y, obj.forwardDirection.z, obj.forwardDirection.units);

			instance.transform.up = ConverterUtils.VectorByCoordinates(
				obj.upDirection.x, obj.upDirection.y, obj.upDirection.z, obj.upDirection.units);
		}

		public override Base ConvertComponent(Camera component) => new View3D
		{
			origin = component.transform.position.ToPoint(),
			forwardDirection = component.transform.forward.ToSpeckle(),
			upDirection = component.transform.up.ToSpeckle(),
			isOrthogonal = component.orthographic
		};
	}
}