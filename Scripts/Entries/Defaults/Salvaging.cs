using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public record Salvaging : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Salvaging", ObjectID.SalvageAndRepairStation, 3300);
		
		public ObjectID Result { get; set; }
		public (int Min, int Max) ResultAmount { get; set; }
		public ObjectID ItemSalvaged { get; set; }

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					if (objectInfo == null)
						continue;

					var hasDurability = PugDatabase.HasComponent<DurabilityCD>(objectData);
					var hasLevel = PugDatabase.HasComponent<LevelCD>(objectData);

					if (!objectInfo.tags.Contains(ObjectCategoryTag.CanBeSalvaged) || objectInfo.rarity == Rarity.Legendary)
						continue;
					
					foreach (var craftingObject in objectInfo.requiredObjectsToCraft) {
						var minAmount = (int) math.round(craftingObject.amount * Constants.minMaterialToGainFromSalvage);
						var maxAmount = (int) math.round(craftingObject.amount * Constants.maxMaterialToGainFromSalvage);

						if (!hasDurability || !hasLevel)
							minAmount = maxAmount;
						
						if (maxAmount > 0) {
							var entry = new Salvaging {
								Result = craftingObject.objectID,
								ResultAmount = (minAmount, maxAmount),
								ItemSalvaged = objectData.objectID
							};
							registry.Register(ObjectEntryType.Source, entry.Result, 0, entry);
						}
					}
				}
			}
		}
	}
}