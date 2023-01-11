using Speckle.ConnectorUnity.UI;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{

  public class SpeckleStreamListItem : StreamElement
  {


    public new class UxmlFactory : UxmlFactory<SpeckleStreamListItem, UxmlTraits>
    { }


    /// <summary>
    /// Optional class name to attached to the element 
    /// </summary>
    protected override string elementClass => SpeckleUss.Classes.Models.STREAM_LIST_ITEM;

    /// <summary>
    /// Name of <seealso cref="StyleSheet"/> without the extension type
    /// </summary>
    protected override string styleSheetName => "stream-list-item-card";



    public bool HasControl(Button b)
    {
      if(b == null)
      {
        SpeckleUnity.Console.Warn($"Invalid button value passed for {nameof(HasControl)} action");
        return false;
      }

      var hashToFind = b.GetHashCode();
      foreach(var e in controlsContainer.Children())
      {
        if(e is Button child && child.GetHashCode() == hashToFind)
          return true;
      }

      return false;
    }

    public void AddControl(Button b)
    {
      if(b == null)
      {
        SpeckleUnity.Console.Warn($"Invalid button value passed for {nameof(AddControl)} action");
        return;
      }
      controlsContainer.Insert(0, b);
    }

    public void RemoveControl(Button b)
    {
      if(b == null)
      {
        SpeckleUnity.Console.Warn($"Invalid button value passed for {nameof(RemoveControl)} action");
        return;
      }

      controlsContainer.Remove(b);
    }
  }

}
