using System.Collections.Generic;
using ItemBrowser.Utilities;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class NaturalSpawnRespawn : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/NaturalSpawnRespawn", ObjectID.MeadowBush, 4800);
		
		public EnvironmentSpawnType Type { get; protected set; }
		public int AmountToSpawn { get; protected set; }
		public Tileset? TilesetToSpawnOn { get; protected set; }
		public float ClusterSpawnChance { get; protected set; }
		public float ClusterSpreadChance { get; protected set; }
		public bool ClusterSpreadFourWayOnly { get; protected set; }
		public RespawnData.SpawnCheck SpawnCheck { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var spawnTable = Manager.mod.SpawnTable;
				
				foreach (var entry in spawnTable.respawnObjects) {
					var tilesets = entry.spawnCheck.tilesets;
					if (tilesets.Count == 0)
						tilesets.Add(Tileset.MAX_VALUE);
					
					foreach (var spawn in entry.spawns) {
						var variations = new List<ValueWithWeight<int>>();
						if (spawn.advancedVariationControl) {
							variations.AddRange(spawn.weightedVariations.value);
						} else {
							for (var i = spawn.variation.min; i <= spawn.variation.max; i++)
								variations.Add(new ValueWithWeight<int>(i, 1f));
						}
						
						variations.RemoveAll(variation => !ObjectUtils.IsPrimaryVariation(spawn.objectID, variation.value));
						if (variations.Count == 0)
							variations.Add(new ValueWithWeight<int>(0, 1f));
						
						foreach (var variation in variations) {
							foreach (var tileset in tilesets) {
								registry.Register(ObjectEntryType.Source, spawn.objectID, variation.value, new NaturalSpawnRespawn {
									Type = spawn.spawnType,
									AmountToSpawn = spawn.amount,
									TilesetToSpawnOn = tileset == Tileset.MAX_VALUE ? null : tileset,
									ClusterSpawnChance = spawn.clusterSpawnChance,
									ClusterSpreadChance = spawn.clusterSpreadChance,
									ClusterSpreadFourWayOnly = spawn.clusterSpreadFourWayOnly,
									SpawnCheck = entry.spawnCheck
								});
							}
						}
					}
				}
			}
		}
	}
}