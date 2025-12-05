using System;
using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class ChangeCategoryAndTypeButton : BasicButton {
		[SerializeField]
		private ObjectEntriesWindow objectEntriesWindow;
		[SerializeField]
		private SpriteRenderer[] icons;
		[SerializeField]
		private ButtonStyle style;
		[SerializeField]
		private bool cyclesForwards;

		private int _categoryIndex;
		private int _entriesInCategory;
		private ObjectEntryType _type;
		private int _entriesInType;
		private ObjectEntryCategory _category;

		private enum ButtonStyle {
			TopButton,
			CycleCategory,
			CycleType
		}

		public void SetCategoryAndType(int categoryIndex, int entriesInCategory, ObjectEntryType type, int entriesInType, ObjectEntryCategory category) {
			_categoryIndex = categoryIndex;
			_entriesInCategory = entriesInCategory;
			_type = type;
			_entriesInType = entriesInType;
			_category = category;

			var objectInfo = PugDatabase.GetObjectInfo(category.Icon);
			foreach (var icon in icons)
				icon.sprite = objectInfo.smallIcon ?? objectInfo.icon;
			
			LateUpdate();
		}

		protected override void LateUpdate() {
			IsToggled = objectEntriesWindow.SelectedCategory == _categoryIndex && objectEntriesWindow.SelectedType == _type;
			
			base.LateUpdate();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (IsToggled || !canBeClicked)
				return;
			
			UserInterfaceUtils.PlayMenuOpenSound();
			objectEntriesWindow.SetTypeAndCategory(_type, _categoryIndex);
		}

		public override TextAndFormatFields GetHoverTitle() {
			if (!canBeClicked)
				return null;

			return new TextAndFormatFields {
				text = style switch {
					ButtonStyle.TopButton => _category.GetTitle(objectEntriesWindow.IsSelectedObjectNonObtainable),
					ButtonStyle.CycleCategory => optionalTitle.mTerm,
					ButtonStyle.CycleType => objectEntriesWindow.IsSelectedObjectNonObtainable ? $"ItemBrowser:ShowType_NonObtainable/{_type}" : $"ItemBrowser:ShowType/{_type}",
					_ => throw new ArgumentOutOfRangeException()
				}
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!canBeClicked)
				return null;
			
			var lines = new List<TextAndFormatFields>();

			if (style == ButtonStyle.TopButton) {
				lines.Add(new() {
					text = _entriesInCategory == 1 ? $"ItemBrowser:EntriesAmount/{_type}" : $"ItemBrowser:EntriesAmountPlural/{_type}",
					formatFields = new[] {
						_entriesInCategory.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else if (style == ButtonStyle.CycleCategory) {
				lines.Add(new TextAndFormatFields {
					text = _category.GetTitle(objectEntriesWindow.IsSelectedObjectNonObtainable),
					color = UserInterfaceUtils.DescriptionColor
				});
				if (!UserInterfaceUtils.IsUsingMouseAndKeyboard)
					UserInterfaceUtils.AppendButtonHint(lines, "ShortCutPC", cyclesForwards ? "ZoomInMap" : "ZoomOutMap");
			} else if (style == ButtonStyle.CycleType) {
				lines.Add(new() {
					text = _entriesInType == 1 ? $"ItemBrowser:EntriesAmount/{_type}" : $"ItemBrowser:EntriesAmountPlural/{_type}",
					formatFields = new[] {
						_entriesInType.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
				if (!UserInterfaceUtils.IsUsingMouseAndKeyboard)
					UserInterfaceUtils.AppendButtonHint(lines, "ShortCutPC", cyclesForwards ? "MapNextMarker" : "MapPreviousMarker");
			}

			return lines;
		}
	}
}