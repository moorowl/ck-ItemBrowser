using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class JewelryCrafter : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/JewelryCrafter", ObjectID.JewelryWorkBench, 3500);
		
		public ObjectID UnpolishedVersion { get; protected set; }
		
		public class Provider : ObjectEntryProvider {
			public static readonly HashSet<ObjectID> JewelryCraftingStations = new() {
				ObjectID.JewelryWorkBench,
				ObjectID.AdvancedJewelryWorkBench
			};
			
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var jewelryItems = new HashSet<ObjectID>();

				foreach (var (objectData, _) in allObjects) {
					if (!JewelryCraftingStations.Contains(objectData.objectID) || !PugDatabase.HasComponent<CanCraftObjectsBuffer>(objectData))
						continue;

					var canCraftObjects = PugDatabase.GetBuffer<CanCraftObjectsBuffer>(objectData);
					foreach (var entry in canCraftObjects)
						jewelryItems.Add(entry.objectID);
				}
				
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<JewelryCanBePolishedCD>(objectData, out var jewelryCanBePolished))
						continue;

					var polishedId = jewelryCanBePolished.polishedVersion;

					var rarity = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).rarity;
					if (rarity == Rarity.Epic || polishedId == objectData.objectID || !jewelryItems.Contains(objectData.objectID))
						continue;

					registry.Register(ObjectEntryType.Source, polishedId, 0, new JewelryCrafter {
						UnpolishedVersion = objectData.objectID
					});
				}
			}
		}
	}
}