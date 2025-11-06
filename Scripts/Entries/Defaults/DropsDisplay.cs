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
		private BasicButton structureExclusiveIcon;

		private int _lastPlayerCount;

		public override IEnumerable<Drops> SortEntries(IEnumerable<Drops> entries) {
			return entries
				.OrderByDescending(entry => entry.FoundInScenes.Count > 0 ? 1 : 0)
				.ThenBy(entry => ObjectUtils.GetLocalizedDisplayName(entry.Entity, entry.EntityVariation))
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
			_lastPlayerCount = WorldUtils.ClientPlayerCount;
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectData.objectID,
				variation = ObjectData.variation
			}, Entry.Amount());
			sourceSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Entity,
				variation = Entry.EntityVariation
			});
			
			var showPoolTypeText = Entry.IsFromTableWithGuaranteedPool;
			var chanceForOne = TextUtils.FormatChance(Entry.ChanceForOne());

			chanceForOneText.Render(chanceForOne + "%");
			chanceForOneText.transform.localPosition = new Vector3(
				chanceForOneText.transform.localPosition.x,
				showPoolTypeText ? textOffsetWhenShowingBoth : 0f,
				chanceForOneText.transform.localPosition.z
			);
			
			poolTypeText.gameObject.SetActive(showPoolTypeText);
			if (showPoolTypeText)
				poolTypeText.Render(Entry.IsFromGuaranteedPool ? "ItemBrowser:GuaranteedPool" : "ItemBrowser:RandomPool");
			
			structureExclusiveIcon.gameObject.SetActive(Entry.FoundInScenes.Count > 0);
		}

		private void RenderMoreInfo() {
			var showPoolTypeText = Entry.IsFromTableWithGuaranteedPool;
			var rolls = TextUtils.FormatAmountOrRollsRange(Entry.Rolls());
			var chanceForOne = TextUtils.FormatChance(Entry.ChanceForOne());
			var chancePerRoll = TextUtils.FormatChance(Entry.Chance);

			MoreInfo.AddLine(new TextAndFormatFields {
				text = showPoolTypeText ? (Entry.IsFromGuaranteedPool ? "ItemBrowser:MoreInfo/Drops_0_GuaranteedPool" : "ItemBrowser:MoreInfo/Drops_0_RandomPool") : "ItemBrowser:MoreInfo/Drops_0",
				formatFields = new[] {
					ObjectUtils.GetUnlocalizedDisplayName(Entry.Entity, Entry.EntityVariation)
				},
				color = TextUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			if (chanceForOne != chancePerRoll) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Drops_1_ForOne",
					formatFields = new[] {
						chanceForOne
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor
				});
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Drops_1_PerRoll",
					formatFields = new[] {
						chancePerRoll,
						rolls
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Drops_1",
					formatFields = new[] {
						chanceForOne
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor
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
						color = TextUtils.DescriptionColor
					});	
				}
				
				if (Entry.OnlyDropsInSeason != Season.None) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/Drops_3",
						formatFields = new[] {
							$"Seasons/{Entry.OnlyDropsInSeason}"
						},
						color = TextUtils.DescriptionColor
					});	
				}

				if (playerCount > 1) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/Drops_4",
						formatFields = new[] {
							playerCount.ToString()
						},
						dontLocalizeFormatFields = true,
						color = TextUtils.DescriptionColor
					});	
				}
			}
			
			if (Entry.FoundInScenes.Count > 0) {
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Drops_5",
					color = TextUtils.DescriptionColor
				});

				foreach (var scene in Entry.FoundInScenes) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/Drops_6",
						formatFields = new[] {
							StructureUtils.GetPersistentSceneName(scene.Name),
							scene.Amount.ToString()
						},
						dontLocalizeFormatFields = true,
						color = TextUtils.DescriptionColor
					});	
				}
			}
		}
	}
}