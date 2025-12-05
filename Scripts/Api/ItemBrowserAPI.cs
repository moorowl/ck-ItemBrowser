using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using I2.Loc;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using ItemBrowser.Api.Entries;
using PugMod;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ItemBrowser.Api {
	public static class ItemBrowserAPI {
		public static event Action OnClientLanguageChanged;

		public static ItemBrowserUI ItemBrowserUI { get; private set; }

		internal static readonly ItemBrowserRegistry Registry = new();
		internal static readonly ObjectEntryRegistry ObjectEntryRegistry = new();
		private static readonly List<ItemBrowserPlugin> Plugins = new();
		
		private static bool _hasRegistered;
		
		public static void AddPlugin<T>(IMod sourceMod) where T : ItemBrowserPlugin, new() {
			var loadedSourceMod = API.ModLoader.LoadedMods.FirstOrDefault(loadedMod => loadedMod.Handlers.Contains(sourceMod));
			if (loadedSourceMod == null)
				return;

			var plugin = new T {
				AssociatedMod = loadedSourceMod
			};
			Plugins.Add(plugin);
			
			Main.Log(nameof(ItemBrowserAPI), $"Added plugin: {plugin.GetType().GetNameChecked()}");
		}
		
		private static void InitBrowserUI() {
			var prefab = Main.AssetBundle.LoadAsset<GameObject>("Assets/ItemBrowser/Prefabs/ItemBrowserUI.prefab");
			ItemBrowserUI = Object.Instantiate(prefab, API.Rendering.UICamera.transform).GetComponent<ItemBrowserUI>();
			
			ObjectUtils.InitOnWorldLoad();
			StructureUtils.InitOnWorldLoad();

			if (!_hasRegistered) {
				_hasRegistered = true;

				foreach (var plugin in Plugins)
					plugin.OnRegister(Registry);
				
				ObjectEntryRegistry.RegisterFromProviders(Registry.EntryProviders);
			}
		}

		private static void UninitBrowserUI() {
			if (ItemBrowserUI != null)
				Object.Destroy(ItemBrowserUI);
		}

		public static bool IsItemIndexed(ObjectDataCD item) {
			return Registry.Items.Contains(item);
		}
		
		public static bool IsCreatureIndexed(ObjectDataCD creature) {
			return Registry.Creatures.Contains(creature);
		}
		
		public static bool IsTechnicalItem(ObjectDataCD item) {
			return Registry.TechnicalItems.Contains(item);
		}

		public static bool IsTechnicalCreature(ObjectDataCD creature) {
			return Registry.TechnicalCreatures.Contains(creature);
		}
		
		[HarmonyPatch]
		public static class Patches {
			private static string _lastLanguage;

			[HarmonyPatch(typeof(PlayerController), "ManagedUpdate")]
			[HarmonyPostfix]
			private static void PlayerController_ManagedUpdate(PlayerController __instance) {
				if (_lastLanguage == LocalizationManager.CurrentLanguage)
					return;

				_lastLanguage = LocalizationManager.CurrentLanguage;
				OnClientLanguageChanged?.Invoke();
			}

			[HarmonyPatch(typeof(PlayerController), "OnOccupied")]
			[HarmonyPostfix]
			private static void PlayerController_OnOccupied(PlayerController __instance) {
				if (!__instance.isLocal)
					return;

				UninitBrowserUI();
				InitBrowserUI();
			}

			[HarmonyPatch(typeof(PlayerController), "OnFree")]
			[HarmonyPostfix]
			private static void PlayerController_OnFree(PlayerController __instance) {
				if (!__instance.isLocal)
					return;

				UninitBrowserUI();
				InitBrowserUI();
			}
		}
	}
}