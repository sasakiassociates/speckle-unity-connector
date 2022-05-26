using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.GUI
{

	public class StreamPreview : VisualElement
	{

		public Image thumbnail;

		public StreamPreview()
		{
			thumbnail = new Image()
			{
				style =
				{
					flexGrow = 1
				}
			};

			Add(thumbnail);

			thumbnail.AddToClassList("previewIcon");
			AddToClassList("previewContainer");
		}

		#region UXML
		[Preserve]
		public new class UxmlFactory : UxmlFactory<StreamPreview, UxmlTraits>
		{ }

		[Preserve]
		public new class UxmlTraits : VisualElement.UxmlTraits
		{ }
		#endregion

	}
}