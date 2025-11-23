using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class Unlocking : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Unlocking", ObjectID.CopperKey, 3300);
		
		public (ObjectID Id, int Variation) Key { get; protected set; }
		public (ObjectID Id, int Variation) OutputObject { get; protected set; }
		public (ObjectID Id, int Variation) InputObject { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!ObjectUtils.IsPrimaryVariation(objectData.objectID, objectData.variation))
						continue;
					
					if (!PugDatabase.TryGetComponent<ChangeVariationWhenContainingObjectCD>(objectData, out var changeVariationWhenContainingObjectCD))
						continue;

					if (changeVariationWhenContainingObjectCD.reinstantiateToNewObjectId == ObjectID.None)
						continue;

					var entry = new Unlocking {
						Key = (changeVariationWhenContainingObjectCD.objectID, 0),
						OutputObject = (
							changeVariationWhenContainingObjectCD.reinstantiateToNewObjectId,
							ObjectUtils.GetPrimaryVariation(changeVariationWhenContainingObjectCD.reinstantiateToNewObjectId, changeVariationWhenContainingObjectCD.variationToChangeTo)
						),
						InputObject = (objectData.objectID, objectData.variation),
					};
					registry.Register(ObjectEntryType.Source, entry.OutputObject.Id, entry.OutputObject.Variation, entry);
					registry.Register(ObjectEntryType.Usage, entry.InputObject.Id, entry.InputObject.Variation, entry);
					registry.Register(ObjectEntryType.Usage, entry.Key.Id, entry.Key.Variation, entry);
				}
			}
		}
	}
}