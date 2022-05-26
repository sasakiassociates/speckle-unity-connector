using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.GUI
{
  public static class ElementHelper
  {

    public static int DropDownChange(this DropdownField field, ChangeEvent<string> evt, Action<int> notify = null)
    {
      var index = -1;
      for (int i = 0; i < field.choices.Count; i++)
      {
        if (field.choices[i].Equals(evt.newValue))
        {
          index = i;
          break;
        }
      }

      if (index >= 0)
        notify?.Invoke(index);

      return index;
    }

    public static DropdownField SetDropDown(this VisualElement root, string fieldName, int index, IEnumerable<string> items, Action<ChangeEvent<string>> callback)
    {
      var dropDown = root.Q<VisualElement>(fieldName + "-container").Q<DropdownField>("items");
      dropDown.choices = items.ToList();
      dropDown.index = index;
      dropDown.RegisterValueChangedCallback(callback.Invoke);
      return dropDown;
    }
  }

}