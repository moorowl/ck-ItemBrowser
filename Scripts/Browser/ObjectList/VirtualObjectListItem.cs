using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Entries;
using ItemBrowser.Entries.Defaults;
using UnityEngine;

namespace ItemBrowser.Browser.ObjectList {
	public class VirtualObjectListItem : BasicItemSlot {
		[SerializeField]
		private FiltersPanel filtersPanel;
		
		public override float localScrollPosition => transform.localPosition.y + transform.parent.localPosition.y;
		private VirtualObjectList VirtualObjectList => (VirtualObjectList) slotsUIContainer;
		private bool ShowHoverWindow => VirtualObjectList != null && VirtualObjectList.uiScrollWindow.IsShowingPosition(localScrollPosition, background.size.y / 2f);
		public override bool isVisibleOnScreen => ShowHoverWindow && base.isVisibleOnScreen;

		public void SetObjectData(ObjectData objectData, VirtualObjectList craftingSelectorUI) {
			DisplayedObject = new DisplayedObject.Static(objectData);
			slotsUIContainer = craftingSelectorUI;
		}

		public override void OnSelected() {
			VirtualObjectList.uiScrollWindow.MoveScrollToIncludePosition(localScrollPosition, background.size.y / 2f);
			OnSelectSlot();
		}

		public override void OnDeselected(bool playEffect = true) {
			OnDeselectSlot();
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
					// TODO support multiple stations!
					var firstCraftingSource = craftingSources[0];
					var stationContainedObject = new ContainedObjectsBuffer {
						objectData = new ObjectDataCD {
							objectID = firstCraftingSource.UsesStation ? firstCraftingSource.Station : firstCraftingSource.Recipe
						}
					};
					
					lines[^1].paddingBeneath = 0.125f;
					lines.Add(new TextAndFormatFields {
						text = "Crafted at " + PlayerController.GetObjectName(stationContainedObject, true).text,
						dontLocalize = true,
						color = Color.white * 0.95f
					});
				}
			}
			
			return lines;
		}
	}
}