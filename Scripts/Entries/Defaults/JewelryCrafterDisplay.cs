using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class JewelryCrafterDisplay : ObjectEntryDisplay<JewelryCrafter> {
		[SerializeField]
		private BasicItemSlot unpolishedSlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			unpolishedSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.UnpolishedVersion
			});
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/JewelryCrafter_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.UnpolishedVersion)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			var chanceAtMin = Manager.mod.SkillTalentsTable.skillTalentTrees.SelectMany(tree => tree.skillTalents)
				.FirstOrDefault(talent => talent.givesCondition == ConditionID.ChanceForPolishedJewelry).conditionValuePerPoint;
			var chanceAtMax = chanceAtMin * Constants.kSkillPointsPerTalentPoint;
				
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/JewelryCrafter_1",
				formatFields = new[] {
					chanceAtMin.ToString(),
					chanceAtMax.ToString(),
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});	
		}
	}
}