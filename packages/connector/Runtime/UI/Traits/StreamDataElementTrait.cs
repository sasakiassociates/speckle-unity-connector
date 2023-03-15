using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{

  public class StreamDataElementTrait : StreamElementTrait
  {

    UxmlBoolAttributeDescription _mShowCommits = new UxmlBoolAttributeDescription {name = "show-commits", defaultValue = true};
    UxmlBoolAttributeDescription _mShowBranches = new UxmlBoolAttributeDescription {name = "show-branches", defaultValue = true};


    public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
    {
      base.Init(ve, bag, cc);

      var el = ve as StreamDataElement;

      el.ShowCommits = _mShowCommits.GetValueFromBag(bag, cc);
      el.ShowBranches = _mShowBranches.GetValueFromBag(bag, cc);
    }
  }

}
