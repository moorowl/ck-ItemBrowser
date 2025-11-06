using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Browser.ObjectList {
	public class VirtualObjectList : ItemSlotsUIContainer, IScrollable {
		public Vector2Int size = Vector2Int.one;
		
		private float _currentScroll;
		private int _prevStartIndex;
		private int _prevSelectedIndex;

		public override bool isShowing => gameObject.activeInHierarchy;
		public override int MAX_ROWS => size.y;
		public override int MAX_COLUMNS => size.x;

		public override UIScrollWindow uiScrollWindow => GetComponent<UIScrollWindow>();

		private List<ObjectDataCD> _objects = new();
		private readonly Dictionary<int, int> _slotToObjectIndex = new();

		public void SetObjects(List<ObjectDataCD> objects) {
			_objects = objects;
			_prevSelectedIndex = 0;
			_prevStartIndex = 0;
			uiScrollWindow.ResetScroll();
			UpdateList();
		}
		
		protected override void LateUpdate() {
			base.LateUpdate();

			if ((Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement) && _objects.Count > 0 && !Manager.input.singleplayerInputModule.PrefersKeyboardAndMouse()) {
				foreach (var slot in itemSlots) {
					if (slot.visibleSlotIndex == 0) {
						slot.Select();
						break;
					}
				}
			}
		}

		public override void ShowContainerUI() {
			base.ShowContainerUI();
			gameObject.SetActive(true);
			UpdateList();
		}

		public override void HideContainerUI() {
			base.HideContainerUI();
			if (gameObject.activeSelf)
				gameObject.SetActive(false);
		}

		public void UpdateContainingElements(float scroll) {
			_currentScroll = scroll;
			UpdateList();
		}

		public bool IsBottomElementSelected() {
			if (Manager.ui.currentSelectedUIElement == null)
				return false;

			var indexOfElement = GetIndexOfElement(Manager.ui.currentSelectedUIElement);
			if (indexOfElement == -1)
				return false;

			return indexOfElement >= _objects.Count - _objects.Count % MAX_COLUMNS;
		}

		public bool IsTopElementSelected() {
			if (Manager.ui.currentSelectedUIElement == null)
				return false;

			var indexOfElement = GetIndexOfElement(Manager.ui.currentSelectedUIElement);
			if (indexOfElement == -1)
				return false;

			return indexOfElement < MAX_COLUMNS;
		}

		private int GetIndexOfElement(UIelement element) {
			for (var i = 0; i < itemSlots.Count && itemSlots[i].gameObject.activeSelf; i++) {
				if (itemSlots[i] == element)
					return _slotToObjectIndex.GetValueOrDefault(i);
			}

			return -1;
		}

		public float GetCurrentWindowHeight() {
			if (itemSlots.Count > MAX_COLUMNS)
				return math.abs(itemSlots[0].transform.localPosition.y - itemSlots[MAX_COLUMNS].transform.localPosition.y) * ((_objects.Count - 1) / MAX_COLUMNS + 1);

			return 0f;
		}

		public void UpdateList() {
			var num = math.max(0, ((int) math.floor(_currentScroll / spread) - 1) * MAX_COLUMNS);
			var num2 = math.max(0, ((int) math.floor(_currentScroll / spread) + MAX_ROWS) * MAX_COLUMNS);
			var num3 = spread * (num / MAX_COLUMNS);
			var sideStartPosition = GetSideStartPosition(MAX_COLUMNS);
			var num4 = 0f;
			var prevSelectedIndex = -1;
			if (Manager.ui.currentSelectedUIElement is VirtualObjectListItem craftingSelectorSlot)
				prevSelectedIndex = craftingSelectorSlot.visibleSlotIndex;

			_slotToObjectIndex.Clear();

			for (var i = 0; i < itemSlots.Count; i++) {
				var num5 = num + i;
				if (num5 >= num2 || num5 >= _objects.Count) {
					itemSlots[i].gameObject.SetActive(value: false);
					continue;
				}

				var slot = itemSlots[i] as VirtualObjectListItem;
				slot.visibleSlotIndex = i;
				slot.SetObjectData(_objects[num5], this);
				var num6 = i % MAX_COLUMNS;
				var num7 = i / MAX_COLUMNS;
				slot.transform.localPosition = new Vector3(sideStartPosition + num6 * spread, num4 - num7 * spread - num3, 0f);
				slot.gameObject.SetActive(value: true);

				_slotToObjectIndex[i] = num5;
			}

			if (_prevStartIndex != num) {
				_prevStartIndex = num;
				if (Manager.ui.currentSelectedUIElement is VirtualObjectListItem && (!Manager.input.SystemPrefersKeyboardAndMouse() || !Manager.input.SystemIsUsingMouse())) {
					for (var j = 0; j < itemSlots.Count; j++) {
						if (itemSlots[j].visibleSlotIndex == _prevSelectedIndex) {
							Manager.ui.DeselectAnySelectedUIElement();
							itemSlots[j].Select();
							Manager.ui.mouse.PlaceMousePositionOnSelectedUIElementWhenControlledByJoystick();
						}
					}
				}
			}

			_prevSelectedIndex = prevSelectedIndex;
		}

		private float GetSideStartPosition(int size) {
			return (0f - (size - 1) / 2f) * spread;
		}
	}
}