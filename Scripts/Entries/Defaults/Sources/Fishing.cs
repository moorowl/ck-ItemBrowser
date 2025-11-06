using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class Fishing : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/Fishing", ObjectID.IronFishingRod, 4400);
		
		public Biome Biome { get; protected set; }
		public Tileset Tileset { get; protected set; }
		public CatchType Type { get; protected set; }
		public float Chance { get; protected set; }
		
		public enum CatchType {
			Fish,
			Loot
		}

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var fishingTable = Manager.mod.FishingTable;
				var biomeLoot = fishingTable.fishingInfos.Where(info => info.biomes.Count > 0);
				var liquidLoot = fishingTable.fishingInfos.Where(info => info.waterTilesets.Count > 0);
				
				foreach (var info in liquidLoot) {
					var tilesets = info.waterTilesets;
					if (tilesets.Count == 0 || tilesets[0] == Tileset.Dirt)
						continue;
					
					foreach (var drop in LootUtils.GetLootTableContents(info.lootTableID)) {
						foreach (var tileset in tilesets) {
							registry.Register(drop.ObjectId, 0, new Fishing {
								Tileset = tileset,
								Type = CatchType.Loot,
								Chance = drop.Chance
							});
						}
					}
					
					foreach (var drop in LootUtils.GetLootTableContents(info.fishLootTableID)) {
						foreach (var tileset in tilesets) {
							registry.Register(drop.ObjectId, 0, new Fishing {
								Tileset = tileset,
								Type = CatchType.Fish,
								Chance = drop.Chance
							});
						}
					}
				}
				
				foreach (var info in biomeLoot) {
					var biomes = info.biomes;
					if (biomes.Count == 0 || biomes.Contains(Biome.None))
						continue;
					
					foreach (var drop in LootUtils.GetLootTableContents(info.lootTableID)) {
						foreach (var biome in biomes) {
							registry.Register(drop.ObjectId, 0, new Fishing {
								Biome = biome,
								Type = CatchType.Loot,
								Chance = drop.Chance
							});
						}
					}
					
					foreach (var drop in LootUtils.GetLootTableContents(info.fishLootTableID)) {
						foreach (var biome in biomes) {
							registry.Register(drop.ObjectId, 0, new Fishing {
								Biome = biome,
								Type = CatchType.Fish,
								Chance = drop.Chance
							});
						}
					}
				}
			}
		}
	}
}