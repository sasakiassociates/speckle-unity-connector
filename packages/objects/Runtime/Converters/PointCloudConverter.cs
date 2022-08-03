using System;
using Objects.Geometry;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = nameof(PointCloudConverter), menuName = SpeckleUnity.Categories.CONVERTERS + "Create PointCloud Converter")]
	public class PointCloudConverter : ComponentConverter<Pointcloud, ParticleSystem>
	{

		protected override void ConvertBase(Pointcloud @base, ref ParticleSystem instance)
		{ }

		protected override Base ConvertComponent(ParticleSystem component) => null;
	}

}