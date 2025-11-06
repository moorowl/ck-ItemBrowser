using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class CattleProduce : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/CattleProduce", ObjectID.Egg, 4000);
		
		public ObjectID Cattle { get; protected set; }
		public int Amount { get; protected set; }
		public List<ObjectCategoryTag> SuitableFeed { get; protected set; }
		public int SuitableFeedRequired { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<CraftingCD>(objectData, out var craftingCD) || !PugDatabase.HasComponent<CanCraftObjectsBuffer>(objectData))
						continue;

					if (craftingCD.craftingType != CraftingType.Cattle)
						continue;

					if (!PugDatabase.TryGetComponent<BehaviourTagsCD>(objectData, out var behaviorTags))
						continue;

					var eatsTags = new List<ObjectCategoryTag>();
					for (var i = 1; i < 64; i++) {
						if ((behaviorTags.eatsTagsBitMask & (1uL << i)) != 0) {
							eatsTags.Add((ObjectCategoryTag) i);
						}
					}
					
					var canCraftObjects = PugDatabase.GetBuffer<CanCraftObjectsBuffer>(objectData);
					foreach (var entry in canCraftObjects) {
						var objectInfo = PugDatabase.GetObjectInfo(entry.objectID);
						if (objectInfo == null || entry.objectID == ObjectID.None || entry.entityAmountToConsume == 0)
							continue;
						
						registry.Register(entry.objectID, 0, new CattleProduce {
							Cattle = objectData.objectID,
							Amount = Math.Max(entry.amount, 1),
							SuitableFeed = eatsTags,
							SuitableFeedRequired = entry.entityAmountToConsume
						});
					}
				}
			}
		}
	}
}