using System;
using UnityEngine;

namespace ItemBrowser.Browser.ObjectList {
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
	}
}