using System.Collections.Generic;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class NaturalSpawnInitial : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/NaturalSpawnInitial", ObjectID.Bush, 4700);
		
		public EnvironmentSpawnType Type { get; protected set; }
		public int AmountToSpawn { get; protected set; }
		public Tileset? TilesetToSpawnOn { get; protected set; }
		public float ClusterSpawnChance { get; protected set; }
		public float ClusterSpreadChance { get; protected set; }
		public bool ClusterSpreadFourWayOnly { get; protected set; }
		public EnvironmentSpawnData.SpawnCheck SpawnCheck { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var spawnTable = Manager.mod.SpawnTable;
				
				foreach (var entry in spawnTable.spawnObjects) {
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

						foreach (var variation in variations) {
							foreach (var tileset in tilesets) {
								registry.Register(ObjectEntryType.Source, spawn.objectID, variation.value, new NaturalSpawnInitial {
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