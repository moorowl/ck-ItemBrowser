using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class CattleProduceDisplay : ObjectEntryDisplay<CattleProduce> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot cattleSlot;
		[SerializeField]
		private BasicItemSlot[] feedSlots;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result
			}, Entry.Amount);
			cattleSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Cattle
			}); 
			
			foreach (var slot in feedSlots)
				slot.gameObject.SetActive(false);
			
			for (var i = 0; i < Entry.SuitableFeed.Count; i++) {
				if (i >= feedSlots.Length)
					break;
				
				var slot = feedSlots[i];
				slot.gameObject.SetActive(true);
				slot.DisplayedObject = new DisplayedObject.Tag(Entry.SuitableFeed[i], Entry.SuitableFeedRequired);
			}
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = Entry.SuitableFeedRequired != 1 ? "ItemBrowser:MoreInfo/CattleProduce_0_Plural" : "ItemBrowser:MoreInfo/CattleProduce_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayName(Entry.Result),
					Entry.SuitableFeedRequired.ToString()
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/CattleProduce_1",
				color = TextUtils.DescriptionColor
			});
			foreach (var feed in Entry.SuitableFeed) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/CattleProduce_2",
					formatFields = new[] {
						$"ItemBrowser:ObjectCategoryNames/{feed}"
					},
					color = TextUtils.DescriptionColor
				});
			}
		}
	}
}