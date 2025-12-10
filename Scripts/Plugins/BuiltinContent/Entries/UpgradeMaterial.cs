using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities.Extensions;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public record UpgradeMaterial : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/UpgradeMaterial", ObjectID.UpgradeForge, Priorities.UpgradeMaterial);
		
		public (int From, int To) Level { get; set; }
		public List<(ObjectID Id, int Amount)> Materials { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var upgradeCostsTable = API.Client.GetEntityQuery(typeof(UpgradeCostsTableCD)).GetSingleton<UpgradeCostsTableCD>();

				for (var i = 2; i <= LevelScaling.GetMaxLevel(); i++) {
					ref var upgradeCosts = ref upgradeCostsTable.GetUpgradeCost(i);
					
					var entry = new UpgradeMaterial {
						Level = (i - 1, i),
						Materials = upgradeCosts.ConvertToList().Select(blob => (blob.item, blob.amount)).ToList()
					};
					foreach (var material in entry.Materials)
						registry.Register(ObjectEntryType.Usage, material.Id, 0, entry);
				}
			}
		}
	}
}