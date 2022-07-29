namespace Speckle.ConnectorUnity.Ops
{
	public interface IShouldValidate
	{
		public bool IsValid();
	}

	public interface IHaveProgress
	{
		public float progress { get; }
	}
}