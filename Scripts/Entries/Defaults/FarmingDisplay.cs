using System.Globalization;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class FarmingDisplay : ObjectEntryDisplay<Farming> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot seedSlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result
			});
			seedSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Seed
			});
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = Entry.HasGoldSeed ? "ItemBrowser:MoreInfo/Farming_0_" + (Entry.RequiresGoldSeed ? "Golden" : "Normal") : "ItemBrowser:MoreInfo/Farming_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Seed)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			if (Entry.RequiresGoldSeed) {
				var chanceAtMin = Manager.mod.SkillTalentsTable.skillTalentTrees.SelectMany(tree => tree.skillTalents)
					.FirstOrDefault(talent => talent.givesCondition == ConditionID.ChanceToGainRarePlant).conditionValuePerPoint;
				var chanceAtMax = chanceAtMin * Constants.kSkillPointsPerTalentPoint;
				
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Farming_1",
					formatFields = new[] {
						chanceAtMin.ToString(),
						chanceAtMax.ToString(),
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});	
			}
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Farming_2",
				formatFields = new[] {
					(Entry.GrowthTime / 60f).ToString(CultureInfo.InvariantCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}