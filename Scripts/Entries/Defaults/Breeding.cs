using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class Breeding : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Breeding", ObjectID.ValentineWallHearts, 3900);
		
		public ObjectID ParentType { get; protected set; }
		public ObjectID ChildType { get; protected set; }
		public int MealsRequired { get; protected set; }

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<BreedStateCD>(objectData, out var breedStateCD))
						continue;

					var entry = new Breeding {
						ParentType = objectData.objectID,
						ChildType = breedStateCD.babyType,
						MealsRequired = breedStateCD.mealsToTrigger
					};
					registry.Register(ObjectEntryType.Source, breedStateCD.babyType, 0, entry);
					registry.Register(ObjectEntryType.Usage, objectData.objectID, 0, entry);
				}
			}
		}
	}
}