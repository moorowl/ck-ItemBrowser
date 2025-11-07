using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class Miscellaneous : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Miscellaneous", ObjectID.GrimyStoneFloor);
		
		public (ObjectID Id, int Variation, int Amount) Result { get; protected set; }
		public string Term { get; protected set; }

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				// Creating a character with name Greggy
				registry.Register(ObjectEntryType.Source, ObjectID.WallDirtBlock, 0, new Miscellaneous {
					Result = (ObjectID.WallDirtBlock, 0, 10),
					Term = "ItemBrowser:MiscellaneousDesc/Greggy"
				});
				
				// Creating a creative character
				registry.Register(ObjectEntryType.Source, ObjectID.OrbLantern, 0, new Miscellaneous {
					Result = (ObjectID.OrbLantern, 0, 1),
					Term = "ItemBrowser:MiscellaneousDesc/CreativeOrbLantern"
				});
			}
		}
	}
}