using Objects.BuiltElements;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = nameof(ComponentConverterView3D), menuName = "Speckle/Converters/Create View3d Converter")]
	public class ComponentConverterView3D : ComponentConverter<View3D, Camera>
	{

		/// <summary>
		///   Converts a Speckle View3D to a GameObject
		/// </summary>
		/// <param name="base"></param>
		/// <returns></returns>
		protected override GameObject ConvertBase(View3D @base)
		{
			var comp = BuildGo(@base.name);

			comp.transform.position = ConverterUtils.VectorByCoordinates(
				@base.origin.x, @base.origin.y, @base.origin.z, @base.origin.units);

			comp.transform.forward = ConverterUtils.VectorByCoordinates(
				@base.forwardDirection.x, @base.forwardDirection.y, @base.forwardDirection.z, @base.forwardDirection.units);

			comp.transform.up = ConverterUtils.VectorByCoordinates(
				@base.upDirection.x, @base.upDirection.y, @base.upDirection.z, @base.upDirection.units);

			return comp.gameObject;
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