using ItemBrowser.Utilities;
using ItemBrowser.Browser;
using PugMod;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class LockedChestDropsDisplay : ObjectEntryDisplay<LockedChestDrops> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot biomeSlot;
		[SerializeField]
		private BasicItemSlot blockSlot;
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
			biomeSlot.DisplayedObject = new DisplayedObject.BiomeIcon(Entry.RequiredBiome);
			blockSlot.DisplayedObject = new DisplayedObject.Tile(TileType.wall, Entry.RequiredTileset);
			
			chanceText.Render(TextUtils.FormatChance(Entry.Chance) + "%");
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/LockedChestDrops_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayName(PugDatabase.TryGetTileItemInfo(TileType.wall, (int) Entry.RequiredTileset).objectID),
					API.Localization.GetLocalizedTerm($"BiomeNames/{Entry.RequiredBiome}") ?? "???"
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});
			MoreInfo.AddLine(new TextAndFormatFields {
				text = $"ItemBrowser:MoreInfo/LockedChestDrops_1",
				formatFields = new[] {
					TextUtils.FormatChance(Entry.Chance) + "%"
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});
		}
	}
}