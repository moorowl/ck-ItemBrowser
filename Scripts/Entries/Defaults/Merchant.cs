using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class Merchant : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Merchant", ObjectID.SlimeOil, 4200);
		
		public ObjectID Result { get; protected set; }
		public ObjectID MerchantType { get; protected set; }
		public int Stock { get; protected set; }
		public MerchantItemRequirement Requirement { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<MerchantItemInfoBuffer>(objectData))
						continue;

					foreach (var info in PugDatabase.GetBuffer<MerchantItemInfoBuffer>(objectData)) {
						var entry = new Merchant {
							Result = info.objectID,
							MerchantType = objectData.objectID,
							Stock = info.amount,
							Requirement = info.requirementToBeAvailable
						};
						registry.Register(ObjectEntryType.Source, entry.Result, 0, entry);
						registry.Register(ObjectEntryType.Usage, entry.MerchantType, 0, entry);
					}
				}
			}
		}
	}
}