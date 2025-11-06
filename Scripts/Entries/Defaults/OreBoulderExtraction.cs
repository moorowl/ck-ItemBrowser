using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class OreBoulderExtraction : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/OreBoulderExtraction", ObjectID.GoldOreBoulder, 5000);
		
		public ObjectID OreBoulder { get; protected set; }
		public int TotalOre { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<RequiresDrillCD>(objectData) || !PugDatabase.TryGetComponent<DropsLootWhenDamagedCD>(objectData, out var dropsLootWhenDamagedCD) || !PugDatabase.TryGetComponent<HealthCD>(objectData, out var healthCD))
						continue;
					
					if (dropsLootWhenDamagedCD.minHealthToDropLoot != 0)
						continue;
					
					registry.Register(ObjectEntryType.Source, dropsLootWhenDamagedCD.dropsLoot, 0, new OreBoulderExtraction {
						OreBoulder = objectData.objectID,
						TotalOre = (int) math.floor(healthCD.maxHealth / (float) dropsLootWhenDamagedCD.damageToDealToDropLoot)
					});
				}
			}
		}
	}
}