using ItemBrowser.Browser;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class NaturalSpawnAroundObjectDisplay : ObjectEntryDisplay<NaturalSpawnAroundObject> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot biomeOrTilesetSlot;
		[SerializeField]
		private BasicItemSlot seasonSlot;
		[SerializeField]
		private BasicItemSlot entitySlot;
		[SerializeField]
		private PugText plusTextRight;
		[SerializeField]
		private PugText plusTextLeft;
		[SerializeField]
		private float moreInfoOffsetFromSlot;

		public override void RenderSelf() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation
			});
			entitySlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Entity.Id,
				variation = Entry.Entity.Variation
			});
			
			biomeOrTilesetSlot.gameObject.SetActive(false);
			seasonSlot.gameObject.SetActive(false);
			plusTextRight.gameObject.SetActive(false);
			plusTextLeft.gameObject.SetActive(false);
			
			if (Entry.SpawnsInBiome != null) {
				biomeOrTilesetSlot.gameObject.SetActive(true);
				biomeOrTilesetSlot.DisplayedObject = new DisplayedObject.BiomeIcon(Entry.SpawnsInBiome.Value);
				plusTextRight.gameObject.SetActive(true);
			}
			if (Entry.SpawnsInTileset != null) {
				biomeOrTilesetSlot.gameObject.SetActive(true);
				biomeOrTilesetSlot.DisplayedObject = new DisplayedObject.Tile(TileType.ground, Entry.SpawnsInTileset.Value);
				plusTextRight.gameObject.SetActive(true);
			}
			if (Entry.SpawnsInSeason != null) {
				seasonSlot.gameObject.SetActive(true);
				seasonSlot.DisplayedObject = new DisplayedObject.SeasonIcon(Entry.SpawnsInSeason.Value);
				plusTextLeft.gameObject.SetActive(true);
			}

			var leftMostSlot = seasonSlot.gameObject.activeSelf ? seasonSlot.transform : biomeOrTilesetSlot.transform;
			MoreInfo.transform.position = new Vector3(leftMostSlot.position.x - moreInfoOffsetFromSlot, MoreInfo.transform.position.y, MoreInfo.transform.position.z);
			
			//RenderMoreInfo(isFromBiome, chance);
		}

		private void RenderMoreInfo(bool isFromBiome, float spawnChance) {
			/*if (isFromBiome) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_0_SpecificBiome",
					formatFields = new[] {
						$"BiomeNames/{Entry.SpawnCheck.biome}"
					},
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_0_AnyBiome",
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_1",
				formatFields = new[] {
					UserInterfaceUtils.FormatChance(spawnChance)	
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_2",
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_3",
				formatFields = new[] {
					TileUtils.GetLocalizedDisplayName(Entry.SpawnCheck.tileType, Entry.TilesetToSpawnOn)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});

			var adjacentTiles = Entry.SpawnCheck.adjacentTiles.list;
			if (adjacentTiles.Count > 0) {
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_4",
					color = UserInterfaceUtils.DescriptionColor
				});
				
				foreach (var adjacentTile in adjacentTiles) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/NaturalSpawnInitial_3",
						formatFields = new[] {
							TileUtils.GetLocalizedDisplayName(adjacentTile.tileType, adjacentTile.mustAlsoMatchTileset ? adjacentTile.tileset : null)
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtils.DescriptionColor
					});
				}
			}*/
		}
	}
}