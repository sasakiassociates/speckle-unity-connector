using System.Linq;
using Speckle.ConnectorUnity.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity
{
	[CustomEditor(typeof(BaseBehaviour), true)]
	public class BaseBehaviourEditor : Editor
	{

		BaseBehaviour obj;
		VisualElement root;

		protected(string speckleType, string appId, string totalChildCount, string id, string props) _fields;

		// VisualTreeAsset tree;
		// string treePath => null +  "obj.uxml";

		protected virtual void OnEnable()
		{
			// tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(treePath);

			obj = (BaseBehaviour)target;

			_fields.speckleType = "_speckle_type";
			_fields.totalChildCount = "_totalChildCount";
			_fields.appId = "_applicationId";
			_fields.id = "_id";
			_fields.props = "_props";
		}

		public override VisualElement CreateInspectorGUI()
		{
			// if (tree == null)
			// return base.CreateInspectorGUI();
			// tree.CloneTree(root);

			root = new VisualElement();

			root.Add(new PropertyField(serializedObject.FindProperty(_fields.speckleType)));
			root.Add(new PropertyField(serializedObject.FindProperty(_fields.totalChildCount)));
			root.Add(new PropertyField(serializedObject.FindProperty(_fields.appId)));
			root.Add(new PropertyField(serializedObject.FindProperty(_fields.id)));
			root.Add(new PropertyField(serializedObject.FindProperty(_fields.props)));

			// root.Add(new Label("Props"));
			// root.Add(SetupList());
		
			return root;
		}

		VisualElement SetupList()
		{
			VisualElement makeItem()
			{
				var card = new VisualElement();
				return card;
			}

			var keys = obj.Props.Data.Keys.ToArray();

			void bindItem(VisualElement e, int i)
			{
				var propName = keys[i];
				var prop = obj.Props.Data[propName];
				e.Add(new Label(propName));
				e.Add(new Label(prop.GetType().ToString()));

				// e.Q<Label>("title").text = stream.Name;
				// e.Q<Label>("id").text = stream.Id;
				// e.Q<Label>("description").text = stream.Description;
			}

			var streamList = new ListView
			{
				bindItem = bindItem,
				makeItem = makeItem,
				fixedItemHeight = 15f
			};

			// Handle updating all list view items after selection 
			streamList.RegisterCallback<ClickEvent>(_ => streamList.RefreshItems());

			return streamList;
		}

	}

}