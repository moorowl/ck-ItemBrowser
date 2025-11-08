using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Entries;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Browser {
	public class ObjectEntriesList : UIelement, IScrollable {
		[SerializeField]
		private GameObject dividerTemplate;
		[SerializeField]
		private float dividerPadding = 2f / 16f;
		[SerializeField]
		private UIScrollWindow scrollWindow;

		public float CurrentScrollProgress => 1f - ((scrollWindow.scrollingContent.localPosition.y - scrollWindow.minScrollPos) / Math.Max(scrollWindow.ScrollHeight - scrollWindow.minScrollPos, 0.001f));

		private PoolSystem _dividerPool;
		private readonly Dictionary<Type, PoolSystem> _displayPools = new();

		private ObjectDataCD _objectData;
		private List<ObjectEntry> _entries;
		private float _top;
		private PoolSystem _activeDisplayPool;
		private readonly List<ObjectEntryDisplayBase> _activeDisplays = new();
		private readonly List<GameObject> _activeDividers = new();

		private void Awake() {
			SetupPools();
		}
		
		public override void OnSelected() {
			base.OnSelected();

			if (_activeDisplays.Count > 0)
				_activeDisplays[0].TopMostUIElements.FirstOrDefault()?.Select();
		}

		public void SetEntries(ObjectDataCD objectData, List<ObjectEntry> entries, float scrollProgress = 1f) {
			_objectData = objectData;
			_entries = entries;

			SetupPools();
			ClearList();

			if (_entries.Count == 0)
				return;
			
			_activeDisplayPool = GetDisplayPool(entries[0].GetType());
			if (_activeDisplayPool == null)
				return;
			
			RenderList();
			
			scrollWindow.SetScrollValue(scrollProgress);
		}

		private void ClearList() {
			foreach (var display in _activeDisplays) {
				foreach (var text in display.GetComponentsInChildren<PugText>())
					text.Clear();
				
				_activeDisplayPool?.Free(display);
			}
			
			foreach (var divider in _activeDividers)
				_dividerPool?.Free(divider);

			_top = 0f;
			_activeDisplays.Clear();
			_activeDividers.Clear();
		}

		private void RenderList() {
			ClearList();

			foreach (var entry in _entries) {
				var display = _activeDisplayPool.GetFreeComponent<ObjectEntryDisplayBase>(true, true);
				display.SetEntry(entry, _objectData);
				display.Render();
				
				var height = RoundToPixelPerfectPosition.RoundFloat(display.CalculateHeight());
				var halfHeight = RoundToPixelPerfectPosition.RoundFloat(height / 2f);
				
				_top -= halfHeight;
				display.transform.localPosition = new Vector3(0f, _top, 0f);
				_activeDisplays.Add(display);
				_top -= halfHeight;
				
				_top -= dividerPadding;
				var divider = _dividerPool.GetFreeObject(true, true);
				divider.transform.localPosition = new Vector3(0f, _top, 0f);
				_activeDividers.Add(divider);
				_top -= dividerPadding;
			}

			for (var i = 0; i < _activeDisplays.Count; i++) {
				var display = _activeDisplays[i];

				if (i == 0) {
					display.topUIElements.Clear();
					display.topUIElements.AddRange(topUIElements);
				}

				if (i > 0) {
					var previousDisplay = _activeDisplays[i - 1];

					foreach (var element in previousDisplay.BottomMostUIElements) {
						element.bottomUIElements.Clear();
						element.bottomUIElements.AddRange(display.TopMostUIElements);
					}

					foreach (var element in display.TopMostUIElements) {
						element.topUIElements.Clear();
						element.topUIElements.AddRange(previousDisplay.BottomMostUIElements);
					}
				}
			}
		}
		
		private PoolSystem GetDisplayPool(Type entryType) {
			if (!ItemBrowserAPI.ObjectEntryDisplayPrefabs.TryGetValue(entryType, out var displayPrefab))
				return null;
			
			var displayPrefabComponent = displayPrefab.GetComponent<ObjectEntryDisplayBase>();
			if (displayPrefabComponent == null)
				return null;
			
			if (!_displayPools.ContainsKey(entryType))
				_displayPools[entryType] = new PoolSystem(displayPrefab, typeof(ObjectEntryDisplayBase), autoParent: scrollWindow.scrollingContent, autoEnable: true, initialSize: 16);

			return _displayPools.GetValueOrDefault(entryType);
		}

		private void SetupPools() {
			if (_dividerPool != null)
				return;
			
			_dividerPool = new PoolSystem(dividerTemplate, autoParent: scrollWindow.scrollingContent, autoEnable: true, initialSize: 16);
			foreach (var (entryType, _) in ItemBrowserAPI.ObjectEntryDisplayPrefabs)
				GetDisplayPool(entryType);
		}
		
		public void UpdateContainingElements(float scroll) { }

		public bool IsBottomElementSelected() {
			return false;
		}

		public bool IsTopElementSelected() {
			return false;
		}

		public float GetCurrentWindowHeight() {
			return Mathf.Abs(_top) - (1f / 16f);
		}
	}
}