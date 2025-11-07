using System;
using HarmonyLib;
using ItemBrowser.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ItemBrowser.Browser {
	public class ItemBrowserUI : ItemBrowserWindow {
		public static event Action<ItemBrowserUI> OnInit;
		public static event Action<ItemBrowserUI> OnUninit;
		
		[SerializeField]
		private ObjectListWindow objectListWindow;
		[SerializeField]
		private ObjectEntriesWindow objectEntriesWindow;

		private void Awake() {
			gameObject.SetActive(false);
			OnInit?.Invoke(this);
		}
		
		private void OnDestroy() {
			OnUninit?.Invoke(this);
		}

		protected override void OnShow(bool isFirstTimeShowing) {
			if (isFirstTimeShowing) {
				objectListWindow.IsShowing = true;
				objectEntriesWindow.IsShowing = false;
			}
			
			UpdateScale();
			HideMapAndInventoryIfShowing();
			PlayToggleSound();
			Manager.ui.DeselectAnySelectedUIElement();
		}

		protected override void OnHide() {
			PlayToggleSound();
		}

		public bool ShowObjectEntries(ObjectDataCD objectData, ObjectEntryType type) {
			if (!objectEntriesWindow.PushObjectData(objectData, type, !IsShowing)) {
				AudioManager.SfxUI(SfxID.menu_denied, 1.15f, false, 0.4f, 0.05f);
				return false;
			}

			IsShowing = true;
			objectListWindow.IsShowing = false;
			objectEntriesWindow.IsShowing = true;
			
			UserInterfaceUtils.PlayMenuOpenSound();

			return true;
		}
		
		public void ShowItemList() {
			IsShowing = true;
			objectListWindow.IsShowing = true;
			objectEntriesWindow.IsShowing = false;
			objectEntriesWindow.Clear();
		}

		private void LateUpdate() {
			UpdateScale();
			UpdateGoBack();
			HideMapAndInventoryIfShowing();
		}

		private void UpdateScale() {
			transform.localScale = Manager.ui.CalcGameplayUITargetScaleMultiplier();
		}
		
		private void UpdateGoBack() {
			if (Manager.menu.IsAnyMenuActive() || Manager.input.activeInputField != null || !Manager.input.IsMenuStartButtonDown())
				return;

			if (objectEntriesWindow.HasAnyHistory) {
				objectEntriesWindow.PopObjectData();
				UserInterfaceUtils.PlayMenuCloseSound();
			} else if (objectEntriesWindow.IsShowing) {
				ShowItemList();
				UserInterfaceUtils.PlayMenuCloseSound();
			} else {
				IsShowing = false;
			}
		}

		private static void HideMapAndInventoryIfShowing() {
			if (Manager.ui.isAnyInventoryShowing)
				Manager.ui.HideAllInventoryAndCraftingUI();
			
			if (Manager.ui.isShowingMap)
				Manager.ui.HideMap();
		}

		private static void PlayToggleSound() {
			if (Manager.main.player == null)
				return;
			
			AudioManager.Sfx(SfxTableID.inventorySFXInfoTab, Manager.main.player.transform.position);
		}
		
		[HarmonyPatch]
		public static class Patches {
			[HarmonyPatch(typeof(MenuManager), "IsPauseDisabled")]
			[HarmonyPostfix]
			private static void MenuManager_IsPauseDisabled(MenuManager __instance, ref bool __result) {
				if (ItemBrowserAPI.ItemBrowserUI!= null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
			
			[HarmonyPatch(typeof(PlayerController), "get_isInteractionBlocked")]
			[HarmonyPostfix]
			private static void PlayerController_get_isInteractionBlocked(PlayerController __instance, ref bool __result) {
				// Prevent using items / scrolling hotbar
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
			
			[HarmonyPatch(typeof(PlayerController), "get_isUIShortCutsBlocked")]
			[HarmonyPostfix]
			private static void PlayerController_get_isUIShortCutsBlocked(PlayerController __instance, ref bool __result) {
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
			
			[HarmonyPatch(typeof(PlayerController), "get_isMovingBlocked")]
			[HarmonyPostfix]
			private static void PlayerController_get_isMovingBlocked(PlayerController __instance, ref bool __result) {
				// Pretty sure this doesn't do anything
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = false;
			}
			
			[HarmonyPatch(typeof(UIManager), "get_isMouseShowing")]
			[HarmonyPostfix]
			private static void UIManager_get_isMouseShowing(UIManager __instance, ref bool __result) {
				// Force mouse to appear (for controllers)
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}

			[HarmonyPatch(typeof(SendClientInputSystem), "PlayerInteractionBlocked")]
			[HarmonyPostfix]
			private static void SendClientInputSystem_PlayerInteractionBlocked(SendClientInputSystem __instance, ref bool __result) {
				// Prevent using items / scrolling hotbar
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
			
			[HarmonyPatch(typeof(SendClientInputSystem), "PlayerInputBlocked")]
			[HarmonyPostfix]
			private static void SendClientInputSystem_PlayerInputBlocked(SendClientInputSystem __instance, ref bool __result) {
				// Prevent moving when using a controller
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing && !Manager.input.singleplayerInputModule.PrefersKeyboardAndMouse())
					__result = true;
			}
			
			[HarmonyPatch(typeof(InGameButtonHintsUI), "LateUpdate")]
			[HarmonyPrefix]
			private static bool InGameButtonHintsUI_LateUpdate(InGameButtonHintsUI __instance) {
				// Hide button hints in bottom right
				if (ItemBrowserAPI.ItemBrowserUI != null && !ItemBrowserAPI.ItemBrowserUI.IsShowing)
					return true;
				
				__instance.container.SetActive(false);
				return false;
			}
		}
	}
}