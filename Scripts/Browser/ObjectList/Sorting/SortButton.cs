using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Browser {
	public class SortButton : BasicButton {
		[SerializeField]
		private ObjectListWindow objectListWindow;
		
		public override List<TextAndFormatFields> GetHoverDescription() {
			return new List<TextAndFormatFields> {
				new() {
					text = objectListWindow.CurrentSorter.Name,
					color = UserInterfaceUtils.DescriptionColor
				}
			};
		}
	}
}