using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.Extensions;
using PugMod;
using Unity.Mathematics;
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
					
					if (craftingCD.craftingType != CraftingType.Simple && craftingCD.craftingType != CraftingType.ProcessResources && craftingCD.craftingType != CraftingType.BossStatue)
						continue;

					if (ObjectUtils.GetLocalizedDisplayName(objectData.objectID, objectData.variation) == null)
						continue;

					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					if (objectInfo.objectType == ObjectType.Creature)
						continue;
					
					// need to check if this is actually a crafting station, signs have a CraftingAuthoring for some reason
					var graphicalPrefab = objectInfo.prefabInfos[0].prefab.gameObject;
					if (graphicalPrefab == null || !graphicalPrefab.TryGetComponent<EntityMonoBehaviour>(out var entityMono) || (entityMono is not CraftingBuilding && entityMono is not PlayerController))
						continue;
					
					var canCraftObjects = PugDatabase.GetBuffer<CanCraftObjectsBuffer>(objectData);
					var canCraftObjectsIndexesToInclude = new HashSet<int>();
					
					if (PugDatabase.HasComponent<IncludedCraftingBuildingsBuffer>(objectData)) {
						var includedCraftingBuildings = PugDatabase.GetBuffer<IncludedCraftingBuildingsBuffer>(objectData).ConvertToList();
						var endIndex = 0;

						if (includedCraftingBuildings.Count > 0) {
							endIndex = includedCraftingBuildings[0].amountOfCraftingOptions - 1;
							for (var i = 0; i <= endIndex; i++)
								canCraftObjectsIndexesToInclude.Add(i);
						}

						if (includedCraftingBuildings.Count > 1) {
							foreach (var includedCraftingBuilding in includedCraftingBuildings.Skip(1)) {
								if (ObjectUtils.GetLocalizedDisplayName(includedCraftingBuilding.objectID) == null) {
									Main.Log(nameof(Crafting), $"{ObjectUtils.GetInternalName(objectData.objectID)} can craft {string.Join(',', Enumerable.Range(endIndex + 1, endIndex + includedCraftingBuilding.amountOfCraftingOptions - 1))} from {ObjectUtils.GetInternalName(includedCraftingBuilding.objectID)}");
									for (var i = endIndex + 1; i < endIndex + includedCraftingBuilding.amountOfCraftingOptions; i++)
										canCraftObjectsIndexesToInclude.Add(i);	
								}
								endIndex = math.max(endIndex, includedCraftingBuilding.amountOfCraftingOptions - 1);
							}
						}
					} else {
						for (var i = 0; i < canCraftObjects.Length; i++)
							canCraftObjectsIndexesToInclude.Add(i);
					}
					
					foreach (var i in canCraftObjectsIndexesToInclude) {
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