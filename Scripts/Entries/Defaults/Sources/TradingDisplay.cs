using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class TradingDisplay : ObjectEntryDisplay<Trading> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot vendorSlot;
		[SerializeField]
		private BasicItemSlot[] ingredientSlots;

		public override void RenderSelf() {
			var objectInfo = PugDatabase.GetObjectInfo(ObjectData.objectID, ObjectData.variation);
			var requiredObjectsToCraft = objectInfo.requiredObjectsToCraft.Where(craftingObject => craftingObject.objectID != ObjectID.None).ToList();
			
			RenderBody(requiredObjectsToCraft);
			RenderMoreInfo(requiredObjectsToCraft);
		}

		private void RenderBody(List<CraftingObject> requiredObjectsToCraft) {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectData.objectID,
				variation = ObjectData.variation,
				amount = Entry.Amount
			});
			vendorSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Vendor
			}); 
			
			foreach (var slot in ingredientSlots)
				slot.gameObject.SetActive(false);

			for (var i = 0; i < requiredObjectsToCraft.Count; i++) {
				if (i >= ingredientSlots.Length)
					break;
					
				var craftingObject = requiredObjectsToCraft[i];
				var slot = ingredientSlots[i];
				slot.gameObject.SetActive(true);

				slot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
					objectID = craftingObject.objectID,
					amount = craftingObject.amount
				});
			}
		}
		
		private void RenderMoreInfo(List<CraftingObject> requiredObjectsToCraft) {
			// Trading with
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Trading_0",
				formatFields = new[] {
					ObjectUtils.GetUnlocalizedDisplayName(Entry.Vendor)
				},
				color = TextUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			// Materials header
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Trading_1",
				color = TextUtils.DescriptionColor
			});
			foreach (var craftingObject in requiredObjectsToCraft) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Trading_2",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayName(craftingObject.objectID),
						craftingObject.amount.ToString()
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor
				});
			}
		}
	}
}