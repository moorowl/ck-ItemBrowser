using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Browser {
	public class BasicButton : ButtonUIElement {
		public TextAndFormatFields Title { get; set; } = new();
		public List<TextAndFormatFields> Description { get; set; } = new();

		public bool IsToggled { get; set; }

		[SerializeField]
		private GameObject optionalToggledMarker;
		
		private BoxCollider _boxCollider;
		private UIScrollWindow _scrollWindow;
		
		public override float localScrollPosition => transform.localPosition.y - 0.625f;
		private bool ShowHoverWindow => _scrollWindow == null || _scrollWindow.IsShowingPosition(localScrollPosition);
		public override bool isVisibleOnScreen => ShowHoverWindow && base.isVisibleOnScreen;
		public override UIScrollWindow uiScrollWindow => _scrollWindow;

		protected override void Awake() {
			base.Awake();
			
			_boxCollider = GetComponent<BoxCollider>();
			_scrollWindow = GetComponentInParent<UIScrollWindow>();
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnRightClicked(mod1, mod2);
			
			if (onRightClick != null && onRightClick.GetPersistentEventCount() > 0 && canBeClicked && playClickSoundEffect)
				AudioManager.SfxUI(Manager.audio.InspectorFriendlySfxIDToSfxID(clickSoundEffect), clickSoundPitch, false, 1f, 0f);
		}

		protected override void LateUpdate() {
			base.LateUpdate();
			
			if (optionalSelectedMarker != null && optionalSelectedMarker.activeSelf && !canBeClicked)
				optionalSelectedMarker.SetActive(false);
			
			if (optionalToggledMarker != null)
				optionalToggledMarker.SetActive(canBeClicked && (IsToggled || leftClickIsHeldDown || (onRightClick != null && onRightClick.GetPersistentEventCount() > 0 && rightClickIsHeldDown)));
		}
		
		public override void OnSelected() {
			base.OnSelected();
			
			_scrollWindow?.MoveScrollToIncludePosition(localScrollPosition, _boxCollider != null ? Mathf.Max(_boxCollider.size.x, _boxCollider.size.y) / 2f : 0f);
		}

		public override TextAndFormatFields GetHoverTitle() {
			if (!canBeClicked)
				return null;
			
			if (showHoverTitle)
				return base.GetHoverTitle();
			
			return Title.text == null ? null : Title;
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!canBeClicked)
				return null;
			
			return showHoverDesc ? base.GetHoverDescription() : Description;
		}

		public override HoverWindowAlignment GetHoverWindowAlignment() {
			return HoverWindowAlignment.BOTTOM_RIGHT_OF_CURSOR;
		}
	}
}