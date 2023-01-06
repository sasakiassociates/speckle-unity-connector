using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.UI
{

  public static class SpeckleUss
  {
    public const string MAIN = "speckle";
    const string ELEM = "__";
    const string MOD = "--";
    const string EXT = "-";

    public static class Classes
    {
      public const string CONTAINER = MAIN + "-container";
      public const string MODEL = MAIN + "-model";
      public const string CONTROL = MAIN + "-control";

      public class Containers
      {
        const string NS = CONTAINER + ELEM;

        public const string ROWS = NS + "rows";
        public const string COLUMNS = NS + "columns";
        public const string STREAM_INFO = NS + "stream" + EXT + "info";
        public const string CONTROLS = NS + "controls";
      }

      public static class Models
      {
        const string NS = MODEL + ELEM;

        public const string STREAM = NS + "stream";
        public const string STREAM_LIST_ITEM = STREAM + EXT + "list" + EXT + "item";
        public const string ACCOUNT = NS + "account";
        public const string OBJECT = NS + "object";
      }

      public static class Control
      {

        const string NS = CONTROL + ELEM;

        public const string BUTTON = NS + "button";
        public const string BUTTON_ICON = BUTTON + EXT + "icon";

        public const string TOGGLE = NS + "toggle";
        public const string PROGRESSBAR = NS + "progressbar";
        public const string LIST = NS + "list";

        public const string REFRESH = NS + "refresh";
        public const string SEND = NS + "sender";
        public const string RECEIVE = NS + "receiver";
        public const string OPEN_NEW = NS + "openInNew";
        public const string SELECT_ACCOUNT = NS + "selectAccount";
      }

      public static class Elements
      {
        public const string NAMESPACE = MAIN + "-element";
        const string SPACE = NAMESPACE + ELEM;

        public const string LABEL = SPACE + "label";


        public static class Texture
        {
          const string NS = SPACE + "texture" + EXT;

          public const string PREVIEW = NS + "preview";
          public const string ICON = NS + "icon";
          public const string IMAGE = NS + "image";
          public const string AVATAR = NS + "avatar";
        }

        public static class Text
        {
          const string NS = SPACE + "text" + EXT;

          public const string TITLE = NS + "title";
          public const string SUBTITLE = NS + "subtitle";
          public const string BODY = NS + "body";

          public const string HEADER1 = NS + "header1";
          public const string HEADER2 = NS + "header2";
          public const string HEADER3 = NS + "header3";
          public const string HEADER4 = NS + "header4";
        }

      }


    }

    public static class Names
    {
      public const string OPEN_IN_WEB_BUTTON = "openInNew";
      public const string INFO = "info";
      public const string CONTROLS = "controls";
      public const string REFRESH = "refresh";
      public const string SEND = "sender";
      public const string RECEIVE = "receiver";
    }


    public static class Prefabs
    {


      public static Button iconButton
      {
        get
        {
          var item = new Button();
          item.AddToClassList(Classes.Control.BUTTON);
          item.AddToClassList(Classes.Elements.Texture.ICON);
          return item;
        }
      }

      public static Button buttonWithIcon
      {
        get
        {
          var item = new Button();
          item.AddToClassList(Classes.Control.BUTTON_ICON);
          item.Add(emptyIcon);
          return item;
        }
      }

      public static VisualElement emptyIcon
      {
        get
        {
          var item = new VisualElement();
          item.AddToClassList(Classes.Elements.Texture.ICON);
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
