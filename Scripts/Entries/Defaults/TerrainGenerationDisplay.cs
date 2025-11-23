using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class TerrainGenerationDisplay : ObjectEntryDisplay<TerrainGeneration> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot rightSourceSlot;
		[SerializeField]
		private BasicItemSlot leftSourceSlot;
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
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation
			});

			var generatesInTileset = Entry.GeneratesInTileset != null;
			if (generatesInTileset) {
				// Biome + Tileset
				MoreInfo.transform.localPosition = new Vector3(moreInfoOffsetWithLeftSlot, MoreInfo.transform.localPosition.y, MoreInfo.transform.localPosition.z);
				leftSourceSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				leftSourceSlot.DisplayedObject = new DisplayedObject.BiomeIcon(Entry.GeneratesInBiome);
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(TileType.wall, Entry.GeneratesInTileset.Value);
			} else {
				// Biome + Any tileset
				MoreInfo.transform.localPosition = new Vector3(moreInfoOffsetWithoutLeftSlot, MoreInfo.transform.localPosition.y, MoreInfo.transform.localPosition.z);
				leftSourceSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSourceSlot.DisplayedObject = new DisplayedObject.BiomeIcon(Entry.GeneratesInBiome);
			}
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/TerrainGeneration_0",
				formatFields = new[] {
					$"BiomeNames/{Entry.GeneratesInBiome}"
				},
				color = UserInterfaceUtils.DescriptionColor
			});
			
			if (Entry.GeneratesInTileset != null) {
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/TerrainGeneration_1",
					formatFields = new[] {
						TileUtils.GetLocalizedDisplayName(TileType.wall, Entry.GeneratesInTileset.Value)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}