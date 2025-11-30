using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class JewelryCrafterDisplay : ObjectEntryDisplay<JewelryCrafter> {
		[SerializeField]
		private BasicItemSlot unpolishedSlot;
		[SerializeField]
		private BasicItemSlot polishedSlot;
		[SerializeField]
		private PugText chanceText;

		public override void RenderSelf() {
			var chanceAtMin = Manager.mod.SkillTalentsTable.skillTalentTrees.SelectMany(tree => tree.skillTalents)
				.FirstOrDefault(talent => talent.givesCondition == ConditionID.ChanceForPolishedJewelry).conditionValuePerPoint;
			var chanceAtMax = chanceAtMin * Constants.kSkillPointsPerTalentPoint;
			
			RenderBody((chanceAtMin, chanceAtMax));
			RenderMoreInfo((chanceAtMin, chanceAtMax));
		}

		private void RenderBody((float Min, float Max) chance) {
			unpolishedSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.UnpolishedVersion
			});
			polishedSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.PolishedVersion
			});
			chanceText.Render($"{chance.Min}-{chance.Max}%");
		}

		private void RenderMoreInfo((float Min, float Max) chance) {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/JewelryCrafter_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.UnpolishedVersion)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});

			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/JewelryCrafter_1",
				formatFields = new[] {
					chance.Min.ToString(),
					chance.Max.ToString(),
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});	
		}
	}
}