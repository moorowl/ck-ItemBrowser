using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class Merchant : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/Merchant", ObjectID.SlimeOil, 4200);
		
		public ObjectID MerchantType { get; protected set; }
		public int Stock { get; protected set; }
		public MerchantItemRequirement Requirement { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<MerchantItemInfoBuffer>(objectData))
						continue;

					foreach (var entry in PugDatabase.GetBuffer<MerchantItemInfoBuffer>(objectData)) {
						registry.Register(entry.objectID, 0, new Merchant {
							MerchantType = objectData.objectID,
							Stock = entry.amount,
							Requirement = entry.requirementToBeAvailable
						});
					}
				}
			}
		}
	}
}