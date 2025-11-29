using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser;
using ItemBrowser.Entries;
using ItemBrowser.Config;
using ItemBrowser.Entries.Defaults;
using ItemBrowser.Utilities.DataStructures;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
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
	
	internal static AssetBundle AssetBundle { get; private set; }
	
	public void EarlyInit() {
		Debug.Log($"[{DisplayName}]: Mod version: {Version}");

		var modInfo = API.ModLoader.LoadedMods.FirstOrDefault(modInfo => modInfo.Handlers.Contains(this));
		AssetBundle = modInfo!.AssetBundles[0];
		
		Options.EarlyInit();
		
		ItemBrowserAPI.OnInit += BuiltinContent.Register;
	}

	public void Init() {
		Options.Init();
		
		ModUtils.InitOnModLoad();
	}

	public void Shutdown() { }

	public void Update() { }

	public void ModObjectLoaded(Object obj) {
		BuiltinContent.OnModObjectLoaded(obj);
	}

	public static void Log(string context, string text) {
		Debug.Log($"[ItemBrowser]: ({context}) {text}");
	}
	
	public static void Log(Exception ex) {
		Debug.LogException(ex);
	}
	
	private static class BuiltinContent {
		private static readonly HashSet<FactionID> UnusedCreatureFactions = new() {
			FactionID.AttacksAllButNotPlayer,
			FactionID.PlayerMinion,
			FactionID.Explosion,
			FactionID.__MAX_VALUE
		};
		
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
				new Loot.Provider(),
				new ChallengeArenaReward.Provider(),
				new ItemBrowser.Entries.Defaults.VendingMachine.Provider(),
				new StructureContents.Provider(),
				new NaturalSpawnAroundObject.Provider(),
				new TerrainGeneration.Provider(),
				new MerchantSpawning.Provider(),
				new Miscellaneous.Provider(),
				new Breeding.Provider(),
				new CreatureSummoning.Provider(),
				new UpgradeMaterial.Provider(),
				new DropsWhenDamaged.Provider(),
				new Unlocking.Provider()
			);

			RegisterSorters();
			RegisterFilters();
		}

		public static void OnModObjectLoaded(Object obj) {
			if (obj is not GameObject gameObject)
				return;
			
			if (gameObject.TryGetComponent<ObjectEntryDisplayBase>(out var displayComponent))
				ItemBrowserAPI.RegisterObjectEntryDisplay(displayComponent);

			foreach (var overrides in gameObject.GetComponents(typeof(ObjectNameAndIconOverride)))
				ItemBrowserAPI.RegisterObjectNameAndIconOverride((ObjectNameAndIconOverride) overrides);
		}

		private static void RegisterSorters() {
			// Item sorters
			ItemBrowserAPI.RegisterItemSorter(new("ItemBrowser:Sorters/Alphabetical") {
				Function = objectData => -ObjectUtils.GetDisplayNameSortOrder(objectData.objectID, objectData.variation)
			});
			/*RegisterItemSorter(new("ItemBrowser:Sorters/Category") {
				Function = objectData => -ObjectUtils.GetDisplayNameScore(objectData.objectID, objectData.variation)
			});*/
			ItemBrowserAPI.RegisterItemSorter(new("ItemBrowser:Sorters/InternalIndex") {
				Function = objectData => (int) objectData.objectID * 10000 + objectData.variation
			});
			ItemBrowserAPI.RegisterItemSorter(new("ItemBrowser:Sorters/Damage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation)
			});
			ItemBrowserAPI.RegisterItemSorter(new("ItemBrowser:Sorters/Level") {
				Function = objectData => ObjectUtils.GetBaseLevel(objectData.objectID, objectData.variation)
			});
			ItemBrowserAPI.RegisterItemSorter(new("ItemBrowser:Sorters/Value") {
				Function = objectData => ObjectUtils.GetValue(objectData.objectID, objectData.variation)
			});
			
			// Creature sorters
			ItemBrowserAPI.RegisterCreatureSorter(new("ItemBrowser:Sorters/Alphabetical") {
				Function = objectData => -ObjectUtils.GetDisplayNameSortOrder(objectData.objectID, objectData.variation)
			});
			ItemBrowserAPI.RegisterCreatureSorter(new("ItemBrowser:Sorters/InternalIndex") {
				Function = objectData => (int) objectData.objectID * 10000 + objectData.variation
			});
			ItemBrowserAPI.RegisterCreatureSorter(new("ItemBrowser:Sorters/Level") {
				Function = objectData => ObjectUtils.GetBaseLevel(objectData.objectID, objectData.variation)
			});
		}
		
		private static void RegisterFilters() {
			// Source
			const string sourceGroup = "ItemBrowser:Filters/Source";
			ItemBrowserAPI.RegisterItemFilter(sourceGroup, new($"{sourceGroup}_Item_FromMods") {
				Function = ModUtils.IsModded
			});
			ItemBrowserAPI.RegisterCreatureFilter(sourceGroup, new($"{sourceGroup}_Creature_FromMods") {
				Function = objectData => (int) objectData.objectID > Constants.maxNonModdedObjectID
			});
			foreach (var mod in API.ModLoader.LoadedMods.OrderBy(mod => ModUtils.GetDisplayName(mod.ModId))) {
				var displayName = ModUtils.GetDisplayName(mod.ModId);
				
				var objectIds = ModUtils.GetAssociatedObjects(mod.ModId);
				var itemIds = objectIds
					.Where(ItemBrowserAPI.ShouldItemBeIncluded)
					.ToList();
				var creatureIds = objectIds
					.Where(ItemBrowserAPI.ShouldItemBeIncluded)
					.ToList();

				if (itemIds.Count > 0) {
					ItemBrowserAPI.RegisterItemFilter(sourceGroup, new($"{sourceGroup}_Item_FromMod") {
						NameFormatFields = new[] { displayName },
						LocalizeNameFormatFields = false,
						DescriptionFormatFields = new[] { displayName },
						LocalizeDescriptionFormatFields = false,
						Function = objectData => itemIds.Contains(objectData)
					});
				}
				
				if (creatureIds.Count > 0) {
					ItemBrowserAPI.RegisterCreatureFilter(sourceGroup, new($"{sourceGroup}_Creature_FromMod") {
						NameFormatFields = new[] { displayName },
						LocalizeNameFormatFields = false,
						DescriptionFormatFields = new[] { displayName },
						LocalizeDescriptionFormatFields = false,
						Function = objectData => creatureIds.Contains(objectData)
					});	
				}
			}

			// Item damage
			const string damageGroup = "ItemBrowser:Filters/Damage";
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_AnyDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation) > 0
			});
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_PhysicalMeleeDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation, ObjectUtils.DamageCategory.PhysicalMelee) > 0
			});
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_PhysicalRangeDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation, ObjectUtils.DamageCategory.PhysicalRange) > 0
			});
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_MagicDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation, ObjectUtils.DamageCategory.Magic) > 0
			});
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_SummonDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation, ObjectUtils.DamageCategory.Summon) > 0
			});
			ItemBrowserAPI.RegisterItemFilter(damageGroup, new($"{damageGroup}_ExplosiveDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation, ObjectUtils.DamageCategory.Explosive) > 0
			});

			// Item equipment
			const string equipmentGroup = "ItemBrowser:Filters/Equipment";
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
			
			// Creature type
			const string typeGroup = "ItemBrowser:Filters/Type";
			ItemBrowserAPI.RegisterCreatureFilter(typeGroup, new($"{typeGroup}_Hostile") {
				Function = objectData => !ObjectCategoryTagsCD.HasTag(PugDatabase.GetComponent<ObjectCategoryTagsCD>(objectData).tagsBitMask, ObjectCategoryTag.NonHostileCreature)
					&& !PugDatabase.HasComponent<CattleCD>(objectData)
					&& !PugDatabase.HasComponent<CritterCD>(objectData)
					&& !PugDatabase.HasComponent<MerchantCD>(objectData)
			});
			ItemBrowserAPI.RegisterCreatureFilter(typeGroup, new($"{typeGroup}_Boss") {
				Function = objectData => PugDatabase.HasComponent<BossCD>(objectData) || ObjectUtils.GetCategories(objectData.objectID).Contains("Boss/BossCreature")
			});
			ItemBrowserAPI.RegisterCreatureFilter(typeGroup, new($"{typeGroup}_Merchant") {
				Function = PugDatabase.HasComponent<MerchantCD>
			});
			ItemBrowserAPI.RegisterCreatureFilter(typeGroup, new($"{typeGroup}_Cattle") {
				Function = PugDatabase.HasComponent<CattleCD>
			});
			ItemBrowserAPI.RegisterCreatureFilter(typeGroup, new($"{typeGroup}_Critter") {
				Function = PugDatabase.HasComponent<CritterCD>
			});

			// Utility
			const string utilityGroup = "ItemBrowser:Filters/Utility";
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
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Paintable") {
				Function = objectData => PugDatabase.HasComponent<PaintableObjectCD>(objectData)
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Discovered") {
				Function = objectData => ObjectUtils.HasBeenDiscovered(objectData.objectID, objectData.variation),
				FunctionIsDynamic = true,
				DefaultState = () => Options.DefaultDiscoveredFilter ? FilterState.Include : FilterState.None
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_Technical_Item") {
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					
					if (PugDatabase.HasComponent<ProjectileCD>(objectData))
						return true;
					
					if (PugDatabase.TryGetComponent<TileCD>(objectData, out var tileCD) && tileCD.tileType == TileType.ground)
						return true;

					if (ObjectUtils.GetLocalizedDisplayName(objectData.objectID, objectData.variation) == null)
						return true;
					
					if (objectInfo.objectType == ObjectType.NonObtainable && !PugDatabase.HasComponent<DestructibleObjectCD>(objectData) && !PugDatabase.HasComponent<SoulOrbCD>(objectData))
						return true;
					
					return false;
				},
				DefaultState = () => Options.DefaultTechnicalFilter ? FilterState.Exclude : FilterState.None
			});
			ItemBrowserAPI.RegisterCreatureFilter(utilityGroup, new($"{utilityGroup}_Technical_Creature") {
				Function = objectData => {
					if (PugDatabase.HasComponent<MinionCD>(objectData))
						return true;

					if (ObjectUtils.GetLocalizedDisplayName(objectData.objectID, objectData.variation) == null)
						return true;
					
					return false;
				},
				DefaultState = () => Options.DefaultTechnicalFilter ? FilterState.Exclude : FilterState.None
			});
			ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_NoSources_Item") {
				Function = objectData => !ItemBrowserAPI.ObjectEntries.GetAllEntries(ObjectEntryType.Source, objectData).Any(),
				DefaultState = () => Options.CheatMode ? FilterState.None : FilterState.Exclude
			});
			ItemBrowserAPI.RegisterCreatureFilter(utilityGroup, new($"{utilityGroup}_NoSources_Creature") {
				Function = objectData => !ItemBrowserAPI.ObjectEntries.GetAllEntries(ObjectEntryType.Source, objectData).Any(),
				DefaultState = () => Options.CheatMode ? FilterState.None : FilterState.Exclude
			});
			/*ItemBrowserAPI.RegisterItemFilter(utilityGroup, new($"{utilityGroup}_IsNonObtainable") {
				Function = objectData => ObjectUtils.IsNonObtainable(objectData.objectID, objectData.variation)
			});*/

			// Creature faction
			const string factionGroup = "ItemBrowser:Filters/Faction";
			foreach (var faction in Enum.GetValues(typeof(FactionID)).Cast<FactionID>()) {
				if (UnusedCreatureFactions.Contains(faction))
					continue;
				
				ItemBrowserAPI.RegisterCreatureFilter(factionGroup, new($"ItemBrowser:FactionNames/{faction}", $"{factionGroup}_FactionDesc") {
					DescriptionFormatFields = new[] { faction.ToString() },
					Function = objectData => PugDatabase.TryGetComponent<FactionCD>(objectData, out var factionCD) && factionCD.faction == faction
				});
			}

			// Item rarity
			const string rarityGroup = "ItemBrowser:Filters/Rarity";
			foreach (var rarity in Enum.GetValues(typeof(Rarity)).Cast<Rarity>()) {
				ItemBrowserAPI.RegisterItemFilter(rarityGroup, new($"{rarityGroup}_{rarity}") {
					Function = objectData => PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).rarity == rarity
				});
			}

			// Item level
			const string levelGroup = "ItemBrowser:Filters/Level";
			for (var i = 1; i <= LevelScaling.GetMaxLevel(); i++) {
				var level = i;
				ItemBrowserAPI.RegisterItemFilter(levelGroup, new($"{levelGroup}_Level") {
					NameFormatFields = new[] { i.ToString() },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { i.ToString() },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => ObjectUtils.GetBaseLevel(objectData.objectID, objectData.variation) == level
				});
			}

			// Version added
			const string versionGroup = "ItemBrowser:Filters/VersionAdded";
			foreach (var version in ObjectsAddedByVersion.AllVersions) {
				if (version.HasAnyItems) {
					ItemBrowserAPI.RegisterItemFilter(versionGroup, new Filter<ObjectDataCD>($"{versionGroup}_Item_Version") {
						NameFormatFields = new[] { version.Name },
						LocalizeNameFormatFields = false,
						DescriptionFormatFields = new[] { version.Name },
						LocalizeDescriptionFormatFields = false,
						Function = objectData => version.Objects.Contains(objectData.objectID)
					});	
				}
				if (version.HasAnyCreatures) {
					ItemBrowserAPI.RegisterCreatureFilter(versionGroup, new Filter<ObjectDataCD>($"{versionGroup}_Creature_Version") {
						NameFormatFields = new[] { version.Name },
						LocalizeNameFormatFields = false,
						DescriptionFormatFields = new[] { version.Name },
						LocalizeDescriptionFormatFields = false,
						Function = objectData => version.Objects.Contains(objectData.objectID)
					});
				}
			}
		}
	}
}