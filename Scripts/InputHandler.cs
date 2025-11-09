using System;
using System.Collections.Generic;
using HarmonyLib;
using ItemBrowser.DataStructures;
using ItemBrowser.Entries;
using PlayerState;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ItemBrowser {
	[HarmonyPatch]
	internal static class InputHandler {
		private const PlayerInput.InputType ToggleBrowserInput = (PlayerInput.InputType) 39000;
		private const PlayerInput.InputType ShowSourcesInput = (PlayerInput.InputType) 39001;
		private const PlayerInput.InputType ShowUsagesInput = (PlayerInput.InputType) 39002;
		
		[HarmonyPatch(typeof(InputManager), "LateUpdate")]
		[HarmonyPostfix]
		public static void InputManager_LateUpdate(InputManager __instance) {
			var player = Manager.main.player;
			
			if (player == null || ItemBrowserAPI.ItemBrowserUI == null || Time.timeScale == 0f || EntityUtility.GetComponentData<PlayerStateCD>(player.entity, player.world).isStateLocked || !Manager.main.currentSceneHandler.isSceneHandlerReady)
				return;
			
			var input = Manager.input.singleplayerInputModule;
			if (input.WasButtonPressedDownThisFrame(ToggleBrowserInput))
				ItemBrowserAPI.ItemBrowserUI.IsShowing = !ItemBrowserAPI.ItemBrowserUI.IsShowing;

			if (Manager.ui.currentSelectedUIElement is SlotUIBase slot) {
				var containedObjectData = slot.GetContainedObject().objectData;
				if (containedObjectData.objectID != ObjectID.None) {
					if (input.WasButtonPressedDownThisFrame(ShowSourcesInput))
						ItemBrowserAPI.ItemBrowserUI.ShowObjectEntries(containedObjectData, ObjectEntryType.Source);

					if (input.WasButtonPressedDownThisFrame(ShowUsagesInput))
						ItemBrowserAPI.ItemBrowserUI.ShowObjectEntries(containedObjectData, ObjectEntryType.Usage);
				}
			}
		}
		
		[HarmonyPatch(typeof(InputManager), "Init")]
		[HarmonyPrefix]
		public static void InputManager_Init(InputManager __instance) {
			var inputManagerBase = Resources.Load<InputManager_Base>("Rewired Input Manager");
			var userData = inputManagerBase.userData;

			RegisterKeybind(userData, "ItemBrowser:ToggleBrowser", ToggleBrowserInput, new KeybindDefaults {
				KeyboardKeyCode = KeyboardKeyCode.Q,
				KeyboardModifierKey = ModifierKey.Alt
			});
			RegisterKeybind(userData, "ItemBrowser:ShowSources", ShowSourcesInput, new KeybindDefaults {
				KeyboardKeyCode = KeyboardKeyCode.O
			});
			RegisterKeybind(userData, "ItemBrowser:ShowUsages", ShowUsagesInput, new KeybindDefaults {
				KeyboardKeyCode = KeyboardKeyCode.U
			});
		}

		private static void RegisterKeybind(UserData userData, string name, PlayerInput.InputType inputType, KeybindDefaults defaults) {
			var newAction = new InputAction();
			newAction.SetValue("_id", (int) inputType);
			newAction.SetValue("_categoryId", 17);
			newAction.SetValue("_name", name);
			newAction.SetValue("_type", InputActionType.Button);
			newAction.SetValue("_descriptiveName", name);
			newAction.SetValue("_userAssignable", true);

			userData.GetValue<List<InputAction>>("actions").Add(newAction);
			userData.GetValue<ActionCategoryMap>("actionCategoryMap").AddAction(17, (int) inputType);

			if (defaults.KeyboardKeyCode != 0) {
				var keyboardMap = userData.GetValue<List<ControllerMap_Editor>>("keyboardMaps")[5];
				
				var keyboardActionElementMap = new ActionElementMap();
				keyboardActionElementMap.SetValue("_actionId", (int) inputType);
				keyboardActionElementMap.SetValue("_elementType", ControllerElementType.Button);
				keyboardActionElementMap.SetValue("_actionCategoryId", 17);
				keyboardActionElementMap.SetValue("_keyboardKeyCode", defaults.KeyboardKeyCode);
				keyboardActionElementMap.SetValue("_modifierKey1", defaults.KeyboardModifierKey);
				
				keyboardMap.actionElementMaps.Add(keyboardActionElementMap);	
			}
		}
		
		public class KeybindDefaults {
			public KeyboardKeyCode KeyboardKeyCode;
			public ModifierKey KeyboardModifierKey;
		}
	}
}