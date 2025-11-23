using System.Collections.Generic;
using ItemBrowser.Config;
using ItemBrowser.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Browser {
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

		public bool IsHovered => hoverBorder.gameObject.activeSelf;
		public bool IsFavorited => ConfigFile.FavoritedObjects.Contains(FavoritedKey);
		private ObjectDataCD FavoritedKey => new() {
			objectID = DisplayedObject.ContainedObject.objectID,
			variation = DisplayedObject.ContainedObject.variation
		};

		private BoxCollider _boxCollider;
		private UIScrollWindow _scrollWindow;
		
		public override float localScrollPosition => transform.localPosition.y - 0.625f;
		private bool ShowHoverWindow => _scrollWindow == null || _scrollWindow.IsShowingPosition(localScrollPosition);
		public override bool isVisibleOnScreen => ShowHoverWindow && base.isVisibleOnScreen;
		public override UIScrollWindow uiScrollWindow => _scrollWindow;
		
		private static bool CanCheatInObjects => ConfigFile.CheatMode && (Manager.saves.IsCreativeModeCharacter() || Manager.main.player.adminPrivileges >= 1);
		
		protected override void Awake() {
			base.Awake();
			
			_boxCollider = GetComponent<BoxCollider>();
			_scrollWindow = GetComponentInParent<UIScrollWindow>();
			
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
			
			if (IsHovered && input.WasButtonPressedDownThisFrame(PlayerInput.InputType.LOCKING_TOGGLE)) {
				if (IsFavorited) {
					AudioManager.Sfx(SfxTableID.inventorySFXSlotLock, transform.position);
					ConfigFile.FavoritedObjects.Remove(FavoritedKey);
				} else {
					AudioManager.Sfx(SfxTableID.inventorySFXSlotUnlock, transform.position);
					ConfigFile.FavoritedObjects.Add(FavoritedKey);
				}

				OnFavoritedStateChanged();
				ConfigFile.Save();
			}
		}

		protected virtual void OnFavoritedStateChanged() { }

		public override void OnSelected() {
			_scrollWindow?.MoveScrollToIncludePosition(localScrollPosition, _boxCollider != null ? _boxCollider.size.sqrMagnitude : 0f);
			OnSelectSlot();
		}

		public override void OnDeselected(bool playEffect = true) {
			OnDeselectSlot();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			var input = Manager.input.singleplayerInputModule;
			var containedObjectData = _displayedObject.ContainedObject.objectData;
			
			if (input.IsButtonCurrentlyDown(PlayerInput.InputType.PICK_UP_HALF) && CanCheatInObjects) {
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
			return _displayedObject.GetHoverDescription(this) ?? new List<TextAndFormatFields>();
		}
		
		public override List<TextAndFormatFields> GetHoverStats(bool previewReinforced) {
			return _displayedObject.GetHoverStats(this, previewReinforced);
		}

		public override HoverTitleIconType GetHoverTitleIconType() {
			return HoverTitleIconType.None;
		}

		public override HoverWindowAlignment GetHoverWindowAlignment() {
			return HoverWindowAlignment.BOTTOM_RIGHT_OF_CURSOR;
		}

		protected override ContainedObjectsBuffer GetSlotObject() {
			return _displayedObject.ContainedObject;
		}

		public void UpdateVisuals() {
			background.sprite = rarityBorders[0];
			if (highlightBorder != null)
				highlightBorder.gameObject.SetActive(false);

			var visualObject = DisplayedObject.VisualObject;
			RenderAmountNumberRange(DisplayedObject.Amount);
			
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
			if (Mathf.Approximately(spriteSize.x, 2.5f) && Mathf.Approximately(spriteSize.y, 2.5f)) {
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

			var slotObject = GetSlotObject();
			if (amount.Max > 1 && !AmountIsShownAsBar()) {
				var text = amount.Min != amount.Max ? $"{amount.Min}-{amount.Max}" : amount.Min.ToString();
				
				amountNumber.Render(text);
				if (amountNumberShadow != null)
					amountNumberShadow.Render(text);

				return true;
			}
			
			return false;
		}
	}
}