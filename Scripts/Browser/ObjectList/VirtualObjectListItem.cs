using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Entries;
using ItemBrowser.Entries.Defaults;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Browser {
	public class VirtualObjectListItem : BasicItemSlot {
		[SerializeField]
		private FiltersPanel filtersPanel;
		[SerializeField]
		private ObjectListWindow objectListWindow;
		
		public void SetObjectData(ObjectData objectData, VirtualObjectList craftingSelectorUI) {
			DisplayedObject = new DisplayedObject.Static(objectData);
			slotsUIContainer = craftingSelectorUI;
		}

		protected override void OnFavoritedStateChanged() {
			objectListWindow.RequestListRefresh(true);
		}

		public override List<PugDatabase.MaterialInfo> GetRequiredMaterials(bool isRepairing, bool isReinforcing) {
			if (!filtersPanel.DisplayItemCraftingRequirements)
				return null;
			
			var craftingHandler = Manager.main.player?.playerCraftingHandler;
			if (craftingHandler == null)
				return null;

			var slotObject = GetSlotObject();
			var objectInfo = PugDatabase.GetObjectInfo(slotObject.objectID, slotObject.variation);
			if (objectInfo == null)
				return null;
					
			var nearbyChests = craftingHandler.GetNearbyChests();
			var recipeInfo = new CraftingHandler.RecipeInfo(objectInfo, 1);

			return craftingHandler.GetCraftingMaterialInfosForRecipe(recipeInfo, nearbyChests, false, false, PugDatabase.HasComponent<CookedFoodCD>(slotObject.objectData));
		}

		public override CraftingSettings GetCraftingSettings() {
			var slotObject = GetSlotObject();
			if (!filtersPanel.DisplayItemCraftingRequirements || !PugDatabase.TryGetObjectInfo(slotObject.objectID, out var objectInfo, slotObject.variation))
				return base.GetCraftingSettings();
			
			return objectInfo.craftingSettings;
		}
		
		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = base.GetHoverDescription();
			
			var slotObject = GetSlotObject();
			if (filtersPanel.DisplayItemCraftingRequirements) {
				var craftingSources = ItemBrowserAPI.ObjectEntries.GetEntries<Crafting>(ObjectEntryType.Source, slotObject.objectID, slotObject.variation).ToList();
				if (craftingSources.Count > 0) {
					lines[^1].paddingBeneath = 0.125f;
					foreach (var craftingSource in craftingSources) {
						lines.Add(new TextAndFormatFields {
							text = craftingSource.UsesStation ? "ItemBrowser:MoreInfo/Crafting_0_Station" : "ItemBrowser:MoreInfo/Crafting_0_Recipe",
							formatFields = new[] {
								ObjectUtils.GetLocalizedDisplayNameOrDefault(craftingSource.UsesStation ? craftingSource.Station : craftingSource.Recipe)
							},
							dontLocalizeFormatFields = true,
							color = Color.white * 0.95f
						});
					}
				}
			}
			
			return lines;
		}
	}
}