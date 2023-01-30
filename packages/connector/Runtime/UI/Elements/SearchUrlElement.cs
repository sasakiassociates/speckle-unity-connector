using Speckle.ConnectorUnity.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{

  public class SearchUrlElement : VisualElement
  {

    public new class UxmlTraits : VisualElement.UxmlTraits
    { }

    public new class UxmlFactory : UxmlFactory<SearchUrlElement, UxmlTraits>
    { }

    string ElementClass => SpeckleUss.Classes.Control.SEARCH_URL;
    string StyleSheetName => "speckle-controls";

    string _cachedInputText;

    public UnityAction<string> onSearch;

    public SearchUrlElement()
    {
      AddToClassList(ElementClass);
      AddToClassList(SpeckleUss.Classes.Containers.ROWS);

    #if UNITY_EDITOR
      this.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(GUIHelper.Folders.CONNECTOR_USS + StyleSheetName + ".uss"));
    #endif


      var inputField = new TextField();
      inputField.RegisterValueChangedCallback(evt => _cachedInputText = evt.newValue);
      this.Add(inputField);

      var searchButton = SpeckleUss.Prefabs.buttonWithIcon;
      searchButton.name = SpeckleUss.Names.SEARCH;
      searchButton.clickable.clicked += Clicked;
      this.Add(searchButton);

    }

    void Clicked()
    {
      Debug.Log("Clicked Search Button");
      onSearch?.Invoke(_cachedInputText);
    }


  }

}
