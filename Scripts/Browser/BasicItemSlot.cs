using System.Collections.Generic;
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
		
		protected override void Awake() {
			base.Awake();
			
			icon.material = new Material(Shader.Find("Amplify/UISpriteColorReplace"));
			UpdateVisuals();
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			_displayedObject.Update(this);
		}

		public override void OnSelected() {
			OnSelectSlot();
		}

		public override void OnDeselected(bool playEffect = true) {
			OnDeselectSlot();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			var input = Manager.input.singleplayerInputModule;
			var containedObjectData = _displayedObject.ContainedObject.objectData;
			
			if (input.IsButtonCurrentlyDown(PlayerInput.InputType.PICK_UP_HALF) && Manager.saves.IsCreativeModeCharacter()) {
				var pickUpTen = mod1 && PugDatabase.GetObjectInfo(containedObjectData.objectID, containedObjectData.variation) is { isStackable: true };

				var player = Manager.main.player;
				player.playerCommandSystem.CreateAndDropEntity(containedObjectData.objectID, player.WorldPosition, pickUpTen ? 10 : 1, player.entity, containedObjectData.variation);
				UserInterfaceUtils.PlayItemTwitchSound(transform);
				
				return;
			}

			if (linksSource && _displayedObject.ShowEntries(this, ObjectEntryType.Source))
				UserInterfaceUtils.PlayMenuOpenSound();
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnRightClicked(mod1, mod2);
			
			if (linksUsage && _displayedObject.ShowEntries(this, ObjectEntryType.Usage))
				UserInterfaceUtils.PlayMenuOpenSound();
		}

		public override TextAndFormatFields GetHoverTitle() {
			var title = _displayedObject.GetHoverTitle(this);
			var visualObject = _displayedObject.VisualObject;
			var amount = _displayedObject.Amount;
			
			if (showAmountInTitle && visualObject.objectID != ObjectID.None && amount.Max > 1) {
				return new TextAndFormatFields {
					text = "ItemBrowser:NameAndAmountFormat",
					formatFields = new[] {
						PlayerController.GetObjectName(GetSlotObject(), true).text,
						amount.Min != amount.Max ? $"{amount.Min}-{amount.Max}" : amount.Min.ToString()
					},
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
			var objectInfo = PugDatabase.GetObjectInfo(slotObject.objectID, slotObject.variation);

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