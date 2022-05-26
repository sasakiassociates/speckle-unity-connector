using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.GUI
{
  public class SpeckleStreamElement : VisualElement
  {
    public DropdownField branches, commits;

    public SpeckleStreamElement()
    {
      branches = new DropdownField("Branches");
      Add(branches);
      commits = new DropdownField("Commits");
      Add(commits);
    }
  }
}