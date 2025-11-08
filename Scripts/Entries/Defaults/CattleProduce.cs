using System;
using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class CattleProduce : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/CattleProduce", ObjectID.Egg, 4000);
		
		public ObjectID Result { get; protected set; }
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
					foreach (var canCraftObject in canCraftObjects) {
						var objectInfo = PugDatabase.GetObjectInfo(canCraftObject.objectID);
						if (objectInfo == null || canCraftObject.objectID == ObjectID.None || canCraftObject.entityAmountToConsume == 0)
							continue;

						var entry = new CattleProduce {
							Result = canCraftObject.objectID,
							Cattle = objectData.objectID,
							Amount = Math.Max(canCraftObject.amount, 1),
							SuitableFeed = eatsTags,
							SuitableFeedRequired = canCraftObject.entityAmountToConsume
						};
						registry.Register(ObjectEntryType.Source, canCraftObject.objectID, 0, entry);
						registry.Register(ObjectEntryType.Usage, objectData.objectID, 0, entry);
						foreach (var eatsTag in eatsTags) {
							foreach (var ingredient in ObjectUtils.GetAllObjectsWithTag(eatsTag)) {
								registry.Register(ObjectEntryType.Usage, ingredient.objectID, ingredient.variation, entry);
							}
						}
					}
				}
			}
		}
	}
}