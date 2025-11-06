using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class VendingMachine : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/VendingMachine", ObjectID.AFVendingMachine, 4100);
		
		public ObjectID Result { get; protected set; }
		public ObjectID Vendor { get; protected set; }
		public int Stock { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<VendingMachineItemBuffer>(objectData))
						continue;

					foreach (var info in PugDatabase.GetBuffer<VendingMachineItemBuffer>(objectData)) {
						var entry = new VendingMachine {
							Result = info.objectID,
							Vendor = objectData.objectID,
							Stock = objectData.amount
						};
						registry.Register(ObjectEntryType.Source, info.objectID, 0, entry);
						registry.Register(ObjectEntryType.Usage, objectData.objectID, 0, entry);
					}
				}
			}
		}
	}
}