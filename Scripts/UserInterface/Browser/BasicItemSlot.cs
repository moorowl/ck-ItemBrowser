using System.Collections.Generic;
using ItemBrowser.Config;
using ItemBrowser.Entries;
using ItemBrowser.Utilities;
using PlayerEquipment;
using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class BasicItemSlot : SlotUIBase {
		private static DisplayedObject EmptyDisplayedObject => new DisplayedObject.Static(new ObjectDataCD());
		
		[SerializeField]
		private ColorReplacer colorReplacer;
		[SerializeField]
		private Sprite[] rarityBorders;
		[SerializeField]
		private bool preferSmallIcons;
		[SerializeField]
		private bool linksSource;
		[SerializeField]
		private bool linksUsage;
		[SerializeField]
		private bool showAmountInTitle;

		private DisplayedObject _displayedObject = EmptyDisplayedObject;
		public DisplayedObject DisplayedObject {
			get => _displayedObject;
			set {
				_displayedObject = value ?? EmptyDisplayedObject;
				UpdateVisuals();
			}
		}
		
		public bool IsSelected => hoverBorder.gameObject.activeSelf;
		public bool IsFavorited => Options.FavoritedObjects.Contains(FavoritedKey);
		private ObjectDataCD FavoritedKey => new() {
			objectID = DisplayedObject.ContainedObject.objectID,
			variation = DisplayedObject.ContainedObject.variation
		};

		private BoxCollider _boxCollider;
		private UIScrollWindow _scrollWindow;
		private Transform _displayTransform;
		
		public override float localScrollPosition => transform.localPosition.y + (_displayTransform != null ? _displayTransform.localPosition.y : -0.625f);
		private bool ShowHoverWindow => _scrollWindow == null || _scrollWindow.IsShowingPosition(localScrollPosition);
		public override bool isVisibleOnScreen => ShowHoverWindow && base.isVisibleOnScreen;
		public override UIScrollWindow uiScrollWindow => _scrollWindow;
		
		public static bool CanCheatInObjects => Options.CheatMode && (Manager.saves.IsCreativeModeCharacter() || Manager.main.player.adminPrivileges >= 1);
		
		protected override void Awake() {
			base.Awake();
			
			_boxCollider = GetComponent<BoxCollider>();
			_scrollWindow = GetComponentInParent<UIScrollWindow>();

			if (_scrollWindow != null && _scrollWindow.gameObject.GetComponent<ObjectEntriesList>() != null)
				_displayTransform = GetComponentInParent<ObjectEntryDisplayBase>().transform;

			icon.material = new Material(Shader.Find("Amplify/UISpriteColorReplace"));
			UpdateVisuals();
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			_displayedObject.Update(this);
			
			if (highlightBorder != null) {
				UpdateFavoriting();
				highlightBorder.gameObject.SetActive(IsFavorited);
			}
		}

		private void UpdateFavoriting() {
			var input = Manager.input.singleplayerInputModule;
			
			if (IsSelected && input.WasButtonPressedDownThisFrame(PlayerInput.InputType.LOCKING_TOGGLE)) {
				if (IsFavorited) {
					AudioManager.Sfx(SfxTableID.inventorySFXSlotLock, transform.position);
					Options.FavoritedObjects.Remove(FavoritedKey);
				} else {
					AudioManager.Sfx(SfxTableID.inventorySFXSlotUnlock, transform.position);
					Options.FavoritedObjects.Add(FavoritedKey);
				}

				OnFavoritedStateChanged();
				Options.Save();
			}
		}

		protected virtual void OnFavoritedStateChanged() { }

		public override void OnSelected() {
			_scrollWindow?.MoveScrollToIncludePosition(localScrollPosition, _boxCollider != null ? Mathf.Max(_boxCollider.size.x, _boxCollider.size.y) : 0f);
			OnSelectSlot();
		}

		public override void OnDeselected(bool playEffect = true) {
			OnDeselectSlot();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			var input = Manager.input.singleplayerInputModule;
			var containedObjectData = _displayedObject.ContainedObject.objectData;
			
			if (containedObjectData.objectID != ObjectID.None && input.IsButtonCurrentlyDown(PlayerInput.InputType.PICK_UP_HALF) && CanCheatInObjects) {
				var pickUpTen = mod1 && PugDatabase.GetObjectInfo(containedObjectData.objectID, containedObjectData.variation) is { isStackable: true };

				var player = Manager.main.player;
				player.playerCommandSystem.CreateAndDropEntity(containedObjectData.objectID, player.WorldPosition, pickUpTen ? 10 : 1, player.entity, containedObjectData.variation);
				UserInterfaceUtils.PlayItemTwitchSound(transform);
				
				return;
			}

			if (linksSource)
				_displayedObject.ShowEntries(this, ObjectEntryType.Source);
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnRightClicked(mod1, mod2);

			if (linksUsage)
				_displayedObject.ShowEntries(this, ObjectEntryType.Usage);
		}

		public override TextAndFormatFields GetHoverTitle() {
			var title = _displayedObject.GetHoverTitle(this);
			var visualObject = _displayedObject.VisualObject;
			var amount = _displayedObject.Amount;
			
			if (showAmountInTitle && visualObject.objectID != ObjectID.None && amount.Max > 1) {
				return new TextAndFormatFields {
					text = "ItemBrowser:NameAndAmountFormat",
					formatFields = new[] {
						title.text,
						amount.Min != amount.Max ? $"{amount.Min}-{amount.Max}" : amount.Min.ToString()
					},
					dontLocalizeFormatFields = true,
					color = Manager.text.GetRarityColor(PugDatabase.GetObjectInfo(visualObject.objectID).rarity)
				};
			}

			return title;
		}
		
		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = _displayedObject.GetHoverDescription(this) ?? new List<TextAndFormatFields>();
			
			var containedObjectData = _displayedObject.ContainedObject.objectData;
			if (containedObjectData.objectID != ObjectID.None) {
				if (IsFavorited) {
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser:Favorited",
						color = Color.yellow
					});	
				}
				
				if (CanCheatInObjects) {
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser:CheatModeEnabled",
						color = Manager.ui.brokenColor
					});
				}
			}
			
			return lines;
		}
		
		public override List<TextAndFormatFields> GetHoverStats(bool previewReinforced) {
			return _displayedObject.GetHoverStats(this, previewReinforced);
		}
		
		public override bool GetDurabilityOrFullnessOrXp(out int durability, out int maxDurability, out AmountType amountType) {
			durability = 0;
			maxDurability = 0;
			amountType = AmountType.Durability;
			
			var slotObject = GetSlotObject();
			if (slotObject.objectID == ObjectID.None)
				return false;

			if (PugDatabase.HasComponent<DurabilityCD>(slotObject.objectData)) {
				maxDurability = PugDatabase.GetComponent<DurabilityCD>(slotObject.objectData).maxDurability;
				durability = maxDurability;
				return true;
			}
			
			if (PugDatabase.HasComponent<FullnessCD>(slotObject.objectData)) {
				amountType = AmountType.Fullness;
				maxDurability = PugDatabase.GetComponent<FullnessCD>(slotObject.objectData).maxFullness;
				durability = maxDurability;
				return true;
			}
			
			if (PugDatabase.HasComponent<PetCD>(slotObject.objectData)) {
				if (PetExtensions.IsAtMaxLevel(slotObject.amount))
					return false;

				amountType = AmountType.Experience;
				maxDurability = PetExtensions.GetTotalXpNeededToLevelUp(slotObject.amount);
				durability = 0;
				return true;
			}
			
			return false;
		}

		public override HoverTitleIconType GetHoverTitleIconType() {
			return HoverTitleIconType.None;
		}

		public override HoverWindowAlignment GetHoverWindowAlignment() {
			return UserInterfaceUtils.IsUsingMouseAndKeyboard ? HoverWindowAlignment.BOTTOM_RIGHT_OF_CURSOR : HoverWindowAlignment.BOTTOM_RIGHT_OF_SCREEN;
		}

		protected override ContainedObjectsBuffer GetSlotObject() {
			return _displayedObject.ContainedObject;
		}
		
		public override UIelement GetAdjacentUIElement(Direction.Id dir, Vector3 currentPosition) {
			return SnapPoint.TryFindNextSnapPoint(this, dir)?.AttachedElement;
		}

		public void UpdateVisuals() {
			background.sprite = rarityBorders[0];
			if (highlightBorder != null)
				highlightBorder.gameObject.SetActive(false);

			var visualObject = DisplayedObject.VisualObject;
			RenderAmountNumberRange(DisplayedObject.Amount);

			icon.transform.localScale = Vector3.one;
			colorReplacer.UpdateColorReplacerFromObjectData(visualObject);
			Manager.ui.ApplyAnyIconGradientMap(visualObject, icon);
			
			if (visualObject.objectID == ObjectID.None) {
				SetEmptyIcon();
				return;
			}

			if (!PugDatabase.TryGetObjectInfo(visualObject.objectID, out var objectInfo, visualObject.variation)) {
				SetMissingIcon();
				return;
			}

			var iconToUse = ObjectUtils.GetIcon(visualObject.objectID, visualObject.variation, preferSmallIcons);
			if (iconToUse == null) {
				SetMissingIcon();
				return;
			}

			icon.sprite = iconToUse;
			icon.transform.localPosition = objectInfo.iconOffset;
			
			var spriteSize = icon.sprite.bounds.size;
			if (spriteSize is { x: > 1f, y: > 1f } && IsCarriedObject(objectInfo)) {
				spriteSize.x = 1f;
				spriteSize.y = 1f;
			}
			var scale = Mathf.Min(1f / spriteSize.x, 1f / spriteSize.y);
			icon.transform.localScale = new Vector3(scale, scale, 1f);
			
			colorReplacer.UpdateColorReplacerFromObjectData(visualObject);
			Manager.ui.ApplyAnyIconGradientMap(visualObject, icon);
			icon.transform.localPosition = objectInfo.iconOffset;

			var rarityIndex = (int) objectInfo.rarity;
			if (rarityIndex >= 0 && rarityIndex < rarityBorders.Length)
				background.sprite = rarityBorders[rarityIndex];
		}
		
		private bool RenderAmountNumberRange((int Min, int Max) amount) {
			if (amountNumber == null)
				return false;
			
			if (amount.Max > 1 && !AmountIsShownAsBar()) {
				var text = amount.Min != amount.Max ? $"{amount.Min}-{amount.Max}" : amount.Min.ToString();
				
				amountNumber.Render(text);
				if (amountNumberShadow != null)
					amountNumberShadow.Render(text);

				return true;
			}
			
			return false;
		}

		private static readonly HashSet<EquipmentSlotType> CarriedEquipmentSlotTypes = new() {
			EquipmentSlotType.MeleeWeaponSlot,
			EquipmentSlotType.RangeWeaponSlot,
			EquipmentSlotType.ShovelSlot,
			EquipmentSlotType.HoeSlot,
			EquipmentSlotType.BugNet,
			EquipmentSlotType.SeederSlot,
			EquipmentSlotType.Shield,
			EquipmentSlotType.InstrumentSlot,
			EquipmentSlotType.FishingRodSlot
		};

		private static bool IsCarriedObject(ObjectInfo objectInfo) {
			var objectType = objectInfo.objectType;
			var equipmentSlotType = PlayerController.GetEquippedSlotTypeForObjectType(objectType, default, default, default);

			return objectType != ObjectType.ThrowingWeapon && CarriedEquipmentSlotTypes.Contains(equipmentSlotType);
		}
	}
}