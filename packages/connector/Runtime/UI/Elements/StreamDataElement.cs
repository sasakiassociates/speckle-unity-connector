using Speckle.ConnectorUnity.UI;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{



  public class StreamDataElement : StreamElement
  {

    public new class UxmlFactory : UxmlFactory<StreamDataElement, StreamDataElementTrait>
    { }

    DropdownField _branchesDrop;
    DropdownField _commitsDrop;
    DropdownField _convertersDrop;

    /// <summary>
    /// Optional class name to attached to the element 
    /// </summary>
    protected override string elementClass => SpeckleUss.Classes.Models.STREAM_DATA;

    /// <summary>
    /// Name of <seealso cref="StyleSheet"/> without the extension type
    /// </summary>
    protected override string styleSheetName => "stream-data-card";


    protected override void ConstructControls()
    {
      base.ConstructControls();
      var container = SpeckleUss.Prefabs.containerRow;
      container.name = SpeckleUss.Classes.CONTAINER + "__data";


      _branchesDrop = new DropdownField {name = "branches"};
      _branchesDrop.AddToClassList(SpeckleUss.Classes.Control.DROPDOWN);

      _commitsDrop = new DropdownField {name = "commits"};
      _commitsDrop.AddToClassList(SpeckleUss.Classes.Control.DROPDOWN);

      container.Add(_branchesDrop);
      container.Add(_commitsDrop);

      this.Add(container);
    }


    protected override void ConstructFooting()
    {
      base.ConstructFooting();

      _convertersDrop = new DropdownField {name = "converters"};
      _convertersDrop.AddToClassList(SpeckleUss.Classes.Control.DROPDOWN);
      Add(_convertersDrop);
    }

    public bool ShowConverters
    {
      get;
      set;
    }

    public bool ShowBranches
    {

      get;
      set;
    }

    public bool ShowCommits
    {
      get;
      set;
    }

  }

}
