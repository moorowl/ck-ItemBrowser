using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class DropsWhenDamaged : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/DropsWhenDamaged", ObjectID.SolariteOre, 3790);
		
		public (ObjectID Id, int Variation) Result { get; protected set; }
		public (ObjectID Id, int Variation) Entity { get; protected set; }
		public int DamageRequiredToDrop { get; protected set; }
		public int HealthRequiredToDrop { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<DropsLootWhenDamagedCD>(objectData, out var dropsLootWhenDamagedCD))
						continue;

					if (PugDatabase.HasComponent<RequiresDrillCD>(objectData))
						continue;

					var entry = new DropsWhenDamaged {
						Result = (dropsLootWhenDamagedCD.dropsLoot, 0),
						Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
						DamageRequiredToDrop = dropsLootWhenDamagedCD.damageToDealToDropLoot,
						HealthRequiredToDrop = dropsLootWhenDamagedCD.minHealthToDropLoot
					};
					registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
					// registry.Register(ObjectEntryType.Usage, entry.Entity.Id, entry.Entity.Variation, entry);
				}
			}
		}
	}
}