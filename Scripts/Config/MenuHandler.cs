using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PugMod;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ItemBrowser.Config {
	internal static class MenuHandler {
		private static readonly Dictionary<RadicalMenu.MenuType, RadicalMenu> MenusByType = new();

		private static RadicalMenu AddMenu(int id, string term) {
			var modMenu = Object.Instantiate(Manager.menu.uiOptionsMenuPrefab, API.Rendering.UICamera.transform).GetComponent<RadicalOptionsMenu>();
			modMenu.gameObject.SetActive(value: false);
			modMenu.transform.Find("Title/Title bigtext").GetComponent<PugText>().Render(term);
			modMenu.transform.Find("Title/Title bigtext shadow").GetComponent<PugText>().Render(term);

			var modMenuScroll = modMenu.transform.Find("Options/Scroll");
			modMenu.menuOptions.Clear();
			for (var i = 0; i < modMenuScroll.childCount; i++)
				Object.Destroy(modMenuScroll.GetChild(i).gameObject);

			// add option to open mod settings in main settings (above controls, below UI settings)
			var settingsMenu = Manager.menu.optionsMenu;
			var settingsMenuScroll = settingsMenu.transform.Find("Options/Scroll");
			var goToUiSettings = settingsMenuScroll.Find("Go to UI settings").GetComponent<RadicalMenuOption>();
			var goToModSettings = Object.Instantiate(goToUiSettings.gameObject, settingsMenuScroll).GetComponent<RadicalOptionsMenuOption_PushMenu>();
			goToModSettings.transform.SetSiblingIndex(goToUiSettings.transform.GetSiblingIndex() + 1);
			goToModSettings.labelText.Render(term);
			goToModSettings.menuToPush = (RadicalMenu.MenuType) id;
			goToModSettings.SetParentMenu(settingsMenu);
			settingsMenu.menuOptions.Insert(goToModSettings.transform.GetSiblingIndex(), goToModSettings);

			MenusByType[goToModSettings.menuToPush] = modMenu;
			
			return modMenu;
		}

		private static void AddMenuOption(RadicalMenu menu, string prefabPath) {
			var scroll = menu.transform.Find("Options/Scroll");
			var prefab = Main.AssetBundle.LoadAsset<GameObject>(prefabPath);
			
			foreach (var prefabMenuOption in prefab.GetComponentsInChildren(typeof(RadicalMenuOption), true)) {
				var instance = Object.Instantiate(prefabMenuOption.gameObject, scroll);
				var menuOption = instance.GetComponent<RadicalMenuOption>();
				menuOption.SetParentMenu(menu);
				menu.menuOptions.Add(menuOption);
			}
		}

		[HarmonyPatch]
		public static class Patches {
			[HarmonyPatch(typeof(MenuManager), "Init")]
			[HarmonyPostfix]
			public static void MenuManager_Init(MenuManager __instance) {
				var settings = AddMenu(19900, "ItemBrowser:Config");
				AddMenuOption(settings, "Assets/ItemBrowser/Prefabs/MenuOptions.prefab");
			}
			
			[HarmonyPatch(typeof(RadicalMenu), "TypeToMenu")]
			[HarmonyPrefix]
			public static void MenuManager_PopMenu(MenuManager __instance) {
				var topMenu = __instance.GetTopMenu();

				if (MenusByType.Values.Contains(topMenu))
					Main.Config.Save();
			}
			
			[HarmonyPatch(typeof(RadicalMenu), "TypeToMenu")]
			[HarmonyPrefix]
			public static bool RadicalMenu_TypeToMenu(RadicalMenu.MenuType type, ref RadicalMenu __result) {
				if (!MenusByType.TryGetValue(type, out var menu))
					return true;
				
				__result = menu;
				return false;
			}
		}
	}
}