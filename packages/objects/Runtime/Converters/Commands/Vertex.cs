using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Speckle.ConnectorUnity.Converters.Commands
{
	public struct Vertex
	{
		public float3 position, normal;
		public float4 tangent;
		public float2 texCoord0;
	}


	
	
	[StructLayout(LayoutKind.Sequential)]
	public struct TriangleUInt16
	{
		public ushort a, b, c;
		
		public static implicit operator TriangleUInt16(int3 t) => new TriangleUInt16()
		{
			a = (ushort)t.x,
			b = (ushort)t.y,
			c = (ushort)t.z
		};
	}
}