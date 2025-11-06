using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class BackgroundPerks : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/BackgroundPerks", ObjectID.FoodRation, 3400);

		public CharacterRole Background { get; protected set; }
		public int Amount { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var rolePerksTable = ((CharacterCustomizationMenu) Manager.menu.characterCustomizationMenu).roleSelection.perksTable;
				
				foreach (var entry in rolePerksTable.perks.GroupBy(entry => entry.role).Select(group => group.First())) {
					foreach (var starterItem in entry.starterItems) {
						registry.Register(starterItem.objectID, 0, new BackgroundPerks {
							Background = entry.role,
							Amount = starterItem.amount
						});
					}
				}
			}
		}
	}
}