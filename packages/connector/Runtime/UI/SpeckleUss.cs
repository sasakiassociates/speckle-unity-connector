using System;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.UI
{

  public static class SpeckleUss
  {
    public const string EXT = "speckle";

    public static class Classes
    {
      public const string CONTAINER = EXT + "-container";
      public const string ELEMENT = EXT + "-element";
      public const string MODEL = EXT + "-model";
      public const string CONTROL = EXT + "-control";



      public class Containers
      {
        public const string ROWS = CONTAINER + "-rows";
        public const string COLUMNS = CONTAINER + "-columns";
      }

      public static class Models
      {
        public const string STREAM = MODEL + "-stream";
        public const string ACCOUNT = MODEL + "-account";
        public const string OBJECT = MODEL + "-object";

      }

      public static class Elements
      {

        public const string BUTTON = ELEMENT + "-button";
        public const string LABEL = ELEMENT + "-label";
        public const string TOGGLE = ELEMENT + "-toggle";
        public const string PROGRESSBAR = ELEMENT + "-progressbar";
        public const string ICON = ELEMENT + "-icon";
        public const string IMAGE = ELEMENT + "-image";
        public const string AVATAR = ELEMENT + "-avatar";

        public const string LIST = ELEMENT + "-list";

        public const string TEXTURE = ELEMENT + "-texture";

        public static class Texture
        {
          public const string PREVIEW = TEXTURE + "-preview";
        }

        public const string TEXT = ELEMENT + "-text";

        public static class Text
        {
          public const string TITLE = TEXT + "-title";
          public const string SUBTITLE = TEXT + "-subtitle";
          public const string BODY = TEXT + "-body";

          public const string HEADER1 = TEXT + "-header1";
          public const string HEADER2 = TEXT + "-header2";
          public const string HEADER3 = TEXT + "-header3";
          public const string HEADER4 = TEXT + "-header4";
        }

      }

      public static class Control
      {

        public const string REFRESH = CONTROL + "-refresh";
        public const string CHANGE_ACCOUNT = CONTROL + "-changeAccount";
        public const string SELECT_ACCOUNT = CONTROL + "-selectAccount";
      }


    }

    public static class Names
    {
      public const string OPEN_IN_WEB_BUTTON = "openInWeb";
      public const string INFO = "info";
      public const string CONTROLS = "controls";
      public const string REFRESH = "refresh";
      public const string CHANGE_ACCOUNT = "changeAccount";
      public const string SELECT_ACCOUNT = "selectAccount";
    }


    public static class Prefabs
    {

      public static Button buttonWithIcon
      {
        get
        {
          var item = new Button()
          {
            name = Classes.Elements.BUTTON,
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
          item.AddToClassList(Classes.Elements.ICON);
          return item;
        }
      }


      public static VisualElement containerColumn
      {
        get
        {
          var element = new VisualElement() {style = {flexDirection = FlexDirection.Column}};
          element.AddToClassList(Classes.Containers.COLUMNS);
          return element;
        }
      }

      public static VisualElement containerRow
      {
        get
        {
          var element = new VisualElement() {style = {flexDirection = FlexDirection.Row}};
          element.AddToClassList(Classes.Containers.ROWS);
          return element;
        }
      }

      public static VisualElement subTitle
      {
        get
        {
          var element = new VisualElement()
          {
            name = Classes.Elements.Text.SUBTITLE,
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
