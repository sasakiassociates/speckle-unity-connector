using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.GUI;
using Speckle.ConnectorUnity.Ops;
using UnityEditor;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity
{
	[CustomEditor(typeof(Sender))]
	public class SenderEditor : SpeckleClientEditor<Sender>
	{

		private TextField message;
		
		protected override string treePath => GUIHelper.Dir + "Sender.uxml";
		
		
		protected override void OnRunClicked()
		{
			if (!obj.isWorking)
				obj.Send().Forget();
		}
	}
}