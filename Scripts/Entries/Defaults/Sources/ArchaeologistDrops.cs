using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class ArchaeologistDrops : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/ArchaeologistDrops", ObjectID.WallDirtBlock, 5100);
		
		public float Chance { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var drop in LootUtils.GetLootTableContents(LootTableID.ArcheologistWallLoot)) {
					registry.Register(drop.ObjectId, 0, new ArchaeologistDrops {
						Chance = drop.Chance
					});
				}
			}
		}
	}
}