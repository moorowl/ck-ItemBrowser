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
		private Transform optionsPanelRoot;
		[SerializeField]
		private BasicButton itemListTabButton;
		[SerializeField]
		private BasicButton creatureListTabButton;

		private WindowTab _currentTab;
		private UIelement _lastSelectedElement;

		private ObjectListWindow CurrentWindow => _currentTab switch {
			WindowTab.ItemList => itemListWindow,
			WindowTab.CreatureList => creatureListWindow,
			_ => throw new ArgumentOutOfRangeException()
		};
		
		private enum WindowTab {
			ItemList,
			CreatureList
		}

		protected override void OnShow(bool isFirstTimeShowing) {
			TrySelectLastSelectedElement();
		}

		private void Update() {
			UpdateControllerInput();
		}

		private void LateUpdate() {
			UpdateLastSelectedElement();
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

		private void TrySelectLastSelectedElement() {
			if (_lastSelectedElement != null && !UserInterfaceUtils.IsUsingMouseAndKeyboard)
				UserInterfaceUtils.SelectAndMoveMouseTo(_lastSelectedElement);
		}

		private void UpdateLastSelectedElement() {
			if (Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement || !SnapPoint.HasSnapPoint(Manager.ui.currentSelectedUIElement))
				return;
			
			_lastSelectedElement = Manager.ui.currentSelectedUIElement;
		}

		private void SetTab(WindowTab tab) {
			_currentTab = tab;
			itemListWindow.IsShowing = _currentTab == WindowTab.ItemList;
			creatureListWindow.IsShowing = _currentTab == WindowTab.CreatureList;

			itemListTabButton.IsToggled = itemListWindow.IsShowing;
			creatureListTabButton.IsToggled = creatureListWindow.IsShowing;

			tabButtonsRoot.SetParent(CurrentWindow.tabButtonsAnchor, false);
			optionsPanelRoot.SetParent(CurrentWindow.optionsPanelAnchor, false);
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
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.ChangeTypeOrCategory, this);
		}

		private void CycleToPreviousTab() {
			CycleToNextTab();
		}
	}
}