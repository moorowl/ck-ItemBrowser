using ItemBrowser.Utilities;
using ItemBrowser.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class CreatureSummoningDisplay : ObjectEntryDisplay<CreatureSummoning> {
		[SerializeField]
		private BasicItemSlot creatureSlot;
		[SerializeField]
		private BasicItemSlot rightSlot;
		[SerializeField]
		private BasicItemSlot leftSlot;
		[SerializeField]
		private PugText plusText;
		[SerializeField]
		private float moreInfoOffsetWithLeftSlot;
		[SerializeField]
		private float moreInfoOffsetWithoutLeftSlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			creatureSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Creature.Id,
				variation = Entry.Creature.Variation
			});
			
			var hasSummoningArea = Entry.SummoningArea.Id != ObjectID.None;
			if (hasSummoningArea) {
				MoreInfo.transform.localPosition = new Vector3(moreInfoOffsetWithLeftSlot, MoreInfo.transform.localPosition.y, MoreInfo.transform.localPosition.z);
				leftSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				rightSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
					objectID = Entry.SummoningArea.Id,
					variation = Entry.SummoningArea.Variation
				});
				leftSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
					objectID = Entry.SummoningItem.Id,
					variation = Entry.SummoningItem.Variation
				});
			} else {
				MoreInfo.transform.localPosition = new Vector3(moreInfoOffsetWithoutLeftSlot, MoreInfo.transform.localPosition.y, MoreInfo.transform.localPosition.z);
				leftSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
					objectID = Entry.SummoningItem.Id,
					variation = Entry.SummoningItem.Variation
				});
			}
		}

		private void RenderMoreInfo() {
			if (Entry.SummoningArea.Id != ObjectID.None) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser:MoreInfo/CreatureSummoning_0_{Entry.SummoningMethod}",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.SummoningItem.Id, Entry.SummoningItem.Variation),
						ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.SummoningArea.Id, Entry.SummoningArea.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser:MoreInfo/CreatureSummoning_0_{Entry.SummoningMethod}",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.SummoningItem.Id, Entry.SummoningItem.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}