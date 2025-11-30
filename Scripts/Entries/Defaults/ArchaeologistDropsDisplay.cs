using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class ArchaeologistDropsDisplay : ObjectEntryDisplay<ArchaeologistDrops> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private PugText chanceText;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result
			});
			chanceText.Render($"{UserInterfaceUtils.FormatChance(Entry.Chance.Min)}-{UserInterfaceUtils.FormatChance(Entry.Chance.Max)}%");
		}
		
		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/ArchaeologistDrops_0",
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddLine(new TextAndFormatFields {
				text = $"ItemBrowser:MoreInfo/ArchaeologistDrops_1",
				formatFields = new[] {
					UserInterfaceUtils.FormatChance(Entry.Chance.Min),
					UserInterfaceUtils.FormatChance(Entry.Chance.Max)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}