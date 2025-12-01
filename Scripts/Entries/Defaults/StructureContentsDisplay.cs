using System.Collections.Generic;
using System.Linq;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class StructureContentsDisplay : ObjectEntryDisplay<StructureContents> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private PugText structureTypeText;
		[SerializeField]
		private PugText structureNameText;

		public override IEnumerable<StructureContents> SortEntries(IEnumerable<StructureContents> entries) {
			return entries.OrderByDescending(entry => entry.Dungeon != null ? 1 : 0)
				.ThenByDescending(entry => entry.Result.Amount)
				.ThenByDescending(entry => entry.Scene ?? entry.Dungeon);
		}
		
		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation
			}, Entry.Result.Amount);

			if (Entry.Scene != null) {
				structureTypeText.Render("ItemBrowser:StructureType/Scene");
				structureNameText.Render(StructureUtils.GetPersistentSceneName(Entry.Scene));
			} else if (Entry.Dungeon != null) {
				structureTypeText.Render("ItemBrowser:StructureType/Dungeon");
				structureNameText.Render(Entry.Dungeon);
			}
		}

		private void RenderMoreInfo() {
			if (Entry.Scene != null) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/StructureContents_1_Scene",
					formatFields = new[] {
						StructureUtils.GetPersistentSceneName(Entry.Scene)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/StructureContents_2_Scene",
					formatFields = new[] {
						Entry.Result.Amount.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else if (Entry.Dungeon != null) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/StructureContents_1_Dungeon",
					formatFields = new[] {
						Entry.Dungeon
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/StructureContents_2_Dungeon",
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}