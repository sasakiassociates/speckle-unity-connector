using System;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.UI
{

  public static class SpeckleUss
  {
    public const string ext = "speckle";

    public static class classes
    {
      public const string container = ext + "-container";
      public const string element = ext + "-element";
      public const string model = ext + "-model";

      public class containers
      {
        public const string stacked = container + "-stacked";
      }

      public static class models
      {
        public const string stream = model + "-stream";
      }

      public static class elements
      {

        public const string button = element + "-button";
        public const string icon = element + "-icon";
        public const string text = element + "-text";
        public const string title = text + "-title";
        public const string subtitle = text + "-subtitle";
        public const string body = text + "-body";
      }
    }

    public static class names
    {
      public const string openInWebButton = "openInWeb";
      public const string info = "info";
      public const string controls = "controls";
    }


    public static class prefabs
    {

      public static Button buttonWithIcon
      {
        get
        {
          var item = new Button()
          {
            name = classes.elements.button,
            style = {flexGrow = 1, flexShrink = 1}
          };
          item.Add(emptyIcon);
          return item;
        }
      }

      public static VisualElement emptyIcon
      {
        get
        {
          var item = new VisualElement() {style = {minHeight = 20, minWidth = 20}};
          item.AddToClassList(classes.elements.icon);
          return item;
        }
      }

      public static VisualElement subTitle
      {
        get
        {
          var element = new VisualElement()
          {
            name = classes.elements.subtitle,
            style =
            {
              flexDirection = FlexDirection.Row,
              paddingLeft = 0, paddingRight = 0, paddingTop = 0, paddingBottom = 0,
              marginLeft = 0, marginRight = 0, marginTop = 0, marginBottom = 0
            }
          };
          return element;
        }
      }



    }

  }

}
