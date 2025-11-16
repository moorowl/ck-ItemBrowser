using System.Linq;
using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class UpgradeMaterialDisplay : ObjectEntryDisplay<UpgradeMaterial> {
		[SerializeField]
		private BasicItemSlot bigMaterialSlot;
		[SerializeField]
		private BasicItemSlot[] smallMaterialSlots;
		[SerializeField]
		private PugText levelText;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			var primaryMaterial = Entry.Materials.FirstOrDefault(material => material.Id == RegisteredTo.objectID);
			if (primaryMaterial.Id == ObjectID.None)
				primaryMaterial = Entry.Materials[0];
			
			bigMaterialSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = primaryMaterial.Id,
				amount = primaryMaterial.Amount
			});

			foreach (var slot in smallMaterialSlots)
				slot.gameObject.SetActive(false);

			var slotIndex = 0;
			foreach (var material in Entry.Materials) {
				if (material.Id == primaryMaterial.Id)
					continue;
				
				var slot = smallMaterialSlots[slotIndex];
				slot.gameObject.SetActive(true);
				slot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
					objectID = material.Id,
					amount = material.Amount
				});
				
				slotIndex++;
				if (slotIndex >= smallMaterialSlots.Length)
					break;
			}
			
			levelText.Render($"{Entry.Level.From} -> {Entry.Level.To}");
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/UpgradeMaterial_0",
				formatFields = new[] {
					Entry.Level.From.ToString(),
					Entry.Level.To.ToString()
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});
			
			// "Materials" header
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/UpgradeMaterial_1",
				color = TextUtils.DescriptionColor
			});
			
			// Materials list
			foreach (var material in Entry.Materials) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/UpgradeMaterial_2",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayName(material.Id),
						material.Amount.ToString()
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor
				});
			}
		}
	}
}