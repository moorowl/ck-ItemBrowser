using System;
using System.Collections.Generic;
using HarmonyLib;
using I2.Loc;
using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using ItemBrowser.Entries;
using ItemBrowser.Utilities.DataStructures;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using PugMod;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ItemBrowser {
	public static class ItemBrowserAPI {
		public static event Action OnClientLanguageChanged;
		public static event Action OnInit;
		
		public static ItemBrowserUI ItemBrowserUI { get; private set; }

		internal static List<(string Group, Filter<ObjectDataCD> Filter)> ItemFilters = new();
		internal static List<(string Group, Filter<ObjectDataCD> Filter)> CreatureFilters = new();
		internal static List<(string Group, Filter<ObjectDataCD> Filter)> CookingFilters = new();

		internal static List<Sorter<ObjectDataCD>> ItemSorters = new();
		internal static List<Sorter<ObjectDataCD>> CreatureSorters = new();
		internal static List<Sorter<ObjectDataCD>> CookingSorters = new();

		internal static ObjectEntryRegistry ObjectEntries = new();
		internal static readonly List<ObjectEntryProvider> ObjectEntryProviders = new();
		internal static readonly Dictionary<Type, GameObject> ObjectEntryDisplayPrefabs = new();

		private static readonly List<ObjectNameAndIconOverride> ObjectNameAndIconOverrides = new();
		internal static Dictionary<ObjectDataCD, string> ObjectNameOverrides = new();
		internal static Dictionary<ObjectDataCD, Sprite> ObjectIconOverrides = new();

		private static bool _hasRegisteredContent;
		
		private static void InitBrowserUI() {
			var prefab = Main.AssetBundle.LoadAsset<GameObject>("Assets/ItemBrowser/Prefabs/ItemBrowserUI.prefab");
			ItemBrowserUI = Object.Instantiate(prefab, API.Rendering.UICamera.transform).GetComponent<ItemBrowserUI>();

			foreach (var overrides in ObjectNameAndIconOverrides) {
				var objectData = overrides.ObjectData;
				
				if (overrides.overrideName && !string.IsNullOrWhiteSpace(overrides.name))
					ObjectNameOverrides.TryAdd(objectData, overrides.name);
				
				if (overrides.overrideIcon)
					ObjectIconOverrides.TryAdd(objectData, overrides.icon);
			}
			
			ObjectUtils.InitOnWorldLoad();

			if (!_hasRegisteredContent) {
				OnInit?.Invoke();
				_hasRegisteredContent = true;
			}
			
			ObjectEntries.RegisterFromProviders(ObjectEntryProviders);
		}

		private static void UninitBrowserUI() {
			if (ItemBrowserUI != null)
				Object.Destroy(ItemBrowserUI);
		}
		
		public static void RegisterObjectEntryProviders(params ObjectEntryProvider[] providers) {
			ObjectEntryProviders.AddRange(providers);
		}

		public static void RegisterObjectEntryDisplay(ObjectEntryDisplayBase component) {
			if (component == null)
				return;

			var gameObject = component.gameObject;
			foreach (var sr in gameObject.GetComponentsInChildren<SpriteRenderer>())
				sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
			foreach (var pugText in gameObject.GetComponentsInChildren<PugText>())
				pugText.style.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

			ObjectEntryDisplayPrefabs.TryAdd(component.AssociatedEntry, gameObject);
		}

		public static void RegisterObjectNameAndIconOverride(ObjectNameAndIconOverride overrides) {
			ObjectNameAndIconOverrides.Add(overrides);
		}

		public static void RegisterItemFilter(string group, Filter<ObjectDataCD> filter) {
			ItemFilters.Add((group, filter));
		}

		public static void RegisterCreatureFilter(string group, Filter<ObjectDataCD> filter) {
			CreatureFilters.Add((group, filter));
		}

		public static void RegisterCookingFilter(string group, Filter<ObjectDataCD> filter) {
			CookingFilters.Add((group, filter));
		}

		public static bool ShouldItemBeIncluded(ObjectDataCD objectData) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);

			if (PugDatabase.HasComponent<CookedFoodCD>(objectData))
				return false;

			if (objectInfo.objectType is ObjectType.Creature or ObjectType.Critter or ObjectType.PlayerType && !PugDatabase.HasComponent<PetCD>(objectData))
				return false;

			if (PugDatabase.HasComponent<ProjectileCD>(objectData))
				return false;

			return (objectInfo.variation == 0 || objectInfo.objectID == ObjectID.Bucket || (objectInfo.objectID == ObjectID.LargeAncientDestructible && objectInfo.variation <= 5) || objectInfo.objectID == ObjectID.LargeCityDestructible || objectInfo.objectID == ObjectID.LargeMoldDestructible || objectInfo.objectID == ObjectID.LargeJellyfishDestructable || objectInfo.objectID == ObjectID.LargeDesertDestructible || objectInfo.objectID == ObjectID.WoodenDestructible || objectInfo.objectID == ObjectID.NatureWoodenDestructible || objectInfo.objectID == ObjectID.SeaWoodenDestructible || objectInfo.objectID == ObjectID.LavaWoodenDestructible || objectInfo.objectID == ObjectID.LargeDesertTempleDestructible || objectInfo.objectID == ObjectID.HiveDestructible || objectInfo.objectID == ObjectID.LargeMoldDestructible || objectInfo.objectID == ObjectID.NatureDestructible || objectInfo.objectID == ObjectID.GreenLargeDesertDestructible || objectInfo.objectID == ObjectID.LargeAlienTechDestructible || objectInfo.objectID == ObjectID.Stalagmite || (objectInfo.objectID == ObjectID.Larva && objectInfo.variation <= 1) || (objectInfo.objectID == ObjectID.BigLarva && objectInfo.variation <= 1)) && !objectInfo.isCustomScenePrefab;
		}
		
		public static bool ShouldCreatureBeIncluded(ObjectDataCD objectData) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);

			if (PugDatabase.HasComponent<CookedFoodCD>(objectData))
				return false;

			if ((objectInfo.objectType != ObjectType.Creature && objectInfo.objectType != ObjectType.Critter) || PugDatabase.HasComponent<PetCD>(objectData))
				return false;

			if (PugDatabase.HasComponent<ProjectileCD>(objectData))
				return false;

			return (objectInfo.variation == 0 || objectInfo.objectID == ObjectID.BirdBoss || objectInfo.objectID == ObjectID.Bucket || (objectInfo.objectID == ObjectID.LargeAncientDestructible && objectInfo.variation <= 5) || objectInfo.objectID == ObjectID.LargeCityDestructible || objectInfo.objectID == ObjectID.LargeMoldDestructible || objectInfo.objectID == ObjectID.LargeJellyfishDestructable || objectInfo.objectID == ObjectID.LargeDesertDestructible || objectInfo.objectID == ObjectID.WoodenDestructible || objectInfo.objectID == ObjectID.NatureWoodenDestructible || objectInfo.objectID == ObjectID.SeaWoodenDestructible || objectInfo.objectID == ObjectID.LavaWoodenDestructible || objectInfo.objectID == ObjectID.LargeDesertTempleDestructible || objectInfo.objectID == ObjectID.HiveDestructible || objectInfo.objectID == ObjectID.LargeMoldDestructible || objectInfo.objectID == ObjectID.NatureDestructible || objectInfo.objectID == ObjectID.GreenLargeDesertDestructible || objectInfo.objectID == ObjectID.LargeAlienTechDestructible || objectInfo.objectID == ObjectID.Stalagmite || (objectInfo.objectID == ObjectID.Larva && objectInfo.variation <= 1) || (objectInfo.objectID == ObjectID.BigLarva && objectInfo.variation <= 1)) && !objectInfo.isCustomScenePrefab;
		}

		public static void RegisterItemSorter(Sorter<ObjectDataCD> sorter) {
			ItemSorters.Add(sorter);
		}

		public static void RegisterCreatureSorter(Sorter<ObjectDataCD> sorter) {
			CreatureSorters.Add(sorter);
		}

		public static void RegisterCookingSorter(Sorter<ObjectDataCD> sorter) {
			CookingSorters.Add(sorter);
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