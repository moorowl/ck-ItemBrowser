using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Browser {
	public class SortOrderButton : BasicButton {
		[SerializeField]
		private ObjectListWindow objectListWindow;
		[SerializeField]
		private Sprite ascendingSprite;
		[SerializeField]
		private Sprite descendingSprite;
		[SerializeField]
		private List<SpriteRenderer> spritesToUpdate;

		private bool _previousState;

		protected override void LateUpdate() {
			base.LateUpdate();

			if (objectListWindow.UseReverseSorting == _previousState)
				return;
			
			var newSprite = objectListWindow.UseReverseSorting ? descendingSprite : ascendingSprite;
			foreach (var sr in spritesToUpdate)
				sr.sprite = newSprite;
				
			_previousState = objectListWindow.UseReverseSorting;
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			return new() {
				new() {
					text = objectListWindow.UseReverseSorting ? "ItemBrowser:SortingOrder/Descending" : "ItemBrowser:SortingOrder/Ascending",
					color = TextUtils.DescriptionColor
				}
			};
		}
	}
}