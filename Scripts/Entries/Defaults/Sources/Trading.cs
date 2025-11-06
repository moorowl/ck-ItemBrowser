using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class Trading : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/Trading", ObjectID.ValentineLetter, 4300);
		
		public ObjectID Vendor { get; protected set; }
		public int Amount { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<CraftingCD>(objectData, out var craftingCD) || !PugDatabase.HasComponent<CanCraftObjectsBuffer>(objectData))
						continue;

					if (craftingCD.craftingType != CraftingType.Simple)
						continue;

					if (PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType != ObjectType.Creature)
						continue;
					
					var canCraftObjects = PugDatabase.GetBuffer<CanCraftObjectsBuffer>(objectData);
					foreach (var entry in canCraftObjects) {
						var objectInfo = PugDatabase.GetObjectInfo(entry.objectID);
						if (objectInfo == null)
							continue;
						
						registry.Register(entry.objectID, 0, new Trading {
							Vendor = objectData.objectID,
							Amount = Math.Max(entry.amount, 1)
						});
					}
				}
			}
		}
	}
}