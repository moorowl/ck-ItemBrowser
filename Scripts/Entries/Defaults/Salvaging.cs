using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class Salvaging : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Salvaging", ObjectID.SalvageAndRepairStation, 3300);
		
		public ObjectID Salvaged { get; protected set; }
		public (int Min, int Max) Amount { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					if (objectInfo == null)
						continue;

					var hasDurability = PugDatabase.HasComponent<DurabilityCD>(objectData);
					var hasLevel = PugDatabase.HasComponent<LevelCD>(objectData);
					
					if (!objectInfo.tags.Contains(ObjectCategoryTag.CanBeSalvaged) && (objectInfo.isStackable || !hasLevel || objectInfo.rarity == Rarity.Legendary))
						continue;
					
					foreach (var entry in objectInfo.requiredObjectsToCraft) {
						var minAmount = (int) math.round(entry.amount * Constants.minMaterialToGainFromSalvage);
						var maxAmount = (int) math.round(entry.amount * Constants.maxMaterialToGainFromSalvage);

						if (!hasDurability || !hasLevel)
							minAmount = maxAmount;
						
						if (maxAmount > 0) {
							registry.Register(ObjectEntryType.Source, entry.objectID, 0, new Salvaging {
								Salvaged = objectData.objectID,
								Amount = (minAmount, maxAmount)
							});
						}
					}
				}
			}
		}
	}
}