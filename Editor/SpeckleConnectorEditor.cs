using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.GUI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Speckle.ConnectorUnity
{
	[CustomEditor(typeof(SpeckleConnector))]
	public class SpeckleConnectorEditor : Editor
	{

		DropdownField accounts;

		DropdownField converters;

		Image img;

		SpeckleConnector obj;

		Button refresh;

		VisualElement root;

		VisualTreeAsset streamCard;

		ListView streamList;

		VisualTreeAsset tree;

		void OnEnable()
		{
			obj = (SpeckleConnector)target;
			obj.onRepaint += RefreshAll;

			tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GUIHelper.Dir + "SpeckleConnectorEditor.uxml");
			streamCard = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GUIHelper.Dir + "Elements/StreamCard/StreamCard.uxml");
		}

		void OnDisable()
		{
			obj.onRepaint -= RefreshAll;
		}

		// TODO: set a loading state for the gui 
		event Action onConnectorLoading;

		public override VisualElement CreateInspectorGUI()
		{
			if (tree == null)
				return base.CreateInspectorGUI();

			root = new VisualElement();
			tree.CloneTree(root);

			accounts = root.SetDropDown("account", FindInt("accountIndex"), obj.Accounts.Format(), e => AccountChange(e).Forget());

			refresh = root.Q<Button>("refresh");
			refresh.clickable.clicked += () =>
			{
				obj.Refresh().Forget();
				onConnectorLoading?.Invoke();
			};

			SetupList();

			return root;
		}

		void SetupList()
		{
			VisualElement makeItem()
			{
				var card = new VisualElement();
				streamCard.CloneTree(card);
				return card;
			}

			void bindItem(VisualElement e, int i)
			{
				var stream = obj.Streams[i];

				e.Q<Label>("title").text = stream.Name;
				e.Q<Label>("id").text = stream.Id;
				e.Q<Label>("description").text = stream.Description;

				var isActive = FindInt("streamIndex") == i;

				e.style.backgroundColor = isActive ? new StyleColor(Color.white) : new StyleColor(Color.clear);

				var ops = e.Q<VisualElement>("operation-container");
				if (ops == null)
				{
					Debug.Log("No container found");
					return;
				}

				ops.visible = isActive;

				// unbind all objects first just in case
				ops.Q<Button>("open-button").clickable.clickedWithEventInfo -= obj.OpenStreamInBrowser;
				ops.Q<Button>("send-button").clickable.clickedWithEventInfo -= obj.CreateSender;
				ops.Q<Button>("receive-button").clickable.clickedWithEventInfo -= obj.CreateReceiver;

				if (isActive)
				{
					ops.Q<Button>("open-button").clickable.clickedWithEventInfo += obj.OpenStreamInBrowser;
					ops.Q<Button>("send-button").clickable.clickedWithEventInfo += obj.CreateSender;
					ops.Q<Button>("receive-button").clickable.clickedWithEventInfo += obj.CreateReceiver;
				}
			}

			streamList = root.Q<ListView>("stream-list");

			streamList.bindItem = bindItem;
			streamList.makeItem = makeItem;
			streamList.fixedItemHeight = 50f;

			SetAndRefreshList();

			// Handle updating all list view items after selection 
			streamList.RegisterCallback<ClickEvent>(_ => streamList.RefreshItems());

			// Pass new selection back to object
			streamList.onSelectedIndicesChange += i => obj.SetStream(i.FirstOrDefault());
		}

		void SetAndRefreshList()
		{
			streamList.ClearSelection();
			streamList.itemsSource = obj.Streams;
			streamList.RefreshItems();
		}

		async UniTask AccountChange(ChangeEvent<string> evt)
		{
			var index = accounts.DropDownChange(evt);

			if (index < 0)
				return;

			await obj.SetAccount(index);
			RefreshAll();
		}

		int FindInt(string propName) => serializedObject.FindProperty(propName).intValue;

		void RefreshAll()
		{
			Debug.Log("Refresh called on Connector");
			Refresh(accounts, obj.Accounts.Format(), "accountIndex");
			SetAndRefreshList();
		}

		void Refresh(DropdownField dropdown, IEnumerable<string> items, string prop)
		{
			dropdown.choices = items?.ToList();
			dropdown.index = FindInt(prop);
		}
	}
}