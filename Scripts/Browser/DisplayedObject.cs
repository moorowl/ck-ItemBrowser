using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Entries;
using ItemBrowser.Utilities;
using PugMod;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Browser {
	public abstract class DisplayedObject {
		public virtual ContainedObjectsBuffer ContainedObject => default;
		public virtual ContainedObjectsBuffer VisualObject => ContainedObject;
		public virtual (int Min, int Max) Amount => (ContainedObject.amount, ContainedObject.amount);
			
		public virtual void Update(SlotUIBase slot) { }
		
		public virtual bool ShowEntries(SlotUIBase slot, ObjectEntryType type) {
			return false;
		}

		public virtual TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
			return null;
		}

		public virtual List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
			return new List<TextAndFormatFields>();
		}

		public virtual List<TextAndFormatFields> GetHoverStats(SlotUIBase slot, bool previewReinforced) {
			return null;
		}
		
		public class Static : DisplayedObject {
			public override ContainedObjectsBuffer ContainedObject => new() {
				objectData = _objectData
			};
			public override (int Min, int Max) Amount { get; } = (0, 0);

			private readonly ObjectDataCD _objectData;

			public Static(ObjectDataCD objectData) {
				_objectData = objectData;
				Amount = (_objectData.amount, _objectData.amount);
			}
			
			public Static(ObjectDataCD objectData, (int Min, int Max) amount) : this(objectData) {
				Amount = amount;
			}
			
			public Static(ObjectDataCD objectData, int amount) : this(objectData) {
				Amount = (amount, amount);
			}
			
			public override bool ShowEntries(SlotUIBase slot, ObjectEntryType type) {
				return ItemBrowserAPI.ItemBrowserUI.ShowObjectEntries(_objectData, type);
			}

			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				var objectName = new TextAndFormatFields {
					text = ObjectUtils.GetUnlocalizedDisplayName(_objectData.objectID, _objectData.variation)
				};

				var objectInfo = PugDatabase.GetObjectInfo(_objectData.objectID);
				if (objectInfo != null)
					objectName.color = Manager.text.GetRarityColor(objectInfo.rarity);

				return objectName;
			}

			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				if (!API.Authoring.ObjectProperties.TryGetPropertyString(_objectData.objectID, "name", out var term))
					term = _objectData.objectID.ToString();

				var nameTermOverride = Manager.ui.itemOverridesTable.GetNameTermOverride(_objectData);
				if (nameTermOverride != null)
					term = nameTermOverride;

				var lines = new List<TextAndFormatFields> {
					new() {
						text = $"Items/{term}Desc"
					}
				};

				if (_objectData.objectID != ObjectID.None) {
					var associatedMod = ModUtils.GetAssociatedMod(_objectData.objectID);
					lines.Add(new TextAndFormatFields {
						text = ModUtils.GetDisplayName(associatedMod),
						dontLocalize = true,
						color = TextUtils.DescriptionColor
					});
				}

				return lines;
			}

			public override List<TextAndFormatFields> GetHoverStats(SlotUIBase slot, bool previewReinforced) {
				return slot.GetHoverStats(ContainedObject, previewReinforced, false);
			}
		}

		public class Tag : DisplayedObject {
			private const float CycleTimer = 1f;
			
			public override ContainedObjectsBuffer VisualObject => new() {
				objectData = _objectsToDisplay.Count > 0 ? _objectsToDisplay[_currentObjectIndex] : default
			};

			private readonly ObjectCategoryTag _tag;
			private readonly int _amount;
			private readonly List<ObjectDataCD> _objectsToDisplay;

			private int _currentObjectIndex;
			private float _lastChangedObjectTime;

			public Tag(ObjectCategoryTag tag, int amount = 1) {
				_tag = tag;
				_objectsToDisplay = GetObjectsToDisplay(tag).Select(objectData => new ObjectDataCD {
					objectID = objectData.objectID,
					variation = objectData.variation,
					amount = amount
				}).ToList();
				_currentObjectIndex = 0;
				_lastChangedObjectTime = 0f;
			}

			public override void Update(SlotUIBase slot) {
				if (Time.time >= _lastChangedObjectTime + CycleTimer) {
					_currentObjectIndex++;
					if (_currentObjectIndex >= _objectsToDisplay.Count)
						_currentObjectIndex = 0;

					_lastChangedObjectTime = Time.time;
					if (slot is BasicItemSlot basicItemSlot)
						basicItemSlot.UpdateVisuals();
				}
			}

			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				return new TextAndFormatFields {
					text = $"ItemBrowser:ObjectCategoryNames/{_tag}"
				};
			}

			private static IEnumerable<ObjectDataCD> GetObjectsToDisplay(ObjectCategoryTag tag) {
				var allObjectsWithTag = GetAllObjectsWithTag(tag);

				return tag switch {
					ObjectCategoryTag.UncommonOrLowerCookedFood or ObjectCategoryTag.RareOrHigherCookedFood => allObjectsWithTag.Select(objectData => new ObjectDataCD {
						objectID = objectData.objectID,
						variation = CookedFoodCD.GetFoodVariation(ObjectID.HeartBerry, ObjectID.GlowingTulipFlower)
					}),
					ObjectCategoryTag.CattlePlantFood => allObjectsWithTag.Where(objectData => !PugDatabase.HasComponent<PlantCD>(objectData)),
					_ => allObjectsWithTag
				};
			}

			private static IEnumerable<ObjectDataCD> GetAllObjectsWithTag(ObjectCategoryTag tag) {
				return PugDatabase.objectsByType.Keys.Where(objectData => objectData.variation == 0 && PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).tags.Contains(tag));
			}
		}
		
		public class Tile : DisplayedObject {
			public override ContainedObjectsBuffer ContainedObject => _staticObject?.ContainedObject ?? new ContainedObjectsBuffer();
			public override ContainedObjectsBuffer VisualObject => _staticObject?.VisualObject ?? new ContainedObjectsBuffer();

			private readonly TileType _tileType;
			private readonly Tileset _tileset;
			private readonly Static _staticObject;

			public Tile(TileType tileType, Tileset? tileset = null) {
				_tileType = tileType;
				_tileset = tileset ?? Tileset.MAX_VALUE;

				if (tileset == null)
					return;
				
				var objectInfo = PugDatabase.TryGetTileItemInfo(_tileType == TileType.ground ? TileType.wall : _tileType, (int) tileset);
				if (objectInfo != null) {
					_staticObject = new Static(new ObjectDataCD {
						objectID = objectInfo.objectID,
						variation = objectInfo.variation
					});
				}
			}
			
			public override bool ShowEntries(SlotUIBase slot, ObjectEntryType type) {
				return _staticObject != null && _staticObject.ShowEntries(slot, type);
			}

			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				return new TextAndFormatFields {
					text = TileUtils.GetLocalizedDisplayName(_tileType, _tileset == Tileset.MAX_VALUE ? null : _tileset),
					dontLocalize = true
				};
			}

			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				return _staticObject != null ? _staticObject.GetHoverDescription(slot) : base.GetHoverDescription(slot);
			}

			public override List<TextAndFormatFields> GetHoverStats(SlotUIBase slot, bool previewReinforced) {
				return _staticObject != null ? _staticObject.GetHoverStats(slot, previewReinforced) : base.GetHoverStats(slot, previewReinforced);
			}
		}
		
		public class BiomeIcon : DisplayedObject {
			public override ContainedObjectsBuffer VisualObject => new() {
				objectData = _objectData
			};
			
			private readonly Biome _biome;
			private readonly ObjectDataCD _objectData;
			
			public BiomeIcon(Biome biome) {
				_biome = biome;
				_objectData = new ObjectDataCD {
					objectID = GetBiomeIcon(_biome)
				};
			}
			
			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				return new TextAndFormatFields {
					text = $"BiomeNames/{_biome}"
				};
			}

			private static ObjectID GetBiomeIcon(Biome biome) {
				return biome switch {
					Biome.Slime => ObjectID.WallDirtBlock,
					Biome.Larva => ObjectID.WallClayBlock,
					Biome.Stone => ObjectID.WallStoneBlock,
					Biome.Nature => ObjectID.WallGrassBlock,
					Biome.Sea => ObjectID.WallLimestoneBlock,
					Biome.Desert => ObjectID.WallDesertBlock,
					Biome.Crystal => ObjectID.WallCrystalBlock,
					Biome.Passage => ObjectID.WallPassageBlock,
					_ => ObjectID.WallObsidianBlock
				};
			}
		}
		
		public class SeasonIcon : DisplayedObject {
			public override ContainedObjectsBuffer VisualObject => new() {
				objectData = _objectData
			};
			
			private readonly Season _season;
			private readonly ObjectDataCD _objectData;
			
			public SeasonIcon(Season season) {
				_season = season;
				_objectData = new ObjectDataCD {
					objectID = GetSeasonIcon(_season)
				};
			}
			
			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				return new TextAndFormatFields {
					text = $"Seasons/{_season}"
				};
			}

			private static ObjectID GetSeasonIcon(Season season) {
				return season switch {
					Season.Easter => ObjectID.EasterEggNature,
					Season.Halloween => ObjectID.PumpkinHelm,
					Season.Christmas => ObjectID.ChristmasLuxuryPresent,
					Season.Valentine => ObjectID.BoxOfChocolates,
					Season.Anniversary => ObjectID.AnniversaryCake,
					Season.CherryBlossom => ObjectID.PinkCherryFlower,
					Season.LunarNewYear => ObjectID.ChineseCoin,
					_ => ObjectID.None
				};
			}
		}
	}
}