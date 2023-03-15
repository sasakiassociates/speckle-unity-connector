namespace Speckle.ConnectorUnity.Ops
{
  public interface ICanAdapt<TObj>
  {
    public TObj Source { get; set; }
    

  }

  public abstract class GenericAdapter<TObj>
  {

    TObj _source;

    public GenericAdapter(TObj value)
    {
      _source = value;
    }

    public virtual TObj Source
    {
      get => _source ?? Get();
      protected set => _source = value;
    }

    protected abstract TObj Get();

  }
}
