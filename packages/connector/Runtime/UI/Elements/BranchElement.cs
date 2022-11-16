using Speckle.ConnectorUnity.Ops;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity.Elements
{
	public class BranchElement : BindableElement, INotifyValueChanged<SpeckleBranch>
	{
		public new class UxmlTraits : BindableElement.UxmlTraits
		{ }

		public new class UxmlFactory : UxmlFactory<BranchElement, UxmlTraits>
		{ }

		public const string ussClassName = "branch-element";

		Label m_Title;
		SpeckleBranch m_Value;

		public BranchElement()
		{
			AddToClassList(ussClassName);
			m_Title = new Label();
			Add(m_Title);
		}

		void OnObjectFieldValueChanged(ChangeEvent<SpeckleBranch> evt)
		{
			value = evt.newValue;
		}

		public void SetValueWithoutNotify(SpeckleBranch newValue)
		{
			if (newValue == null)
			{
				// Update the preview Image and update the ObjectField.
				m_Value = newValue;
				m_Title.name = m_Value.name;
			}
		}

		public SpeckleBranch value
		{
			get => m_Value;
			// The setter is called when the user changes the value of the ObjectField, which calls
			// OnObjectFieldValueChanged(), which calls this.
			set
			{
				if (value == this.value)
					return;

				var previous = this.value;
				SetValueWithoutNotify(value);

				using (var evt = ChangeEvent<SpeckleBranch>.GetPooled(previous, value))
				{
					evt.target = this;
					SendEvent(evt);
				}
			}
		}
	}
}