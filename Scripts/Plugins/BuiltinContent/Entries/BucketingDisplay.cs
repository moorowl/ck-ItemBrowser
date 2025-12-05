using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public class BucketingDisplay : ObjectEntryDisplay<Bucketing> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot liquidOrPitSlot;
		[SerializeField]
		private BasicItemSlot bucketSlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			if (Entry.IsEmptying) {
				bucketSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
					objectID = Entry.FilledBucket.Id,
					variation = Entry.FilledBucket.Variation
				});
				resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
					objectID = Entry.EmptyBucket.Id,
					variation = Entry.EmptyBucket.Variation
				});
				liquidOrPitSlot.DisplayedObject = new DisplayedObject.Tile(TileType.pit);
			} else {
				resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
					objectID = Entry.FilledBucket.Id,
					variation = Entry.FilledBucket.Variation
				});
				bucketSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
					objectID = Entry.EmptyBucket.Id,
					variation = Entry.EmptyBucket.Variation
				});
				liquidOrPitSlot.DisplayedObject = new DisplayedObject.Tile(TileType.water, Entry.LiquidType);
			}
		}

		private void RenderMoreInfo() {
			if (Entry.IsEmptying) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser:MoreInfo/Bucketing_0_Pit",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.FilledBucket.Id, Entry.FilledBucket.Variation),
						TileUtils.GetLocalizedDisplayName(TileType.water, Entry.LiquidType) ?? "???"
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser:MoreInfo/Bucketing_0_Liquid",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.EmptyBucket.Id, Entry.EmptyBucket.Variation),
						TileUtils.GetLocalizedDisplayName(TileType.water, Entry.LiquidType) ?? "???"
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}