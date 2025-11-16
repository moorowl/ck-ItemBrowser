using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class Trading : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Trading", ObjectID.ValentineLetter, 4300);
		
		public (ObjectID Id, int Variation) Result { get; protected set; }
		public (ObjectID Id, int Variation) Vendor { get; protected set; }
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
					foreach (var canCraftObject in canCraftObjects) {
						var objectInfo = PugDatabase.GetObjectInfo(canCraftObject.objectID);
						if (objectInfo == null)
							continue;

						var entry = new Trading {
							Result = (canCraftObject.objectID, 0),
							Vendor = (objectData.objectID, objectData.variation),
							Amount = Math.Max(canCraftObject.amount, 1)
						};
						registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
						registry.Register(ObjectEntryType.Usage, entry.Vendor.Id, entry.Vendor.Variation, entry);
					}
				}
			}
		}
	}
}