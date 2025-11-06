using System.Collections.Generic;
using System.Globalization;
using I2.Loc;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class ObjectUtils {
		public static readonly IEnumerable<ObjectType> CreatureObjectTypes = new List<ObjectType> {
			ObjectType.Creature,
			ObjectType.Pet,
			ObjectType.PlayerType
		};
		public static readonly IEnumerable<ObjectType> ArmorObjectTypes = new List<ObjectType> {
			ObjectType.Helm,
			ObjectType.BreastArmor,
			ObjectType.PantsArmor
		};
		public static readonly IEnumerable<ObjectType> AccessoryObjectTypes = new List<ObjectType> {
			ObjectType.Ring,
			ObjectType.Necklace,
			ObjectType.Offhand,
			ObjectType.Lantern
		};
		public static readonly IEnumerable<ObjectType> ToolObjectTypes = new List<ObjectType> {
			ObjectType.MiningPick,
			ObjectType.Sledge,
			ObjectType.DrillTool,
			ObjectType.BeamWeapon,
			ObjectType.Shovel,
			ObjectType.FishingRod,
			ObjectType.Hoe,
			ObjectType.PaintTool,
			ObjectType.Bucket,
			ObjectType.RoofingTool
		};
		public static readonly IEnumerable<ObjectType> MiningToolObjectTypes = new List<ObjectType> {
			ObjectType.MiningPick,
			ObjectType.Sledge,
			ObjectType.DrillTool,
			ObjectType.BeamWeapon
		};
		
		private static readonly Dictionary<ObjectDataCD, string> DisplayNames = new();
		private static readonly Dictionary<ObjectDataCD, string> DisplayNameTerms = new();
		private static readonly Dictionary<ObjectDataCD, int> DisplayNameSortOrders = new();
		private static readonly Dictionary<ObjectID, HashSet<string>> Categories = new();

		internal static void InitOnWorldLoad() {
			SetupDisplayNamesAndCategories();
		}
		
		private static void SetupDisplayNamesAndCategories() {
			DisplayNames.Clear();
			DisplayNameTerms.Clear();
			
			var authoringList = new List<MonoBehaviour>();
			authoringList.AddRange(Manager.ecs.pugDatabase.prefabList);
			authoringList.AddRange(Manager.mod.ExtraAuthoring);
			
			foreach (var authoring in authoringList) {
				var gameObject = authoring.gameObject;
				if (!gameObject.TryGetComponent<IEntityMonoBehaviourData>(out var entityMonoBehaviourData))
					continue;

				var objectInfo = entityMonoBehaviourData.ObjectInfo;
				var objectData = new ObjectDataCD {
					objectID = objectInfo.objectID,
					variation = objectInfo.variation
				};
				
				string localizedName;
				string unlocalizedName;
				if (ItemBrowserAPI.ObjectNameOverrides.TryGetValue(objectData, out var term)) {
					localizedName = LocalizationManager.GetTranslation(term);
					unlocalizedName = term;
				} else {
					localizedName = PlayerController.GetObjectName(new ContainedObjectsBuffer {
						objectData = objectData
					}, true).text;
					unlocalizedName = PlayerController.GetObjectName(new ContainedObjectsBuffer {
						objectData = objectData
					}, false).text;
				}

				DisplayNames.TryAdd(objectData, localizedName);
				DisplayNameTerms.TryAdd(objectData, unlocalizedName);
			}
			
			DisplayNameSortOrders.Clear();
			var objectNames = new List<(ObjectDataCD ObjectData, string Name)>();
			
			foreach (var authoring in authoringList) {
				var gameObject = authoring.gameObject;
				if (!gameObject.TryGetComponent<IEntityMonoBehaviourData>(out var entityMonoBehaviourData))
					continue;

				var objectInfo = entityMonoBehaviourData.ObjectInfo;
				var objectData = new ObjectDataCD {
					objectID = objectInfo.objectID,
					variation = objectInfo.variation
				};
				var objectName = DisplayNames.GetValueOrDefault(objectData);
				objectName ??= $"ZZZ+{objectData.objectID}+{objectData.variation}";

				objectNames.Add((objectData, objectName));
			}
			
			objectNames.Sort((a, b) => string.Compare(a.Name, b.Name, LocalizationManager.CurrentCulture, CompareOptions.StringSort));

			for (var i = 0; i < objectNames.Count; i++) {
				var (objectData, _) = objectNames[i];
				DisplayNameSortOrders.TryAdd(objectData, i);
			}
			
			Categories.Clear();
			foreach (var subCategory in ObjectIDCategoryManager.SubCategories) {
				foreach (var objectId in subCategory.ObjectIds) {
					if (!Categories.ContainsKey(objectId))
						Categories[objectId] = new HashSet<string>();

					Categories[objectId].Add(subCategory.ToString());
				}
			}
		}
		
		public static string GetLocalizedDisplayName(ObjectID id, int variation = 0) {
			return DisplayNames.GetValueOrDefault(new ObjectDataCD {
				objectID = id,
				variation = variation
			});
		}
		
		public static string GetUnlocalizedDisplayName(ObjectID id, int variation = 0) {
			return DisplayNameTerms.GetValueOrDefault(new ObjectDataCD {
				objectID = id,
				variation = variation
			});
		}
		
		public static int GetDisplayNameSortOrder(ObjectID id, int variation = 0) {
			return DisplayNameSortOrders.GetValueOrDefault(new ObjectDataCD {
				objectID = id,
				variation = variation
			});
		}
		
		public static HashSet<string> GetCategories(ObjectID id) {
			return Categories.GetValueOrDefault(id);
		}

		public static Sprite GetIcon(ObjectID id, int variation = 0, bool preferSmallIcons = false) {
			var objectInfo = PugDatabase.GetObjectInfo(id, variation);
			if (objectInfo == null)
				return null;

			var objectData = new ObjectData {
				objectID = id,
				variation = variation
			};
			
			var iconToUse = preferSmallIcons ? (objectInfo.smallIcon ?? objectInfo.icon) : objectInfo.icon;
			var iconOverride = Manager.ui.itemOverridesTable.GetIconOverride(objectData, preferSmallIcons);
			if (iconOverride != null)
				iconToUse = iconOverride;
			
			if (ItemBrowserAPI.ObjectIconOverrides.TryGetValue(objectData, out iconOverride))
				iconToUse = iconOverride;

			return iconToUse;
		}
		
		public static int GetDamage(ObjectID id, int variation = 0) {
			var objectData = new ObjectData {
				objectID = id,
				variation = variation
			};
			
			if (PugDatabase.TryGetComponent<SecondaryUseCD>(objectData, out var secondaryUseCD) && secondaryUseCD.summonsMinion && PugDatabase.TryGetComponent<LevelCD>(objectData, out var levelCD) && PugDatabase.TryGetComponent<MinionCD>(secondaryUseCD.minionToSpawn, out var minionCD))
				return MinionExtensions.GetMinionBaseDamage(minionCD, levelCD.level);

			var levelEntity = EntityUtility.GetLevelEntity(objectData);
			if (levelEntity != Entity.Null && EntityUtility.TryGetComponentData<WeaponDamageCD>(levelEntity, Manager.ecs.ClientWorld, out var weaponDamageCD))
				return weaponDamageCD.GetDamage(false);

			return 0;
		}

		public static int GetBaseLevel(ObjectID id, int variation = 0) {
			var objectData = new ObjectData {
				objectID = id,
				variation = variation
			};
			return PugDatabase.TryGetComponent<LevelCD>(objectData, out var levelCD) ? levelCD.level : 0;
		}

		// from InventoryUtility.GetRaritySellValue
		public static int GetValue(ObjectID id, int variation = 0, bool buy = false) {
			var objectData = new ObjectData {
				objectID = id,
				variation = variation
			};
			
			var objectInfo = PugDatabase.GetObjectInfo(id, variation);
			if (objectData.objectID == ObjectID.None || PugDatabase.HasComponent<CantBeSoldAuthoring>(objectData) || objectInfo.rarity == Rarity.Legendary)
				return 0;

			var sellValue = objectInfo.sellValue;
			if (sellValue < 0) {
				sellValue = GetRaritySellValue(objectInfo.rarity);
				
				if (PugDatabase.HasComponent<CookedFoodAuthoring>(objectData)) {
					var primaryIngredient = CookedFoodCD.GetPrimaryIngredientFromVariation(objectData.variation);
					var secondaryIngredient = CookedFoodCD.GetSecondaryIngredientFromVariation(objectData.variation);
					sellValue = GetValue(primaryIngredient, 0, buy) + GetValue(secondaryIngredient, 0, buy);
				} else {
					var extraSellFromIngredients = 0;
					var requiredObjectsToCraft = objectInfo.requiredObjectsToCraft;

					foreach (var craftingObject in requiredObjectsToCraft) {
						var ingredientObjectInfo = PugDatabase.GetObjectInfo(craftingObject.objectID);
						if (ingredientObjectInfo.sellValue != 0)
							extraSellFromIngredients += GetRaritySellValue(ingredientObjectInfo.rarity) * craftingObject.amount;
					}

					if (extraSellFromIngredients > 0)
						sellValue = (int) math.round(math.max(1f, sellValue * 0.3f) + extraSellFromIngredients);
				}

				var randomization = Unity.Mathematics.Random.CreateFromIndex((uint)objectData.objectID).NextFloat(-0.1f, 0.1f);
				sellValue = math.max(1, sellValue + (int) math.round(sellValue * randomization));
			}

			if (buy) {
				sellValue = math.max(1, sellValue);
				var buyValueMultiplier = objectInfo.buyValueMultiplier;
				return (int) math.round(sellValue * 5f * buyValueMultiplier);
			}

			return sellValue;
		}

		// from InventoryUtility.GetRaritySellValue
		private static int GetRaritySellValue(Rarity rarity) {
			return 1 + math.max(0, (int) rarity) * 5;
		}
	}
}