using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class CraftingDisplay : ObjectEntryDisplay<Crafting> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot stationSlot;
		[SerializeField]
		private BasicItemSlot[] ingredientSlots;
		[SerializeField]
		private BasicItemSlot tagIngredientSlot;

		public override void RenderSelf() {
			var objectInfo = PugDatabase.GetObjectInfo(Entry.Result.Id, Entry.Result.Variation);
			var requiredObjectsToCraft = objectInfo.requiredObjectsToCraft.Where(craftingObject => craftingObject.objectID != ObjectID.None)
				.GroupBy(craftingObject => craftingObject.objectID)
				.Select(group => new CraftingObject {
					objectID = group.Key,
					amount = group.Sum(craftingObject => craftingObject.amount)
				})
				.ToList();
			var useMaterialsWithTag = objectInfo.craftingSettings.canOnlyUseAnyMaterialsWithTag;
			
			RenderBody(requiredObjectsToCraft, useMaterialsWithTag);
			RenderMoreInfo(requiredObjectsToCraft, useMaterialsWithTag);
		}

		private void RenderBody(List<CraftingObject> requiredObjectsToCraft, ObjectCategoryTag useMaterialsWithTag) {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation,
				amount = Entry.Amount
			});
			stationSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.UsesStation ? Entry.Station : Entry.Recipe
			}); 
			
			tagIngredientSlot.gameObject.SetActive(false);
			foreach (var slot in ingredientSlots)
				slot.gameObject.SetActive(false);

			if (useMaterialsWithTag != ObjectCategoryTag.None) {
				tagIngredientSlot.gameObject.SetActive(true);
				tagIngredientSlot.DisplayedObject = new DisplayedObject.Tag(useMaterialsWithTag);
			} else {
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
		}

		private void RenderMoreInfo(List<CraftingObject> requiredObjectsToCraft, ObjectCategoryTag useMaterialsWithTag) {
			// Crafted at
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Crafting_0_" + (Entry.Station == ObjectID.None ? "Recipe" : "Station"),
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault( Entry.UsesStation ? Entry.Station : Entry.Recipe)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			
			// "Materials" header
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Crafting_1",
				color = UserInterfaceUtils.DescriptionColor
			});
			
			// Materials list
			if (useMaterialsWithTag != ObjectCategoryTag.None) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Crafting_2",
					formatFields = new[] {
						API.Localization.GetLocalizedTerm($"ItemBrowser:ObjectCategoryNames/{useMaterialsWithTag}"),
						"1"
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				foreach (var craftingObject in requiredObjectsToCraft) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/Crafting_2",
						formatFields = new[] {
							ObjectUtils.GetLocalizedDisplayNameOrDefault(craftingObject.objectID),
							craftingObject.amount.ToString()
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtils.DescriptionColor
					});
				}	
			}
			
			// Crafting time
			if (Entry.CraftingTime > 0) {
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Crafting_3",
					formatFields = new[] {
						Entry.CraftingTime.ToString(CultureInfo.InvariantCulture)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});		
			}
			
			// Crafted at nearby object
			if (Entry.RequiresObjectNearby != ObjectID.None) {
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Crafting_4",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.RequiresObjectNearby)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}