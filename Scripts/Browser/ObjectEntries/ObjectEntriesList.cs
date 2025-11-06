using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Entries;
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
		
		private readonly List<ObjectEntryDisplayBase> _displays = new();
		private readonly List<GameObject> _allElements = new();
		private float _top;

		public override void OnSelected() {
			base.OnSelected();

			if (_displays.Count > 0)
				_displays[0].TopMostUIElements.FirstOrDefault()?.Select();
		}

		public void ClearEntries() {
			foreach (var element in _allElements) {
				foreach (var text in element.GetComponentsInChildren<PugText>())
					text.Clear();
				
				Destroy(element.gameObject);
			}
			
			_allElements.Clear();
			_displays.Clear();
			_top = 0f;
			
			scrollWindow.ResetScroll();
		}
		
		public void SetEntries(ObjectDataCD objectData, List<ObjectEntry> entries, float scrollProgress = 1f) {
			ClearEntries();

			if (entries.Count == 0)
				return;
			
			if (!ItemBrowserAPI.ObjectEntryDisplayPrefabs.TryGetValue(entries[0].GetType(), out var prefab))
				return;
			
			var prefabDisplayComponent = prefab.GetComponent<ObjectEntryDisplayBase>();
			if (prefabDisplayComponent == null)
				return;
			
			foreach (var entry in prefabDisplayComponent.SortEntries(entries)) {
				// This should probably be changed to pooling in the future
				var displayGo = Instantiate(prefab, scrollWindow.scrollingContent);
				var displayComponent = displayGo.GetComponent<ObjectEntryDisplayBase>();
				displayComponent.SetEntry(entry, objectData);
				displayComponent.Render();
				
				var height = RoundToPixelPerfectPosition.RoundFloat(displayComponent.CalculateHeight());
				var halfHeight = RoundToPixelPerfectPosition.RoundFloat(height / 2f);
				
				_top -= halfHeight;
				displayGo.transform.localPosition = new Vector3(0f, _top, 0f);
				_top -= halfHeight;
				
				_displays.Add(displayComponent);
				_allElements.Add(displayGo);

				_top -= dividerPadding;
				var divider = Instantiate(dividerTemplate, scrollWindow.scrollingContent);
				divider.SetActive(true);
				divider.transform.localPosition = new Vector3(0f, _top, 0f);
				_top -= dividerPadding;
				
				_allElements.Add(divider);
			}

			for (var i = 0; i < _displays.Count; i++) {
				var display = _displays[i];

				if (i == 0)
					display.topUIElements.AddRange(topUIElements);

				if (i > 0) {
					var previousDisplay = _displays[i - 1];
					
					foreach (var element in previousDisplay.BottomMostUIElements)
						element.bottomUIElements.AddRange(display.TopMostUIElements);
					
					foreach (var element in display.TopMostUIElements)
						element.topUIElements.AddRange(previousDisplay.BottomMostUIElements);
				}
			}
			
			scrollWindow.SetScrollValue(scrollProgress);
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

		private static (List<UIelement> TopMost, List<UIelement> BottomMost) GetTopAndBottomMostElements(Transform root) {
			return (
				root.GetComponentsInChildren<UIelement>().Where(element => element.topUIElements.Count == 0).ToList(),
				root.GetComponentsInChildren<UIelement>().Where(element => element.bottomUIElements.Count == 0).ToList()
			);
		}
	}
}