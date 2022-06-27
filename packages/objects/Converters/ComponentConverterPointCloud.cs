using System;
using Objects.Geometry;
using Speckle.Core.Models;
using UnityEngine;

namespace Speckle.ConnectorUnity.Converter
{
	[CreateAssetMenu(fileName = nameof(ComponentConverterPointCloud), menuName = "Speckle/Converters/Create PointCloud Converter")]
	public class ComponentConverterPointCloud : ComponentConverter<Pointcloud, ParticleSystem>
	{

		protected override GameObject ConvertBase(Pointcloud @base) => throw new NotImplementedException();
		protected override Base ConvertComponent(ParticleSystem component) => throw new NotImplementedException();
	}

}