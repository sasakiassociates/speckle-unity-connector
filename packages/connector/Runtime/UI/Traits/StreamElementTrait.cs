using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{


  public class StreamElementUxmlFactory : UxmlFactory<StreamElement, StreamElementTrait>
  { }

  public class StreamElementTrait : BindableElement.UxmlTraits
  {
    UxmlBoolAttributeDescription _mShowDescription = new UxmlBoolAttributeDescription {name = "show-description", defaultValue = false};
    UxmlBoolAttributeDescription _mShowOpenInNew = new UxmlBoolAttributeDescription {name = "show-open-url", defaultValue = false};
    UxmlBoolAttributeDescription _mShowOperations = new UxmlBoolAttributeDescription {name = "show-operations", defaultValue = false};
    UxmlBoolAttributeDescription _mShowPreview = new UxmlBoolAttributeDescription {name = "show-preview", defaultValue = false};

    public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
    {
      get
      {
        yield return new UxmlChildElementDescription(typeof(StreamElement));
      }
    }

    public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
    {
      base.Init(ve, bag, cc);

      var el = ve as StreamElement;

      el.displayDescription = _mShowDescription.GetValueFromBag(bag, cc);
      el.displayOpenInNew = _mShowOpenInNew.GetValueFromBag(bag, cc);
      el.displayOperations = _mShowOperations.GetValueFromBag(bag, cc);
      el.displayPreview = _mShowPreview.GetValueFromBag(bag, cc);
    }

  }




  public class StreamDataElementTrait : StreamElementTrait
  {

    UxmlBoolAttributeDescription _mShowCommits = new UxmlBoolAttributeDescription {name = "show-commits", defaultValue = true};
    UxmlBoolAttributeDescription _mShowBranches = new UxmlBoolAttributeDescription {name = "show-branches", defaultValue = true};
    UxmlBoolAttributeDescription _mShowConverters = new UxmlBoolAttributeDescription {name = "show-converters", defaultValue = true};


    public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
    {
      base.Init(ve, bag, cc);

      var el = ve as StreamDataElement;

      el.ShowCommits = _mShowCommits.GetValueFromBag(bag, cc);
      el.ShowBranches = _mShowBranches.GetValueFromBag(bag, cc);
      el.ShowConverters = _mShowConverters.GetValueFromBag(bag, cc);
    }
  }

}
