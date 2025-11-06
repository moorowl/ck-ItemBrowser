using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class Crafting : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/Crafting", ObjectID.WoodenWorkBench, 4500);

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
					
					for (var i = startCanCraftObjectsIndex; i <= endCanCraftObjectsIndex; i++) {
						var entry = canCraftObjects[i];
						var entryObjectInfo = PugDatabase.GetObjectInfo(entry.objectID);
						if (entryObjectInfo == null)
							continue;
						
						registry.Register(entry.objectID, 0, new Crafting {
							Station = objectData.objectID,
							Amount = Math.Max(entry.amount, 1),
							CraftingTime = craftingCD.craftingType == CraftingType.ProcessResources ? entryObjectInfo.craftingTime : 0f
						});
					}
				}
				
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<ParchmentRecipeCD>(objectData, out var parchmentRecipe))
						continue;

					var castTime = PugDatabase.TryGetComponent<CooldownCD>(objectData, out var cooldown) ? cooldown.cooldown : 0f;
					var objectToCraft = parchmentRecipe.objectToCraft;
					registry.Register(objectToCraft.objectID, objectToCraft.variation, new Crafting {
						Recipe = objectData.objectID,
						Amount = Math.Max(objectToCraft.amount, 1),
						CraftingTime = castTime,
						RequiresObjectNearby = parchmentRecipe.requiresNearbyObject
					});
				}
			}
		}
	}
}