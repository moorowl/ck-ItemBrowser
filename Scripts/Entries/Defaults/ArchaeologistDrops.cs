using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public record ArchaeologistDrops : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/ArchaeologistDrops", ObjectID.WallStoneBlock, 5100);
		
		public ObjectID Result { get; set; }
		public (float Min, float Max) Chance { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var chanceAtMin = Manager.mod.SkillTalentsTable.skillTalentTrees.SelectMany(tree => tree.skillTalents)
					.FirstOrDefault(talent => talent.givesCondition == ConditionID.ChanceForRandomLootFromWall).conditionValuePerPoint / 100f;
				var chanceAtMax = chanceAtMin * Constants.kSkillPointsPerTalentPoint;
				
				foreach (var drop in LootUtils.GetLootTableContents(LootTableID.ArcheologistWallLoot)) {
					registry.Register(ObjectEntryType.Source, drop.ObjectId, 0, new ArchaeologistDrops {
						Result = drop.ObjectId,
						Chance = (drop.Chance * chanceAtMin, drop.Chance * chanceAtMax)
					});
				}
			}
		}
	}
}