namespace Speckle.ConnectorUnity.Ops
{
	public abstract class GenericWrapper<TObj>
	{

		TObj _source;

		public GenericWrapper(TObj obj)
		{
			_source = obj;
		}

		public virtual TObj source
		{
			get => _source ?? Get();
			protected set => _source = value;
		}

		protected abstract TObj Get();

	}
}