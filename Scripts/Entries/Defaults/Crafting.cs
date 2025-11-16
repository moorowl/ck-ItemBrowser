using System;
using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class Crafting : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Crafting", ObjectID.WoodenWorkBench, 4500);

		public (ObjectID Id, int Variation) Result { get; protected set; }
		public ObjectID Station { get; protected set; }
		public ObjectID Recipe { get; protected set; }
		public int Amount { get; protected set; }
		public float CraftingTime { get; protected set; }
		public ObjectID RequiresObjectNearby { get; protected set; }
		
		public bool UsesStation => Station != ObjectID.None;
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<CraftingCD>(objectData, out var craftingCD) || !PugDatabase.HasComponent<CanCraftObjectsBuffer>(objectData))
						continue;

					if (ObjectUtils.GetLocalizedDisplayName(objectData.objectID, objectData.variation) == null)
						continue;

					if (craftingCD.craftingType != CraftingType.Simple && craftingCD.craftingType != CraftingType.ProcessResources && craftingCD.craftingType != CraftingType.BossStatue)
						continue;

					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					if (objectInfo.objectType == ObjectType.Creature)
						continue;
					
					// need to check if this is actually a crafting station, signs have a CraftingAuthoring for some reason
					var graphicalPrefab = objectInfo.prefabInfos[0].prefab.gameObject;
					if (graphicalPrefab == null || !graphicalPrefab.TryGetComponent<EntityMonoBehaviour>(out var entityMono) || (entityMono is not CraftingBuilding && entityMono is not PlayerController))
						continue;
					
					var canCraftObjects = PugDatabase.GetBuffer<CanCraftObjectsBuffer>(objectData);
					var startCanCraftObjectsIndex = 0;
					var endCanCraftObjectsIndex = canCraftObjects.Length - 1;
					
					if (PugDatabase.HasComponent<IncludedCraftingBuildingsBuffer>(objectData)) {
						var includedCraftingBuildings = PugDatabase.GetBuffer<IncludedCraftingBuildingsBuffer>(objectData);

						if (includedCraftingBuildings.Length > 0)
							endCanCraftObjectsIndex = includedCraftingBuildings[0].amountOfCraftingOptions - 1;
					}

					Main.Log(nameof(Crafting), $"{objectData.objectID} can craft {startCanCraftObjectsIndex}-{endCanCraftObjectsIndex}");
					
					for (var i = startCanCraftObjectsIndex; i <= endCanCraftObjectsIndex; i++) {
						var canCraftObject = canCraftObjects[i];
						var canCraftObjectInfo = PugDatabase.GetObjectInfo(canCraftObject.objectID);
						if (canCraftObjectInfo == null)
							continue;

						var entry = new Crafting {
							Result = (canCraftObject.objectID, 0),
							Station = objectData.objectID,
							Amount = Math.Max(canCraftObject.amount, 1),
							CraftingTime = craftingCD.craftingType == CraftingType.ProcessResources ? canCraftObjectInfo.craftingTime : 0f
						};
						registry.Register(ObjectEntryType.Source, canCraftObject.objectID, 0, entry);
						registry.Register(ObjectEntryType.Usage, objectData.objectID, 0, entry);
						foreach (var ingredient in ObjectUtils.GroupAndSumObjects(canCraftObjectInfo.requiredObjectsToCraft))
							registry.Register(ObjectEntryType.Usage, ingredient.objectID, 0, entry);
						foreach (var ingredient in ObjectUtils.GetAllObjectsWithTag(canCraftObjectInfo.craftingSettings.canOnlyUseAnyMaterialsWithTag))
							registry.Register(ObjectEntryType.Usage, ingredient.objectID, 0, entry);
					}
				}
				
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<ParchmentRecipeCD>(objectData, out var parchmentRecipe))
						continue;

					var castTime = PugDatabase.TryGetComponent<CooldownCD>(objectData, out var cooldown) ? cooldown.cooldown : 0f;
					var objectToCraft = parchmentRecipe.objectToCraft;
					var objectToCraftInfo = PugDatabase.GetObjectInfo(objectToCraft.objectID, objectToCraft.variation);

					var entry = new Crafting {
						Result = (objectToCraft.objectID, objectToCraft.variation),
						Recipe = objectData.objectID,
						Amount = Math.Max(objectToCraft.amount, 1),
						CraftingTime = castTime,
						RequiresObjectNearby = parchmentRecipe.requiresNearbyObject
					};
					registry.Register(ObjectEntryType.Source, objectToCraft.objectID, objectToCraft.variation, entry);
					registry.Register(ObjectEntryType.Usage, objectData.objectID, 0, entry);
					foreach (var ingredient in ObjectUtils.GroupAndSumObjects(objectToCraftInfo.requiredObjectsToCraft))
						registry.Register(ObjectEntryType.Usage, ingredient.objectID, 0, entry);
				}
			}
		}
	}
}