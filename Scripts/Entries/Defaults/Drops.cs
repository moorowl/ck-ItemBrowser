using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.Extensions;
using PugMod;
using Unity.Entities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class Drops : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Drops", ObjectID.Slime, 3800);
		
		public (ObjectID Id, int Variation) Result { get; protected set; }
		public (ObjectID Id, int Variation) Entity { get; protected set; }
		public float Chance { get; protected set; }
		public Func<float> ChanceForOne { get; protected set; }
		public Func<(int Min, int Max)> Amount { get; protected set; }
		public Func<(int Min, int Max)> Rolls { get; protected set; }
		public float MaxAmountCheckRadius { get; protected set; }
		public int MaxAmountAllowedWithinRadius { get; protected set; }
		public Biome OnlyDropsInBiome { get; protected set; }
		public Season OnlyDropsInSeason { get; protected set; }
		public bool IsFromGuaranteedPool { get; protected set; }
		public bool IsFromTableWithGuaranteedPool { get; protected set; }
		public List<(string Name, int Amount)> FoundInScenes { get; protected set; } = new();
		
		protected bool Equals(Drops other) {
			return Entity.Id == other.Entity.Id
			       && Entity.Variation == other.Entity.Variation
			       && Mathf.Approximately(Chance, other.Chance)
			       && Mathf.Approximately(ChanceForOne(), other.ChanceForOne())
			       && Amount() == other.Amount()
			       && Rolls() == other.Rolls()
			       && Mathf.Approximately(MaxAmountCheckRadius, other.MaxAmountCheckRadius)
			       && MaxAmountAllowedWithinRadius == other.MaxAmountAllowedWithinRadius
			       && OnlyDropsInBiome == other.OnlyDropsInBiome
			       && OnlyDropsInSeason == other.OnlyDropsInSeason
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
			return Equals((Drops) obj);
		}

		public override int GetHashCode() {
			var hashCode = new HashCode();
			hashCode.Add((int) Entity.Id);
			hashCode.Add(Entity.Variation);
			hashCode.Add(Chance);
			hashCode.Add(ChanceForOne());
			hashCode.Add(Amount());
			hashCode.Add(Rolls());
			hashCode.Add(MaxAmountCheckRadius);
			hashCode.Add(MaxAmountAllowedWithinRadius);
			hashCode.Add((int) OnlyDropsInBiome);
			hashCode.Add((int) OnlyDropsInSeason);
			hashCode.Add(IsFromGuaranteedPool);
			hashCode.Add(IsFromTableWithGuaranteedPool);
			return hashCode.ToHashCode();
		}

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var entriesToAdd = new List<(ObjectID Id, int Variation, Drops Entry)>();
				var pugDatabaseBankBlob = API.Client.GetEntityQuery(typeof(PugDatabase.DatabaseBankCD)).GetSingleton<PugDatabase.DatabaseBankCD>().databaseBankBlob;

				void AddNormalEntry(ObjectID id, int variation, Drops entry) {
					entriesToAdd.Add((id, variation, entry));
				}
				
				void AddEntryFromScene(ObjectID id, int variation, string sceneName, Drops entry) {
					foreach (var existingEntry in entriesToAdd) {
						if (existingEntry.Id == id && existingEntry.Variation == variation && existingEntry.Entry.Equals(entry)) {
							if (existingEntry.Entry.FoundInScenes.Count > 0) {
								var existingScene = existingEntry.Entry.FoundInScenes.FirstOrDefault(x => x.Name == sceneName);
								if (existingScene.Name != null)
									existingScene.Amount += 1;
								else
									existingEntry.Entry.FoundInScenes.Add((sceneName, 1));
							}

							return;
						}
					}
					
					entry.FoundInScenes.Add((sceneName, 1));
					AddNormalEntry(id, variation, entry);
				}
				
				void AddNormalOrSceneEntry(ObjectID id, int variation, string sceneName, Drops entry) {
					if (sceneName == null)
						AddNormalEntry(id, variation, entry);
					else
						AddEntryFromScene(id, variation, sceneName, entry);
				}

				void AddEntriesFromPrefab(World world, ObjectDataCD objectData, Entity entity, string optionalSceneName = null) {
					if (EntityUtility.TryGetComponentData<SnakeMovementStateCD>(entity, world, out var snakeMovementStateCD) && snakeMovementStateCD.tailObjectId == objectData.objectID)
						return;
					
					if (EntityUtility.HasComponentData<ProjectileCD>(entity, world) || EntityUtility.HasComponentData<MortarProjectileCD>(entity, world))
						return;
					
					// Spawns on death
					if (EntityUtility.TryGetComponentData<SpawnEntityOnDeathCD>(entity, world, out var spawnEntityOnDeathCD)) {
						var entry = new Drops {
							Result = (spawnEntityOnDeathCD.objectToSpawn, ObjectUtils.GetPrimaryVariation(spawnEntityOnDeathCD.objectToSpawn, spawnEntityOnDeathCD.objectVariation)),
							Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
							Chance = spawnEntityOnDeathCD.spawnChance,
							ChanceForOne = () => spawnEntityOnDeathCD.spawnChance,
							Amount = () => (1, 1),
							Rolls = () => (1, 1),
							MaxAmountCheckRadius = spawnEntityOnDeathCD.maxAmountCheckRadius,
							MaxAmountAllowedWithinRadius = spawnEntityOnDeathCD.maxAmountAllowedWithinRadius
						};
						AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
					}
					
					// Boss chest
					if (EntityUtility.TryGetComponentData<BossCD>(entity, world, out var bossCD)) {
						var chestObjectData = UsesOptionalChest(objectData) ? bossCD.optionalChestVersion : bossCD.chestToSpawn;
						var entry = new Drops {
							Result = (chestObjectData.objectID, 0),
							Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
							Chance = 1f,
							ChanceForOne = () => 1f,
							Amount = () => (1, 1),
							Rolls = () => (1, 1)
						};
						AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
					}
					
					// Drops
					if (EntityUtility.TryGetComponentData<ChanceToDropLootCD>(entity, world, out var chanceToDropLootCD) && EntityUtility.TryGetBuffer<DropsLootBuffer>(entity, world, out var dropsLootBuffer)) {
						var groupedDrops = dropsLootBuffer.ConvertToList().GroupBy(entry => entry.lootDrop.lootDropID).Select(group => new LootDrop {
							lootDropID = group.First().lootDrop.lootDropID,
							amount = group.Sum(entry => entry.lootDrop.amount),
							multiplayerAmountAdditionScaling = group.First().lootDrop.multiplayerAmountAdditionScaling
						});

						foreach (var drop in groupedDrops) {
							if (drop.lootDropID == objectData.objectID && EntityUtility.HasComponentData<DontDropSelfCD>(entity, world) && !EntityUtility.HasComponentData<TileCD>(entity, world))
								continue;

							var entry = new Drops {
								Result = (drop.lootDropID, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
								Chance = chanceToDropLootCD.chance,
								ChanceForOne = () => chanceToDropLootCD.chance,
								Amount = () => {
									var scaledAmount = LootUtils.GetMultiplayerScaledAmount(drop.amount, drop.multiplayerAmountAdditionScaling);
									return (scaledAmount, scaledAmount);
								},
								Rolls = () => (1, 1)
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
						}
					}
					
					// Drops from table
					if (EntityUtility.TryGetComponentData<DropsLootFromLootTableCD>(entity, world, out var dropsLootFromLootTableCD)) {
						var isBoss = PugDatabase.HasComponent<BossCD>(objectData);
						foreach (var drop in LootUtils.GetLootTableContents(dropsLootFromLootTableCD.lootTableID)) {
							var entry = new Drops {
								Result = (drop.ObjectId, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
								Chance = drop.Chance,
								ChanceForOne = () => isBoss ? drop.CalculateChanceForOneForBosses() : drop.CalculateChanceForOne(),
								Amount = () => drop.ObjectAmount,
								Rolls = () => isBoss ? drop.CalculateRollsForBosses() : drop.CalculateRolls(),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
								IsFromTableWithGuaranteedPool = drop.TableHasGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
						}
					}

					// Spawns on use (containers)
					if (EntityUtility.TryGetComponentData<SpawnsItemsOnUseCD>(entity, world, out var spawnsItemsOnUseCD) && EntityUtility.TryGetBuffer<OnUseLootBuffer>(entity, world, out var onUseLootBuffer)) {
						foreach (var onUseLoot in onUseLootBuffer) {
							var entry = new Drops {
								Result = (onUseLoot.lootDropID, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
								Chance = onUseLoot.chance,
								ChanceForOne = () => onUseLoot.chance,
								Amount = () => (onUseLoot.amount, onUseLoot.amount),
								Rolls = () => (1, 1)
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
						}
						
						foreach (var drop in LootUtils.GetLootTableContents(spawnsItemsOnUseCD.lootTable)) {
							var entry = new Drops {
								Result = (drop.ObjectId, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
								Chance = drop.Chance,
								ChanceForOne = () => drop.CalculateChanceForOne(),
								Amount = () => drop.ObjectAmount,
								Rolls = () => drop.CalculateRolls(),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
								IsFromTableWithGuaranteedPool = drop.TableHasGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
						}
					}
				}
				
				// Normal objects
				foreach (var (objectData, authoring) in allObjects) {
					var entity = PugDatabase.GetPrimaryPrefabEntity(objectData.objectID, pugDatabaseBankBlob, objectData.variation);
					if (entity == Unity.Entities.Entity.Null || !ObjectUtils.IsPrimaryVariation(objectData.objectID, objectData.variation))
						continue;
					
					AddEntriesFromPrefab(API.Client.World, objectData, entity);
					
					// Seasonal drops
					// Have to use DropLootAuthoring because drops from a season that isn't active aren't converted
					if (authoring.TryGetComponent<DropLootAuthoring>(out var dropLootAuthoring) && dropLootAuthoring.hasSeasonalLoot && IsSeasonalDropRequirementFulfilled(objectData)) {
						var seasonalLoot = dropLootAuthoring.seasonalLootDrops;

						foreach (var group in seasonalLoot.lootDrops) {
							foreach (var drop in group.lootDrops) {
								var entry = new Drops {
									Result = (drop.lootDropID, 0),
									Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
									Chance = drop.chance,
									ChanceForOne = () => drop.chance,
									Amount = () => {
										var scaledAmount = LootUtils.GetMultiplayerScaledAmount(drop.amount, drop.multiplayerAmountAdditionScaling);
										return (scaledAmount, scaledAmount);
									},
									Rolls = () => (1, 1),
									OnlyDropsInSeason = group.season
								};
								AddNormalEntry(entry.Result.Id, entry.Result.Variation, entry);
							}
						}
					}
				}
				
				// Scene objects
				ref var customScenesBlobArray = ref API.Client.GetEntityQuery(typeof(CustomSceneTableCD)).GetSingleton<CustomSceneTableCD>().Value.Value.scenes;
				for (var sceneIdx = 0; sceneIdx < customScenesBlobArray.Length; sceneIdx++) {
					ref var customSceneBlob = ref customScenesBlobArray[sceneIdx];
					var sceneName = customSceneBlob.sceneName.ToString();
					
					if (!StructureUtils.CanSceneSpawn(sceneName))
						continue;

					for (var i = 0; i < customSceneBlob.prefabInventoryOverrides.Length; i++) {
						ref var prefabObjectData = ref customSceneBlob.prefabObjectDatas[i];
						ref var prefab = ref customSceneBlob.prefabs[i];

						if (prefab != null && EntityUtility.HasComponentData<CustomScenePrefab>(prefab, API.Client.World))
							AddEntriesFromPrefab(API.Client.World, prefabObjectData, prefab, sceneName);
					}
				}

				foreach (var entry in entriesToAdd) {
					registry.Register(ObjectEntryType.Source, entry.Id, entry.Variation, entry.Entry);
					// registry.Register(ObjectEntryType.Usage, entry.Entry.Entity.Id, entry.Entry.Entity.Variation, entry.Entry);
				}
			}
			
			private static bool IsSeasonalDropRequirementFulfilled(ObjectData objectData) {
				// this is hardcoded in BirdBossSystem sadly
				if (objectData.objectID == ObjectID.BirdBoss)
					return objectData.variation == 1;

				return true;
			}

			private static bool UsesOptionalChest(ObjectData objectData) {
				// this is hardcoded in BirdBossSystem sadly
				return objectData is { objectID: ObjectID.BirdBoss, variation: 1 };
			}
		}
	}
}