using System.Collections.Generic;
using System.Linq;
using ItemBrowser.DataStructures;
using PugProperties;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class NaturalSpawnAroundObject : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/NaturalSpawnAroundObject", ObjectID.NatureCicadaSummoningItem, 4650);
		
		public ObjectID Entity { get; protected set; }
		public int EntityVariation { get; protected set; }
		public float DespawnRadius { get; protected set; }
		public float SpawnRadius { get; protected set; }
		public (float Min, float Max) SpawnCooldown { get; protected set; }
		public int SpawnLimit { get; protected set; }
		public (float Min, float Max) SpawnLimitReachedCooldown { get; protected set; }
		public Season? SpawnsInSeason { get; protected set; }
		public Biome? SpawnsInBiome { get; protected set; }
		public bool NeedToBeInsideBiome { get; protected set; }
		public Tileset? SpawnsInTileset { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var allCritters = GetContinuouslySpawningCritters(allObjects);

				foreach (var (objectData, authoring) in allObjects) {
					if (!authoring.TryGetComponent<SpawnAroundObjectAuthoring>(out var spawnAroundObjectAuthoring))
						continue;

					foreach (var entry in spawnAroundObjectAuthoring.spawnEntries) {
						var spawnsInBiomes = entry.spawnsInBiome.ToList();
						if (spawnsInBiomes.Count == 0)
							spawnsInBiomes.Add(Biome.None);
						
						if (entry.spawnCrittersInsteadOfObject) {
							foreach (var critter in allCritters) {
								foreach (var biome in critter.Biomes) {
									foreach (var tileset in critter.Tilesets) {
										registry.Register(critter.Id, 0, new NaturalSpawnAroundObject {
											Entity = objectData.objectID,
											EntityVariation = objectData.variation,
											DespawnRadius = entry.critterDespawnDistance,
											SpawnRadius = entry.maxSpawnDistance,
											SpawnCooldown = (entry.minSpawnCooldown, entry.maxSpawnCooldown),
											SpawnLimit = entry.limitNumberSpawned,
											SpawnLimitReachedCooldown = (entry.minReachedLimitCooldown, entry.maxReachedLimitCooldown),
											SpawnsInSeason = entry.onlySpawnsInSeason != Season.None ? entry.onlySpawnsInSeason : null,
											SpawnsInBiome = biome != Biome.None ? biome : null,
											NeedToBeInsideBiome = entry.playerNeedsToBeInsideBiome,
											SpawnsInTileset = tileset != Tileset.MAX_VALUE ? tileset : null
										});
									}
								}
							}
						} else {
							foreach (var biome in spawnsInBiomes) {
								registry.Register(entry.objectToSpawn.objectID, entry.objectToSpawn.variation, new NaturalSpawnAroundObject {
									Entity = objectData.objectID,
									EntityVariation = objectData.variation,
									DespawnRadius = entry.critterDespawnDistance,
									SpawnRadius = entry.maxSpawnDistance,
									SpawnCooldown = (entry.minSpawnCooldown, entry.maxSpawnCooldown),
									SpawnLimit = entry.limitNumberSpawned,
									SpawnLimitReachedCooldown = (entry.minReachedLimitCooldown, entry.maxReachedLimitCooldown),
									SpawnsInSeason = entry.onlySpawnsInSeason != Season.None ? entry.onlySpawnsInSeason : null,
									SpawnsInBiome = biome != Biome.None ? biome : null,
									NeedToBeInsideBiome = entry.playerNeedsToBeInsideBiome
								});
							}
						}
					}
				}
			}

			private static List<(ObjectID Id, List<Biome> Biomes, List<Tileset> Tilesets)> GetContinuouslySpawningCritters(List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				return allObjects
					.Where(entry => PugDatabase.TryGetComponent<ObjectPropertiesCD>(entry.ObjectData, out var propertiesCD) && propertiesCD.Has(PropertyID.Critter.spawnContinuously))
					.GroupBy(entry => entry.ObjectData.objectID)
					.Select(group => {
						var objectData = group.First().ObjectData;
						var propertiesCD = PugDatabase.GetComponent<ObjectPropertiesCD>(objectData);

						var biomesToSpawnIn = propertiesCD.GetManagedList<Biome>(PropertyID.Critter.biomesToSpawnIn);
						var tilesetsToSpawnIn = propertiesCD.Has(PropertyID.Critter.tilesetsToSpawnIn)
							? propertiesCD.GetManagedList<Tileset>(PropertyID.Critter.tilesetsToSpawnIn)
							: new List<Tileset>();
						
						if (tilesetsToSpawnIn.Count == 0)
							tilesetsToSpawnIn.Add(Tileset.MAX_VALUE);
						
						return (
							objectData.objectID,
							biomesToSpawnIn,
							tilesetsToSpawnIn
						);
					})
					.ToList();
			}
		}
	}
}