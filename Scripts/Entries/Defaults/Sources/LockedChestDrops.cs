using System;
using System.Collections.Generic;
using System.Linq;
using PugMod;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class LockedChestDrops : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/LockedChestDrops", ObjectID.WallDirtBlock, 5100);
		
		public Biome RequiredBiome { get; protected set; }
		public Tileset RequiredTileset { get; protected set; }
		public float Chance { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			private static readonly MemberInfo MiBiomeAndTilesetToChest = typeof(DropLootSystem).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "BiomeAndTilesetToChest");
			
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var biome in Enum.GetValues(typeof(Biome)).Cast<Biome>()) {
					foreach (var tileset in Enum.GetValues(typeof(Tileset)).Cast<Tileset>()) {
						var chest = (ObjectID) API.Reflection.Invoke(MiBiomeAndTilesetToChest, null, biome, tileset);
						if (chest == ObjectID.None)
							continue;
						
						registry.Register(chest, 0, new LockedChestDrops {
							RequiredBiome = biome,
							RequiredTileset = tileset,
							// from DropLootSystem, this isn't a const anywhere?
							Chance = 0.0027027028f
						});
					}
				}
			}
		}
	}
}