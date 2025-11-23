using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class DropsDisplay : ObjectEntryDisplay<Drops> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot sourceSlot;
		[SerializeField]
		private PugText chanceForOneText;
		[SerializeField]
		private PugText poolTypeText;
		[SerializeField]
		private float textOffsetWhenShowingBoth;
		[SerializeField]
		private MoreInfoButton structureInfo;

		private int _lastPlayerCount;

		public override IEnumerable<Drops> SortEntries(IEnumerable<Drops> entries) {
			return entries
				.OrderByDescending(entry => entry.FoundInScenes.Count > 0 ? 0 : 1)
				.ThenBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Entity.Id, entry.Entity.Variation))
				.ThenByDescending(entry => entry.IsFromGuaranteedPool ? 1 : 0);
		}
		
		protected override void LateUpdate() {
			base.LateUpdate();

			if (_lastPlayerCount != WorldUtils.ClientPlayerCount)
				Render();
		}

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
			RenderStructureInfo();
			_lastPlayerCount = WorldUtils.ClientPlayerCount;
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation
			}, Entry.Amount());
			sourceSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Entity.Id,
				variation = Entry.Entity.Variation
			});
			
			var showPoolTypeText = Entry.IsFromTableWithGuaranteedPool;
			var chanceForOne = UserInterfaceUtils.FormatChance(Entry.ChanceForOne());

			chanceForOneText.Render(chanceForOne + "%");
			chanceForOneText.transform.localPosition = new Vector3(
				chanceForOneText.transform.localPosition.x,
				showPoolTypeText ? textOffsetWhenShowingBoth : 0f,
				chanceForOneText.transform.localPosition.z
			);
			
			poolTypeText.gameObject.SetActive(showPoolTypeText);
			if (showPoolTypeText)
				poolTypeText.Render(Entry.IsFromGuaranteedPool ? "ItemBrowser:GuaranteedPool" : "ItemBrowser:RandomPool");
		}

		private void RenderMoreInfo() {
			var showPoolTypeText = Entry.IsFromTableWithGuaranteedPool;
			var rolls = UserInterfaceUtils.FormatAmountOrRollsRange(Entry.Rolls());
			var chanceForOne = UserInterfaceUtils.FormatChance(Entry.ChanceForOne());
			var chancePerRoll = UserInterfaceUtils.FormatChance(Entry.Chance);

			MoreInfo.AddLine(new TextAndFormatFields {
				text = showPoolTypeText ? (Entry.IsFromGuaranteedPool ? "ItemBrowser:MoreInfo/Drops_0_GuaranteedPool" : "ItemBrowser:MoreInfo/Drops_0_RandomPool") : "ItemBrowser:MoreInfo/Drops_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Entity.Id, Entry.Entity.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			if (chanceForOne != chancePerRoll) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Drops_1_ForOne",
					formatFields = new[] {
						chanceForOne
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Drops_1_PerRoll",
					formatFields = new[] {
						chancePerRoll,
						rolls
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Drops_1",
					formatFields = new[] {
						chanceForOne
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}

			var playerCount = WorldUtils.ClientPlayerCount;
			if (Entry.OnlyDropsInBiome != Biome.None || Entry.OnlyDropsInSeason != Season.None || playerCount > 1) {
				MoreInfo.AddPadding();

				if (Entry.OnlyDropsInBiome != Biome.None) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/Drops_2",
						formatFields = new[] {
							$"BiomeNames/{Entry.OnlyDropsInBiome}"
						},
						color = UserInterfaceUtils.DescriptionColor
					});	
				}
				
				if (Entry.OnlyDropsInSeason != Season.None) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/Drops_3",
						formatFields = new[] {
							$"Seasons/{Entry.OnlyDropsInSeason}"
						},
						color = UserInterfaceUtils.DescriptionColor
					});	
				}

				if (playerCount > 1) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/Drops_4",
						formatFields = new[] {
							playerCount.ToString()
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtils.DescriptionColor
					});	
				}
			}
		}
		
		private void RenderStructureInfo() {
			structureInfo.gameObject.SetActive(Entry.FoundInScenes.Count > 0);
			if (!structureInfo.gameObject.activeSelf)
				return;
			
			structureInfo.Clear();
			structureInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:StructureExclusiveDrop"
			});
			structureInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Drops_5",
				color = UserInterfaceUtils.DescriptionColor
			});

			foreach (var scene in Entry.FoundInScenes) {
				structureInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Drops_6",
					formatFields = new[] {
						StructureUtils.GetPersistentSceneName(scene.Name)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});	
			}
		}
	}
}