using System;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser;
using ItemBrowser.Entries;
using ItemBrowser.Browser;
using ItemBrowser.Entries.Defaults;
using ItemBrowser.Utilities.DataStructures;
using PugMod;
using PugProperties;
using PugTilemap;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

public class Main : IMod {
	public const string Version = "0.1.0";
	public const string InternalName = "ItemBrowser";
	public const string DisplayName = "Item Browser";

	internal static Options Options { get; private set; } = new();
	internal static AssetBundle AssetBundle { get; private set; }

	public void EarlyInit() {
		Debug.Log($"[{DisplayName}]: Mod version: {Version}");

		var modInfo = API.ModLoader.LoadedMods.FirstOrDefault(modInfo => modInfo.Handlers.Contains(this));
		AssetBundle = modInfo!.AssetBundles[0];

		ItemBrowserUI.OnInit += _ => {
			BuiltinContent.Register();
		};
	}

	public void Init() {
		ModUtils.InitOnModLoad();
	}

	public void Shutdown() { }

	public void Update() { }

	public void ModObjectLoaded(Object obj) {
		BuiltinContent.OnModObjectLoaded(obj);
	}

	public static void Log(string context, string text) {
		Debug.Log($"[ItemBrowser] {context}: {text}");
	}
	
	private static class BuiltinContent {
		public static void Register() {
			ItemBrowserAPI.RegisterObjectEntryProviders(
				new ArchaeologistDrops.Provider(),
				new BackgroundPerks.Provider(),
				new CattleProduce.Provider(),
				new Crafting.Provider(),
				new Drops.Provider(),
				new Farming.Provider(),
				new Fishing.Provider(),
				new JewelryCrafter.Provider(),
				new LockedChestDrops.Provider(),
				new Merchant.Provider(),
				new NaturalSpawnInitial.Provider(),
				new NaturalSpawnRespawn.Provider(),
				new OreBoulderExtraction.Provider(),
				new Salvaging.Provider(),
				new Trading.Provider(),
				new BackgroundPerks.Provider(),
				new Loot.Provider(),
				new ChallengeArenaReward.Provider(),
				new ItemBrowser.Entries.Defaults.VendingMachine.Provider(),
				new StructureContents.Provider(),
				new NaturalSpawnAroundObject.Provider(),
				new TerrainGeneration.Provider(),
				new MerchantSpawning.Provider(),
				new Miscellaneous.Provider(),
				new Breeding.Provider()
			);

			RegisterItemSorters();
			RegisterItemFilters();
		}

		public static void OnModObjectLoaded(Object obj) {
			if (obj is not GameObject gameObject)
				return;
			
			if (gameObject.TryGetComponent<ObjectEntryDisplayBase>(out var displayComponent))
				ItemBrowserAPI.RegisterObjectEntryDisplay(displayComponent);

			foreach (var overrides in gameObject.GetComponents(typeof(ObjectNameAndIconOverride)))
				ItemBrowserAPI.RegisterObjectNameAndIconOverride((ObjectNameAndIconOverride) overrides);
		}

		private static void RegisterItemSorters() {
			ItemBrowserAPI.RegisterItemSorter(new("ItemBrowser:ItemSorter/Alphabetical") {
				Function = objectData => -ObjectUtils.GetDisplayNameSortOrder(objectData.objectID, objectData.variation)
			});
			/*RegisterItemSorter(new("ItemBrowser:ItemSorter/Category") {
				Function = objectData => -ObjectUtils.GetDisplayNameScore(objectData.objectID, objectData.variation)
			});*/
			ItemBrowserAPI.RegisterItemSorter(new("ItemBrowser:ItemSorter/InternalIndex") {
				Function = objectData => (int) objectData.objectID * 10000 + objectData.variation
			});
			ItemBrowserAPI.RegisterItemSorter(new("ItemBrowser:ItemSorter/Damage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation)
			});
			ItemBrowserAPI.RegisterItemSorter(new("ItemBrowser:ItemSorter/Level") {
				Function = objectData => ObjectUtils.GetBaseLevel(objectData.objectID, objectData.variation)
			});
			ItemBrowserAPI.RegisterItemSorter(new("ItemBrowser:ItemSorter/Value") {
				Function = objectData => ObjectUtils.GetValue(objectData.objectID, objectData.variation)
			});
		}

		private static void RegisterItemFilters() {
			// Source
			const string sourceGroup = "ItemBrowser:ItemFilter/Source";
			ItemBrowserAPI.RegisterItemFilter(sourceGroup, new($"{sourceGroup}_FromMods") {
				Function = objectData => (int) objectData.objectID > Constants.maxNonModdedObjectID,
				Group = "Source"
			});
			foreach (var mod in API.ModLoader.LoadedMods.OrderBy(mod => ModUtils.GetDisplayName(mod.ModId))) {
				var displayName = ModUtils.GetDisplayName(mod.ModId);
				var objectIds = ModUtils.GetAssociatedObjects(mod.ModId)
					.Where(id => ItemBrowserAPI.ShouldItemBeIncluded(new ObjectDataCD { objectID = id }))
					.ToList();

				if (objectIds.Count == 0)
					continue;

				ItemBrowserAPI.RegisterItemFilter(sourceGroup, new($"{sourceGroup}_FromMod") {
					NameFormatFields = new[] { displayName },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { displayName },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => objectIds.Contains(objectData.objectID),
					Group = "Source"
				});
			}

			// Damage
			const string damageGroup = "ItemBrowser:ItemFilter/Damage";
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_AnyDamage") {
				Function = objectData => PugDatabase.HasComponent<HasWeaponDamageCD>(objectData)
				                         || (PugDatabase.TryGetComponent<SecondaryUseCD>(objectData, out var secondaryUse) && secondaryUse.summonsMinion)
			});
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_PhysicalMeleeDamage") {
				Function = objectData => PugDatabase.TryGetComponent<HasWeaponDamageCD>(objectData, out var weaponDamage) && weaponDamage is { isRange: false, isMagic: false }
			});
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_PhysicalRangeDamage") {
				Function = objectData => PugDatabase.TryGetComponent<HasWeaponDamageCD>(objectData, out var weaponDamage) && weaponDamage is { isRange: true, isMagic: false }
			});
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_MagicDamage") {
				Function = objectData => PugDatabase.TryGetComponent<HasWeaponDamageCD>(objectData, out var weaponDamage) && weaponDamage is { isMagic: true }
			});
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_SummonDamage") {
				Function = objectData => PugDatabase.TryGetComponent<SecondaryUseCD>(objectData, out var secondaryUse) && secondaryUse.summonsMinion
			});

			// Equipment
			const string equipmentGroup = "ItemBrowser:ItemFilter/Equipment";
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Weapon") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					if (PugDatabase.HasComponent<HasWeaponDamageCD>(objectData) && !ObjectUtils.MiningToolObjectTypes.Contains(objectType))
						return true;

					return PugDatabase.TryGetComponent<SecondaryUseCD>(objectData, out var secondaryUse) && secondaryUse.summonsMinion;
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Tool") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtils.ToolObjectTypes.Contains(objectType);
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Armor") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtils.ArmorObjectTypes.Contains(objectType);
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Helm") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Helm;
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_BreastArmor") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.BreastArmor;
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_PantsArmor") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.PantsArmor;
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Accessory") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtils.AccessoryObjectTypes.Contains(objectType);
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Ring") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Ring;
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Necklace") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Necklace;
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_OffHand") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Offhand;
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Bag") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Bag;
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Pouch") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Pouch;
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Lantern") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Lantern;
				}
			});
			ItemBrowserAPI.RegisterItemFilter(equipmentGroup, new($"{equipmentGroup}_Pet") {
				Function = objectData => PugDatabase.HasComponent<PetCD>(objectData)
			});

			// Utility
			const string utilityGroup = "ItemBrowser:ItemFilter/Utility";
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Placeable") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.PlaceablePrefab
					       && PugDatabase.TryGetComponent<ObjectPropertiesCD>(objectData, out var properties)
					       && properties.Has(PropertyID.PlaceableObject.placeableObject);
				}
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Consumable") {
				Function = objectData => PugDatabase.HasComponent<GivesConditionsWhenConsumedBuffer>(objectData)
				                         || (PugDatabase.TryGetComponent<CastItemCD>(objectData, out var castItem) && castItem.useType != CastItemUseType.LeashCattle)
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Material") {
				Function = objectData => false
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Craftable") {
				Function = objectData => {
					var craftingHandler = Manager.main.player?.playerCraftingHandler;
					if (craftingHandler == null)
						return false;

					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					if (objectInfo == null)
						return false;

					if (!ItemBrowserAPI.ObjectEntries.GetEntries<Crafting>(ObjectEntryType.Source, objectData).Any())
						return false;

					var nearbyChests = craftingHandler.GetNearbyChests();
					var recipeInfo = new CraftingHandler.RecipeInfo(objectInfo, 1);

					return craftingHandler.HasMaterialsInCraftingInventoryToCraftRecipe(recipeInfo, true, nearbyChests, true);
				},
				FunctionIsDynamic = true,
				CausesItemCraftingRequirementsToDisplay = true
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_CookingIngredient") {
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					return objectInfo.tags.Contains(ObjectCategoryTag.CookingIngredient);
				}
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Sand") {
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					return objectInfo.tags.Contains(ObjectCategoryTag.Sand);
				}
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_CattlePlantFood") {
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					return objectInfo.tags.Contains(ObjectCategoryTag.CattlePlantFood);
				}
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Insect") {
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					return objectInfo.tags.Contains(ObjectCategoryTag.Insect);
				}
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_CattleKelpFood") {
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					return objectInfo.tags.Contains(ObjectCategoryTag.CattleKelpFood);
				}
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Paintable") {
				Function = objectData => PugDatabase.HasComponent<PaintableObjectCD>(objectData)
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Discovered") {
				Function = objectData => Manager.saves.HasDiscoveredObject(objectData.objectID, objectData.variation),
				FunctionIsDynamic = true
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Unobtainable") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					if (objectType == ObjectType.NonObtainable)
						return true;

					if (PugDatabase.HasComponent<IndestructibleCD>(objectData))
						return true;

					if (PugDatabase.TryGetComponent<HealthCD>(objectData, out var health) && health.maxHealth >= 1000000)
						return true;

					if (PugDatabase.HasComponent<DontSerializeCD>(objectData) && objectType != ObjectType.Creature && objectType != ObjectType.Critter && !PugDatabase.HasComponent<TileCD>(objectData))
						return true;

					if (PugDatabase.GetComponent<TileCD>(objectData).tileType == TileType.ground)
						return true;

					if (ObjectUtils.GetLocalizedDisplayName(objectData.objectID, objectData.variation) == null)
						return true;

					return false;
				}
			});

			// Rarity
			const string rarityGroup = "ItemBrowser:ItemFilter/Rarity";
			foreach (var rarity in Enum.GetValues(typeof(Rarity)).Cast<Rarity>()) {
				ItemBrowserAPI.RegisterItemFilter(rarityGroup, new($"{rarityGroup}_{rarity}") {
					Function = objectData => PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).rarity == rarity
				});
			}

			/* Level
			const string levelGroup = "ItemBrowser:ItemFilter/Level";
			for (var i = 1; i <= LevelScaling.GetMaxLevel(); i++) {
				var level = i;
				ItemBrowserAPI.RegisterItemFilter(levelGroup, new($"{levelGroup}_Level") {
					NameFormatFields = new[] { i.ToString() },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { i.ToString() },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => ObjectUtils.GetLevel(objectData) == level
				});
			}*/

			// Version added
			const string versionGroup = "ItemBrowser:ItemFilter/VersionAdded";
			ItemBrowserAPI.RegisterItemFilter(versionGroup, new($"{versionGroup}_Version") {
				NameFormatFields = new[] { "1.1" },
				LocalizeNameFormatFields = false,
				DescriptionFormatFields = new[] { "1.1" },
				LocalizeDescriptionFormatFields = false,
				Function = objectData => ObjectsAddedByVersion.In11.Contains(objectData.objectID)
			});
			ItemBrowserAPI.RegisterItemFilter(versionGroup, new($"{versionGroup}_Version") {
				NameFormatFields = new[] { "1.1.1" },
				LocalizeNameFormatFields = false,
				DescriptionFormatFields = new[] { "1.1.1" },
				LocalizeDescriptionFormatFields = false,
				Function = objectData => ObjectsAddedByVersion.In111.Contains(objectData.objectID)
			});
			ItemBrowserAPI.RegisterItemFilter(versionGroup, new($"{versionGroup}_Version") {
				NameFormatFields = new[] { "1.1.2" },
				LocalizeNameFormatFields = false,
				DescriptionFormatFields = new[] { "1.1.2" },
				LocalizeDescriptionFormatFields = false,
				Function = objectData => ObjectsAddedByVersion.In112.Contains(objectData.objectID)
			});
		}
	}
}