namespace Speckle.ConnectorUnity.Converters.Commands
{
	public struct SpeckleMeshGenerator
	{
		public SpeckleMeshGenerator(int vertexCount, int indexCount)
		{
			VertexCount = vertexCount;
			IndexCount = indexCount;
			JobLength = 1;
			
		}

		public void Execute(int index, MeshStream stream)
		{
			throw new System.NotImplementedException();
		}

		public int VertexCount { get; private set; }

		public int IndexCount { get; private set; }

		public int JobLength { get; private set; }

		// public UnityEngine.Bounds Bounds { get; set; }

	}
}