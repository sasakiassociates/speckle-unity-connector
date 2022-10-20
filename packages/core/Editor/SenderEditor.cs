using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Args;
using Speckle.ConnectorUnity.GUI;
using Speckle.ConnectorUnity.Ops;
using UnityEditor;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity
{
	[CustomEditor(typeof(Sender))]
	public class SenderEditor : SpeckleClientEditor<Sender, SendWorkArgs>
	{

		TextField message;

		protected override string treePath
		{
			get => GUIHelper.Folders.GUI + "Sender.uxml";
		}

		protected override void OnRunClicked()
		{
			if (!obj.IsWorking)
				obj.DoWork().Forget();
		}
	}
}