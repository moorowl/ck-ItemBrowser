using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class StructureContentsDisplay : ObjectEntryDisplay<StructureContents> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private PugText structureTypeText;
		[SerializeField]
		private PugText structureNameText;

		public override IEnumerable<StructureContents> SortEntries(IEnumerable<StructureContents> entries) {
			return entries.OrderByDescending(entry => entry.Amount).ThenByDescending(entry => entry.Scene ?? entry.Dungeon);
		}
		
		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectData.objectID,
				variation = ObjectData.variation
			}, Entry.Amount);

			if (Entry.Scene != null) {
				structureTypeText.Render("ItemBrowser:StructureType/Scene");
				structureNameText.Render(Entry.Scene);
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
						Entry.Scene
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor
				});
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/StructureContents_2_Scene",
					formatFields = new[] {
						Entry.Amount.ToString()
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor
				});
			} else if (Entry.Dungeon != null) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/StructureContents_1_Dungeon",
					formatFields = new[] {
						Entry.Dungeon
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor
				});
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/StructureContents_2_Dungeon",
					color = TextUtils.DescriptionColor
				});
			}
		}
	}
}