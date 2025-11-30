using System;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class ObjectListContainerWindow : ItemBrowserWindow {
		[SerializeField]
		private ItemListWindow itemListWindow;
		[SerializeField]
		private CreatureListWindow creatureListWindow;
		[SerializeField]
		private Transform tabButtonsRoot;
		[SerializeField]
		private BasicButton itemListTabButton;
		[SerializeField]
		private BasicButton creatureListTabButton;

		private WindowTab _currentTab;

		private ObjectListWindow CurrentWindow => _currentTab switch {
			WindowTab.ItemList => itemListWindow,
			WindowTab.CreatureList => creatureListWindow,
			_ => throw new ArgumentOutOfRangeException()
		};
		
		private enum WindowTab {
			ItemList,
			CreatureList
		}

		private void Update() {
			UpdateControllerInput();
		}

		private void UpdateControllerInput() {
			if (UserInterfaceUtils.IsUsingMouseAndKeyboard)
				return;
			
			var inputModule = Manager.input.singleplayerInputModule;
			if (inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_IN_MAP) || inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_NEXT_MAP_MARKER))
				CycleToNextTab();
			if (inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_OUT_MAP) || inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_PREVIOUS_MAP_MARKER))
				CycleToPreviousTab();
		}

		private void SetTab(WindowTab tab) {
			_currentTab = tab;
			itemListWindow.IsShowing = _currentTab == WindowTab.ItemList;
			creatureListWindow.IsShowing = _currentTab == WindowTab.CreatureList;

			itemListTabButton.IsToggled = itemListWindow.IsShowing;
			creatureListTabButton.IsToggled = creatureListWindow.IsShowing;

			tabButtonsRoot.SetParent(CurrentWindow.tabButtonsAnchor, false);
		}

		public void SetItemsTab() {
			SetTab(WindowTab.ItemList);
		}

		public void SetCreaturesTab() {
			SetTab(WindowTab.CreatureList);
		}

		private void CycleToNextTab() {
			SetTab(_currentTab switch {
				WindowTab.ItemList => WindowTab.CreatureList,
				WindowTab.CreatureList => WindowTab.ItemList,
				_ => throw new ArgumentOutOfRangeException()
			});
			UserInterfaceUtils.PlayMenuOpenSound();
		}

		private void CycleToPreviousTab() {
			CycleToNextTab();
		}
	}
}