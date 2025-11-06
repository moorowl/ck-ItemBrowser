using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.DataStructures;
using ItemBrowser.Utilities;
using PugMod;
using Unity.Entities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class Loot : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Loot", ObjectID.CopperChest, 3700);
		
		public ObjectID Entity { get; protected set; }
		public int EntityVariation  { get; protected set; }
		public float Chance { get; protected set; }
		public Func<float> ChanceForOne { get; protected set; }
		public Func<(int Min, int Max)> Amount { get; protected set; }
		public Func<(int Min, int Max)> Rolls { get; protected set; }
		public Biome OnlyDropsInBiome { get; protected set; }
		public bool IsFromGuaranteedPool { get; protected set; }
		public bool IsFromTableWithGuaranteedPool { get; protected set; }
		public List<(string Name, int Amount)> FoundInScenes { get; protected set; } = new();
		public List<(string Name, int Amount)> FoundInDungeons { get; protected set; } = new();
		
		protected bool Equals(Loot other) {
			return Entity == other.Entity
			       && EntityVariation == other.EntityVariation
			       && Mathf.Approximately(Chance, other.Chance)
			       && Mathf.Approximately(ChanceForOne(), other.ChanceForOne())
			       && Amount() == other.Amount()
			       && Rolls() == other.Rolls()
			       && OnlyDropsInBiome == other.OnlyDropsInBiome
			       && IsFromGuaranteedPool == other.IsFromGuaranteedPool
			       && IsFromTableWithGuaranteedPool == other.IsFromTableWithGuaranteedPool;
		}

		public override bool Equals(object obj) {
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((Loot) obj);
		}

		public override int GetHashCode() {
			var hashCode = new HashCode();
			hashCode.Add((int) Entity);
			hashCode.Add(EntityVariation);
			hashCode.Add(Chance);
			hashCode.Add(ChanceForOne());
			hashCode.Add(Amount());
			hashCode.Add(Rolls());
			hashCode.Add((int) OnlyDropsInBiome);
			hashCode.Add(IsFromGuaranteedPool);
			hashCode.Add(IsFromTableWithGuaranteedPool);
			return hashCode.ToHashCode();
		}
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var entriesToAdd = new List<(ObjectID Id, int Variation, Loot Entry)>();
				var pugDatabaseBankBlob = API.Client.GetEntityQuery(typeof(PugDatabase.DatabaseBankCD)).GetSingleton<PugDatabase.DatabaseBankCD>().databaseBankBlob;

				void AddNormalEntry(ObjectID id, int variation, Loot entry) {
					entriesToAdd.Add((id, variation, entry));
				}
				
				void AddEntryFromScene(ObjectID id, int variation, string sceneName, Loot entry) {
					foreach (var existingEntry in entriesToAdd) {
						if (existingEntry.Id == id && existingEntry.Variation == variation && existingEntry.Entry.Equals(entry) && existingEntry.Entry.FoundInScenes.Count > 0) {
							var existingScene = existingEntry.Entry.FoundInScenes.FirstOrDefault(x => x.Name == sceneName);
							if (existingScene.Name != null)
								existingScene.Amount += 1;
							else
								existingEntry.Entry.FoundInScenes.Add((sceneName, 1));
							
							return;
						}
					}
					
					entry.FoundInScenes.Add((sceneName, 1));
					AddNormalEntry(id, variation, entry);
				}
				
				void AddNormalOrSceneEntry(ObjectID id, int variation, string sceneName, Loot entry) {
					if (sceneName == null)
						AddNormalEntry(id, variation, entry);
					else
						AddEntryFromScene(id, variation, sceneName, entry);
				}

				void AddEntriesFromPrefab(World world, ObjectDataCD objectData, Entity entity, string optionalSceneName = null) {
					if (EntityUtility.HasComponentData<InventoryBuffer>(entity, world) && EntityUtility.TryGetBuffer<ContainedObjectsBuffer>(entity, world, out var containedObjects)) {
						var groupedContainedObjects = containedObjects.ConvertToList()
							.GroupBy(entry => entry.objectID)
							.Select(group => {
								var entry = group.First();
								return new ObjectDataCD {
									objectID = entry.objectID,
									variation = entry.variation,
									amount = group.Sum(item => item.amount)
								};
							});

						foreach (var containedObject in groupedContainedObjects) {
							AddNormalOrSceneEntry(containedObject.objectID, containedObject.variation, optionalSceneName, new Loot {
								Entity = objectData.objectID,
								EntityVariation = objectData.variation,
								Amount = () => (containedObject.amount, containedObject.amount),
								Chance = 1f,
								ChanceForOne = () => 1f,
								Rolls = () => (1, 1)
							});
						}	
					}

					if (EntityUtility.TryGetComponentData<AddRandomLootCD>(entity, world, out var addRandomLootCD)) {
						foreach (var drop in LootUtils.GetLootTableContents(addRandomLootCD.lootTableID)) {
							AddNormalOrSceneEntry(drop.ObjectId, 0, optionalSceneName, new Loot {
								Entity = objectData.objectID,
								EntityVariation = objectData.variation,
								Chance = drop.Chance,
								ChanceForOne = () => drop.CalculateChanceForOne(),
								Amount = () => drop.ObjectAmount,
								Rolls = () => drop.CalculateRolls(),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
								IsFromTableWithGuaranteedPool = drop.TableHasGuaranteedPool
							});
						}
					}

					if (EntityUtility.TryGetComponentData<ChangeVariationWhenContainingObjectCD>(entity, world, out var changeVariationWhenContainingObjectCD)) {
						foreach (var drop in LootUtils.GetLootTableContents(changeVariationWhenContainingObjectCD.addLootFromTableToNewObject)) {
							AddNormalOrSceneEntry(drop.ObjectId, 0, optionalSceneName, new Loot {
								Entity = objectData.objectID,
								EntityVariation = objectData.variation,
								Chance = drop.Chance,
								ChanceForOne = () => drop.CalculateChanceForOne(),
								Amount = () => drop.ObjectAmount,
								Rolls = () => drop.CalculateRolls(),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
								IsFromTableWithGuaranteedPool = drop.TableHasGuaranteedPool
							});
						}
						
						if (EntityUtility.TryGetBuffer<ItemsToAddToNewObjectBuffer>(entity, world, out var itemsToAddToNewObject)) {
							var groupedItemsToAddToNewObject = itemsToAddToNewObject.ConvertToList()
								.GroupBy(entry => entry.objectData.objectID)
								.Select(group => {
									var entry = group.First();
									return new ObjectDataCD {
										objectID = entry.objectData.objectID,
										variation = entry.objectData.variation,
										amount = group.Sum(item => item.objectData.amount)
									};
								});

							foreach (var entry in groupedItemsToAddToNewObject) {
								AddNormalOrSceneEntry(entry.objectID, entry.variation, optionalSceneName, new Loot {
									Entity = objectData.objectID,
									EntityVariation = objectData.variation,
									Amount = () => (entry.amount, entry.amount),
									Chance = 1f,
									ChanceForOne = () => 1f,
									Rolls = () => (1, 1)
								});
							}
						}
					}
				}
				
				// Normal objects
				foreach (var (objectData, _) in allObjects) {
					var entity = PugDatabase.GetPrimaryPrefabEntity(objectData.objectID, pugDatabaseBankBlob, objectData.variation);
					if (entity == Unity.Entities.Entity.Null)
						continue;
					
					AddEntriesFromPrefab(API.Client.World, objectData, entity);
				}
				
				// Scene objects
				ref var customScenesBlobArray = ref API.Client.GetEntityQuery(typeof(CustomSceneTableCD)).GetSingleton<CustomSceneTableCD>().Value.Value.scenes;
				for (var sceneIdx = 0; sceneIdx < customScenesBlobArray.Length; sceneIdx++) {
					ref var customSceneBlob = ref customScenesBlobArray[sceneIdx];
					var sceneName = customSceneBlob.sceneName.ToString();
					
					if (!StructureUtils.CanSceneSpawn(sceneName))
						continue;

					for (var i = 0; i < customSceneBlob.prefabInventoryOverrides.Length; i++) {
						ref var prefabInventoryOverride = ref customSceneBlob.prefabInventoryOverrides[i];
						ref var prefabObjectData = ref customSceneBlob.prefabObjectDatas[i];
						ref var prefab = ref customSceneBlob.prefabs[i];
						
						if (prefabInventoryOverride.hasLootTableOverride) {
							var lootTableOverride = prefabInventoryOverride.lootTableOverride;
							
							foreach (var drop in LootUtils.GetLootTableContents(lootTableOverride)) {
								AddEntryFromScene(drop.ObjectId, 0, sceneName, new Loot {
									Entity = prefabObjectData.objectID,
									EntityVariation = prefabObjectData.variation,
									Chance = drop.Chance,
									ChanceForOne = () => drop.CalculateChanceForOne(),
									Amount = () => drop.ObjectAmount,
									Rolls = () => drop.CalculateRolls(),
									OnlyDropsInBiome = drop.OnlyDropsInBiome,
									IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
									IsFromTableWithGuaranteedPool = drop.TableHasGuaranteedPool
								});
							}
						}

						if (prefabInventoryOverride.hasItemsOverride) {
							var groupedItemsOverride = prefabInventoryOverride.itemsOverride.ConvertToList()
								.GroupBy(entry => entry.objectID)
								.Select(group => {
									var entry = group.First();
									return new ObjectDataCD {
										objectID = entry.objectID,
										variation = entry.variation,
										amount = group.Sum(item => PugDatabase.GetObjectInfo(entry.objectID) is { isStackable: true } ? Math.Max(item.amount, 1) : 1)
									};
								});
							
							foreach (var entry in groupedItemsOverride) {
								AddEntryFromScene(entry.objectID, entry.variation, sceneName, new Loot {
									Entity = prefabObjectData.objectID,
									EntityVariation = prefabObjectData.variation,
									Amount = () => (entry.amount, entry.amount),
									Chance = 1f,
									ChanceForOne = () => 1f,
									Rolls = () => (1, 1)
								});
							}
						}
						
						if (prefab != null && EntityUtility.HasComponentData<CustomScenePrefab>(prefab, API.Client.World))
							AddEntriesFromPrefab(API.Client.World, prefabObjectData, prefab, sceneName);
					}
				}
				
				foreach (var entry in entriesToAdd)
					registry.Register(ObjectEntryType.Source, entry.Id, entry.Variation, entry.Entry);
			}
		}
	}
}