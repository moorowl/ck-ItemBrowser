using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Browser {
	public class ToggleFiltersButton : BasicButton {
		[SerializeField]
		private ObjectListWindow objectListWindow;
		[SerializeField]
		private FiltersPanel filtersPanel;

		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = filtersPanel.IsShowing ? "ItemBrowser:HideFilters" : "ItemBrowser:ShowFilters"
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = new List<TextAndFormatFields> {
				new() {
					text = "ItemBrowser:FilteredResults",
					formatFields = new[] {
						objectListWindow.IncludedObjects.ToString(),
						(objectListWindow.IncludedObjects + objectListWindow.ExcludedObjects).ToString()
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor,
					paddingBeneath = filtersPanel.HasBeenModified ? 0.125f : 0f
				}
			};
			if (filtersPanel.HasBeenModified) {
				lines.Add(new TextAndFormatFields {
					text = "ItemBrowser:ButtonHint/RestoreDefaults",
					formatFields = new[] {
						TextUtils.GetInputGlyph("UISecondInteract")
					},
					dontLocalizeFormatFields = true,
					color = Color.white * 0.95f
				});
			}
			
			return lines;
		}
	}
}