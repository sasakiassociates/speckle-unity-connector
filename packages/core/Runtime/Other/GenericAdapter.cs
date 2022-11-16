namespace Speckle.ConnectorUnity.Ops
{
  public interface ICanAdapt<TObj>
  {
    public TObj source { get; set; }

  }

  public abstract class GenericAdapter<TObj>
  {

    TObj _source;

    public GenericAdapter(TObj value)
    {
      _source = value;
    }

    public virtual TObj source
    {
      get => _source ?? Get();
      protected set => _source = value;
    }

    protected abstract TObj Get();

  }
}
