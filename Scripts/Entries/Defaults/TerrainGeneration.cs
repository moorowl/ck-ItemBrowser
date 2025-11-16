using System.Collections.Generic;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class TerrainGeneration : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/TerrainGeneration", ObjectID.WallDirtBlock, 5100);
		
		public (ObjectID Id, int Variation) Result { get; protected set; }
		public Tileset? GeneratesInTileset { get; protected set; }
		public Biome GeneratesInBiome { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public static Dictionary<Biome, List<Tileset>> BiomeBlockTilesets = new() {
				{Biome.Slime, new List<Tileset> { Tileset.Dirt, Tileset.Turf, Tileset.Sand }},
				{Biome.Larva, new List<Tileset> { Tileset.Clay, Tileset.Sand }},
				{Biome.Stone, new List<Tileset> { Tileset.Stone, Tileset.Sand }},
				{Biome.Nature, new List<Tileset> { Tileset.Nature, Tileset.Stone }},
				{Biome.Sea, new List<Tileset> { Tileset.Sea }},
				{Biome.Desert, new List<Tileset> { Tileset.Desert }},
				{Biome.Crystal, new List<Tileset> { Tileset.Crystal }},
				{Biome.Passage, new List<Tileset> { Tileset.Passage }}
			};
			public static List<Biome> AncientGemstoneBiomes = new() {
				Biome.Slime,
				Biome.Larva,
				Biome.Stone,
				Biome.Nature,
				Biome.Sea,
				Biome.Desert,
			};
			
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				// this could probably be done automatically but...

				// Dirt biome
				RegisterNormalBlocks(Biome.Slime);
				RegisterExplosiveBlock(Biome.Slime, ObjectID.WallNaturalExplosiveBlock);
				RegisterLiquid(Biome.Slime, Tileset.Dirt);
				RegisterOre(Biome.Slime, ObjectID.CopperOre);
				
				// Clay biome
				RegisterNormalBlocks(Biome.Larva);
				RegisterExplosiveBlock(Biome.Larva, ObjectID.WallNaturalExplosiveBlock);
				RegisterLiquid(Biome.Larva, Tileset.Dirt);
				RegisterOre(Biome.Larva, ObjectID.TinOre);
				
				// Stone biome
				RegisterNormalBlocks(Biome.Stone);
				RegisterExplosiveBlock(Biome.Stone, ObjectID.WallNaturalExplosiveBlock);
				RegisterLiquid(Biome.Stone, Tileset.Dirt);
				RegisterOre(Biome.Stone, ObjectID.IronOre);
				
				// Nature biome
				RegisterNormalBlocks(Biome.Nature);
				RegisterExplosiveBlock(Biome.Nature, ObjectID.WallForestExplosiveBlock);
				RegisterLiquid(Biome.Nature, Tileset.Dirt);
				RegisterOre(Biome.Nature, ObjectID.ScarletOre);
				
				// Sea biome
				RegisterNormalBlocks(Biome.Sea);
				RegisterLiquid(Biome.Sea, Tileset.Sea);
				RegisterOre(Biome.Sea, ObjectID.OctarineOre);
				
				// Desert biome
				RegisterNormalBlocks(Biome.Desert);
				RegisterExplosiveBlock(Biome.Desert, ObjectID.WallDesertExplosiveBlock);
				RegisterLiquid(Biome.Desert, Tileset.Dirt);
				RegisterOre(Biome.Desert, ObjectID.GalaxiteOre);
				
				// Crystal biome
				RegisterNormalBlocks(Biome.Crystal);
				RegisterLiquid(Biome.Crystal, Tileset.Crystal);
				RegisterOre(Biome.Crystal, ObjectID.SolariteOre);
				
				// Passage biome
				RegisterNormalBlocks(Biome.Passage);
				RegisterLiquid(Biome.Passage, Tileset.Passage);
				
				// All biomes
				foreach (var biome in AncientGemstoneBiomes)
					RegisterOre(biome, ObjectID.AncientGemstone);

				return;

				void RegisterNormalBlocks(Biome biome) {
					foreach (var tileset in BiomeBlockTilesets[biome]) {
						var objectInfo = PugDatabase.TryGetTileItemInfo(TileType.wall, (int) tileset);
						registry.Register(ObjectEntryType.Source, objectInfo.objectID, 0, new TerrainGeneration {
							Result = (objectInfo.objectID, 0),
							GeneratesInBiome = biome
						});
					}
				}

				void RegisterExplosiveBlock(Biome biome, ObjectID id) {
					registry.Register(ObjectEntryType.Source, id, 0, new TerrainGeneration {
						Result = (id, 0),
						GeneratesInBiome = biome
					});
				}

				void RegisterOre(Biome biome, ObjectID ore) {
					foreach (var tileset in BiomeBlockTilesets[biome]) {
						registry.Register(ObjectEntryType.Source, ore, 0, new TerrainGeneration {
							Result = (ore, 0),
							GeneratesInBiome = biome,
							GeneratesInTileset = tileset
						});
					}
				}

				void RegisterLiquid(Biome biome, Tileset tileset) {
					var objectInfo = PugDatabase.TryGetTileItemInfo(TileType.water, (int) tileset);
					registry.Register(ObjectEntryType.Source, objectInfo.objectID, objectInfo.variation, new TerrainGeneration {
						Result = (objectInfo.objectID, objectInfo.variation),
						GeneratesInBiome = biome
					});
				}
			}
		}
	}
}