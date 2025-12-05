using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public record Fishing : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Fishing", ObjectID.IronFishingRod, 4400);
		
		public ObjectID Result { get; set; }
		public Biome Biome { get; set; }
		public Tileset Tileset { get; set; }
		public CatchType Type { get; set; }
		public float Chance { get; set; }
		
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
							registry.Register(ObjectEntryType.Source, drop.ObjectId, 0, new Fishing {
								Result = drop.ObjectId,
								Tileset = tileset,
								Type = CatchType.Loot,
								Chance = drop.Chance
							});
						}
					}
					
					foreach (var drop in LootUtils.GetLootTableContents(info.fishLootTableID)) {
						foreach (var tileset in tilesets) {
							registry.Register(ObjectEntryType.Source, drop.ObjectId, 0, new Fishing {
								Result = drop.ObjectId,
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
							registry.Register(ObjectEntryType.Source, drop.ObjectId, 0, new Fishing {
								Result = drop.ObjectId,
								Biome = biome,
								Type = CatchType.Loot,
								Chance = drop.Chance
							});
						}
					}
					
					foreach (var drop in LootUtils.GetLootTableContents(info.fishLootTableID)) {
						foreach (var biome in biomes) {
							registry.Register(ObjectEntryType.Source, drop.ObjectId, 0, new Fishing {
								Result = drop.ObjectId,
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