using ItemBrowser.Utilities;
using ItemBrowser.Browser;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class FishingDisplay : ObjectEntryDisplay<Fishing> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot rightSourceSlot;
		[SerializeField]
		private BasicItemSlot leftSourceSlot;
		[SerializeField]
		private PugText plusText;
		[SerializeField]
		private PugText chanceText;
		[SerializeField]
		private PugText catchTypeText;
		[SerializeField]
		private float moreInfoOffsetWithLeftSlot;
		[SerializeField]
		private float moreInfoOffsetWithoutLeftSlot;

		public override void RenderSelf() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectData.objectID,
				variation = ObjectData.variation,
				amount = 1
			});

			var isFromBiome = Entry.Biome != Biome.None;
			
			if (isFromBiome) {
				// Biome + Normal water
				MoreInfo.transform.localPosition = new Vector3(moreInfoOffsetWithLeftSlot, MoreInfo.transform.localPosition.y, MoreInfo.transform.localPosition.z);
				leftSourceSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				leftSourceSlot.DisplayedObject = new DisplayedObject.BiomeIcon(Entry.Biome);
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(TileType.water, Tileset.Dirt);
			} else {
				// Any biome + specific liquid
				MoreInfo.transform.localPosition = new Vector3(moreInfoOffsetWithoutLeftSlot, MoreInfo.transform.localPosition.y, MoreInfo.transform.localPosition.z);
				leftSourceSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(TileType.water, Entry.Tileset);
			}

			var baseCatchChance = (Entry.Chance * 100).ToString("0.##");
			chanceText.Render(baseCatchChance + "%");
			catchTypeText.Render($"ItemBrowser:CatchType/{Entry.Type}");
			
			MoreInfo.AddLine(new TextAndFormatFields {
				text = isFromBiome ? "ItemBrowser:MoreInfo/Fishing_0_Biome" : "ItemBrowser:MoreInfo/Fishing_0_Liquid",
				formatFields = new[] {
					isFromBiome ? $"BiomeNames/{Entry.Biome}" : $"ItemBrowser:LiquidNames/{Entry.Tileset}"
				},
				color = TextUtils.DescriptionColor
			});
			MoreInfo.AddLine(new TextAndFormatFields {
				text = $"ItemBrowser:MoreInfo/Fishing_1_{Entry.Type}",
				formatFields = new[] {
					baseCatchChance
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});
		}
	}
}