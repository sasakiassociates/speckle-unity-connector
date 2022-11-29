using UnityEditor;
using UnityEngine;

namespace Speckle.ConnectorUnity
{

  public static class ConnectorCommands
  {
    [MenuItem(SpeckleUnity.NAMESPACE + "Create Scene Connector", false, 100)]
    public static void CreateSpeckleConnectorObject()
    {
      var item = Object.FindObjectOfType<SpeckleConnector>();
      
      if (item == null)
      {
        item = new GameObject("Speckle Connector").AddComponent<SpeckleConnector>();
      }

      Selection.activeObject = item;
    }


  }

}
