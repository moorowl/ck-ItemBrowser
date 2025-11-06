using System.Collections.Generic;
using ItemBrowser.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Browser {
	public class ChangeCategoryButton : BasicButton {
		[SerializeField]
		private ObjectEntriesWindow objectEntriesWindow;
		[SerializeField]
		private SpriteRenderer[] icons;

		private ObjectEntryCategory _category;
		private int _count;
		private int _index;

		public void SetCategory(int index, int count, ObjectEntryCategory category) {
			_index = index;
			_count = count;
			_category = category;

			var objectInfo = PugDatabase.GetObjectInfo(category.Icon);
			foreach (var icon in icons)
				icon.sprite = objectInfo.smallIcon ?? objectInfo.icon;
			
			LateUpdate();
		}

		protected override void LateUpdate() {
			IsToggled = objectEntriesWindow.SelectedCategory == _index;
			
			base.LateUpdate();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (IsToggled || !canBeClicked)
				return;
			
			UserInterfaceUtils.PlayMenuOpenSound(0.0025f * _index);
			objectEntriesWindow.SetCategory(_index);
		}

		public override TextAndFormatFields GetHoverTitle() {
			if (!canBeClicked)
				return null;
			
			if (showHoverTitle)
				return base.GetHoverTitle();
			
			return new TextAndFormatFields {
				text = _category.Title
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!canBeClicked)
				return null;
			
			if (showHoverTitle) {
				return new List<TextAndFormatFields> {
					new() {
						text = _category.Title,
						color = TextUtils.DescriptionColor
					}
				};
			} else {
				return new List<TextAndFormatFields> {
					new() {
						text = _count == 1 ? "ItemBrowser:SourcesFor" : "ItemBrowser:SourcesForPlural",
						formatFields = new[] {
							_count.ToString()
						},
						dontLocalizeFormatFields = true,
						color = TextUtils.DescriptionColor
					}
				};	
			}
		}
	}
}