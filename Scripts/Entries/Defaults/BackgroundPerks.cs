using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public record BackgroundPerks : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/BackgroundPerks", ObjectID.FoodRation, 3400);
		
		public (ObjectID Id, int Variation, int Amount) Result { get; set; }
		public CharacterRole Background { get; set; }
		public SkillID BackgroundSkill { get; set; }

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var rolePerksTable = ((CharacterCustomizationMenu) Manager.menu.characterCustomizationMenu).roleSelection.perksTable;
				
				foreach (var entry in rolePerksTable.perks.GroupBy(entry => entry.role).Select(group => group.First())) {
					foreach (var starterItem in entry.starterItems) {
						registry.Register(ObjectEntryType.Source, starterItem.objectID, 0, new BackgroundPerks {
							Result = (starterItem.objectID, starterItem.variation, starterItem.amount),
							Background = entry.role,
							BackgroundSkill = entry.starterSkill
						});
					}
				}
			}
		}
	}
}