using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class NaturalSpawnInitialDisplay : ObjectEntryDisplay<NaturalSpawnInitial> {
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
		private float moreInfoOffsetWithLeftSlot;
		[SerializeField]
		private float moreInfoOffsetWithoutLeftSlot;

		public override void RenderSelf() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectData.objectID,
				variation = ObjectData.variation,
				amount = Entry.AmountToSpawn
			});

			var isFromBiome = Entry.SpawnCheck.biome != Biome.None;
			
			if (isFromBiome) {
				// Specific biome + Tile
				MoreInfo.transform.localPosition = new Vector3(moreInfoOffsetWithLeftSlot, MoreInfo.transform.localPosition.y, MoreInfo.transform.localPosition.z);
				leftSourceSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				leftSourceSlot.DisplayedObject = new DisplayedObject.BiomeIcon(Entry.SpawnCheck.biome);
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(Entry.SpawnCheck.tileType, Entry.TilesetToSpawnOn);
			} else {
				// Any biome + Tile
				MoreInfo.transform.localPosition = new Vector3(moreInfoOffsetWithoutLeftSlot, MoreInfo.transform.localPosition.y, MoreInfo.transform.localPosition.z);
				leftSourceSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(Entry.SpawnCheck.tileType, Entry.TilesetToSpawnOn);
			}

			var chance = LootUtils.GetChanceForActiveWorld(Entry.SpawnCheck.spawnChance);
			chanceText.Render((chance * 100).ToString("0.##") + "%");
			
			RenderMoreInfo(isFromBiome, chance);
		}

		private void RenderMoreInfo(bool isFromBiome, float spawnChance) {
			if (isFromBiome) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_0_SpecificBiome",
					formatFields = new[] {
						$"BiomeNames/{Entry.SpawnCheck.biome}"
					},
					color = TextUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_0_AnyBiome",
					color = TextUtils.DescriptionColor
				});
			}
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_1",
				formatFields = new[] {
					TextUtils.FormatChance(spawnChance)	
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_2",
				color = TextUtils.DescriptionColor
			});
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_3",
				formatFields = new[] {
					TileUtils.GetLocalizedDisplayName(Entry.SpawnCheck.tileType, Entry.TilesetToSpawnOn)
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});

			var adjacentTiles = Entry.SpawnCheck.adjacentTiles.list;
			if (adjacentTiles.Count > 0) {
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_4",
					color = TextUtils.DescriptionColor
				});
				
				foreach (var adjacentTile in adjacentTiles) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_3",
						formatFields = new[] {
							TileUtils.GetLocalizedDisplayName(adjacentTile.tileType, adjacentTile.mustAlsoMatchTileset ? adjacentTile.tileset : null)
						},
						dontLocalizeFormatFields = true,
						color = TextUtils.DescriptionColor
					});
				}
			}
		}
	}
}