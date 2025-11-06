using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class VendingMachine : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/VendingMachine", ObjectID.AFVendingMachine, 4100);
		
		public ObjectID Vendor { get; protected set; }
		public int Stock { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<VendingMachineItemBuffer>(objectData))
						continue;

					foreach (var entry in PugDatabase.GetBuffer<VendingMachineItemBuffer>(objectData)) {
						registry.Register(entry.objectID, 0, new VendingMachine {
							Vendor = objectData.objectID,
							Stock = objectData.amount
						});
					}
				}
			}
		}
	}
}