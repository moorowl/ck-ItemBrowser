using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
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

		public override void RenderSelf() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result
			});

			var isFromBiome = Entry.Biome != Biome.None;
			
			if (isFromBiome) {
				// Biome + Normal water
				leftSourceSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				leftSourceSlot.DisplayedObject = new DisplayedObject.BiomeIcon(Entry.Biome);
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(TileType.water, Tileset.Dirt);
			} else {
				// Any biome + specific liquid
				leftSourceSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(TileType.water, Entry.Tileset);
			}

			var baseCatchChance = (Entry.Chance * 100).ToString("0.##");
			chanceText.Render(baseCatchChance + "%");
			catchTypeText.Render($"ItemBrowser:CatchType/{Entry.Type}");

			if (isFromBiome) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Fishing_0_Biome",
					formatFields = new[] {
						$"BiomeNames/{Entry.Biome}"
					},
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Fishing_0_Liquid",
					formatFields = new[] {
						TileUtils.GetLocalizedDisplayName(TileType.water, Entry.Tileset)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			MoreInfo.AddLine(new TextAndFormatFields {
				text = $"ItemBrowser:MoreInfo/Fishing_1_{Entry.Type}",
				formatFields = new[] {
					baseCatchChance
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}