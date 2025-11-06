using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Entries;
using UnityEngine;
using UnityEngine.Serialization;

namespace ItemBrowser.Browser {
	public class ObjectEntriesWindow : ItemBrowserWindow {
		private const int MaxCategoryButtons = 8;
		
		[SerializeField]
		private SelectedItemSlot selectedItemSlot;
		[FormerlySerializedAs("itemDetailsList")] [SerializeField]
		private ObjectEntriesList objectEntriesList;
		[SerializeField]
		private PugText selectedTypeText;
		[SerializeField]
		private PugText selectedCategoryText;
		[SerializeField]
		private ChangeCategoryButton nextCategoryButton;
		[SerializeField]
		private ChangeCategoryButton prevCategoryButton;
		[SerializeField]
		private ChangeCategoryButton categoryButtonPrefab;
		[SerializeField]
		private Transform categoryButtonContainer;
		[SerializeField]
		private float categoryButtonGap;
		
		private ObjectDataCD _objectData;
		private List<List<ObjectEntry>> _entries;
		private readonly List<ChangeCategoryButton> _categoryButtons = new();
		private readonly Stack<(ObjectDataCD ObjectData, ObjectEntryType SelectedType, int SelectedCategory, float ScrollProgress)> _history = new();
		
		public ObjectEntryType SelectedType { get; private set; }
		public int SelectedCategory { get; private set; }

		public bool HasAnyHistory => _history.Count > 0;
		
		private void LateUpdate() {
			var inputModule = Manager.main.player.inputModule;

			/*if (_entries.Count > 1) {
				if (inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.NEXT_SLOT))
					CycleToNextCategory();
				
				if (inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.PREVIOUS_SLOT))
					CycleToPreviousCategory();
			}*/
		}

		private void TryInstantiateCategoryButtons() {
			if (_categoryButtons.Count > 0)
				return;
			
			for (var i = 0; i < MaxCategoryButtons; i++) {
				var button = Instantiate(categoryButtonPrefab, categoryButtonContainer);
				button.gameObject.SetActive(false);
				button.transform.localPosition = new Vector3(categoryButtonGap * i, 0f, 0f);
				
				_categoryButtons.Add(button);
			}
		}

		public bool PushObjectData(ObjectDataCD objectData, ObjectEntryType initialSelectedType) {
			if (objectData.Equals(_objectData))
				return false;
			
			var entries = ItemBrowserAPI.ObjectEntries.GetEntries(objectData.objectID, objectData.variation).Where(entry => entry.Category.Type == SelectedType);
			if (!entries.Any())
				return false;
			
			if (_objectData.objectID != ObjectID.None)
				_history.Push((_objectData, SelectedType, SelectedCategory, objectEntriesList.CurrentScrollProgress));
			
			_objectData = objectData;
			SetTypeAndCategory(initialSelectedType, 0);

			return true;
		}

		public bool PopObjectData() {
			if (!_history.TryPop(out var state))
				return false;
			
			_objectData = state.ObjectData;
			Main.Log("ObjectEntriesWindow", $"Restoring entry list state for {_objectData.objectID}:{_objectData.variation} ({state.SelectedType}/{state.SelectedCategory}, {state.ScrollProgress * 100f}%)");
			SetTypeAndCategory(state.SelectedType, state.SelectedCategory, state.ScrollProgress);

			return true;
		}

		public void Clear() {
			_history.Clear();
			_objectData = default;
		}
		
		public void SetTypeAndCategory(ObjectEntryType type, int category, float scrollProgress = 1f) {
			SelectedType = type;
			SelectedCategory = category;

			_entries = ItemBrowserAPI.ObjectEntries.GetEntries(_objectData.objectID, _objectData.variation)
				.Where(entry => entry.Category.Type == SelectedType)
				.GroupBy(details => details.Category.Title)
				.Select(group => group.ToList())
				.OrderByDescending(entries => entries.First().Category.Priority)
				.ToList();
			
			selectedItemSlot.SetObjectData(_objectData);
			selectedTypeText.Render($"ItemBrowser:ObjectEntryTypeHeader_Item/{SelectedType}");
			
			selectedCategoryText.gameObject.SetActive(false);
			nextCategoryButton.canBeClicked = false;
			prevCategoryButton.canBeClicked = false;

			TryInstantiateCategoryButtons();
			foreach (var button in _categoryButtons)
				button.gameObject.SetActive(false);
			for (var i = 0; i < Math.Min(_entries.Count, MaxCategoryButtons); i++) {
				var button = _categoryButtons[i];
				button.SetCategory(i, _entries[i].Count, _entries[i].First().Category);
				button.gameObject.SetActive(true);
			}
			
			if (_entries.Count == 0) {
				objectEntriesList.ClearEntries();
				return;
			}

			var details = _entries[category];
			if (details.Count == 0) {
				objectEntriesList.ClearEntries();
				return;
			}
			
			selectedCategoryText.gameObject.SetActive(true);
			selectedCategoryText.Render(details[0].Category.Title);

			if (_entries.Count >= 2) {
				var nextCategoryIndex = SelectedCategory + 1;
				if (nextCategoryIndex >= _entries.Count)
					nextCategoryIndex = 0;
				var nextCategory = _entries[nextCategoryIndex].First().Category;
				
				var prevCategoryIndex = SelectedCategory - 1;
				if (prevCategoryIndex < 0)
					prevCategoryIndex = _entries.Count - 1;
				var prevCategory = _entries[prevCategoryIndex].First().Category;
				
				nextCategoryButton.canBeClicked = true;
				nextCategoryButton.SetCategory(nextCategoryIndex, 0, nextCategory);
				prevCategoryButton.canBeClicked = true;
				prevCategoryButton.SetCategory(prevCategoryIndex, 0, prevCategory);
			}
			
			objectEntriesList.SetEntries(_objectData, details, scrollProgress);
		}

		public void SetCategory(int category) {
			SetTypeAndCategory(SelectedType, Math.Clamp(category, 0, _entries.Count - 1));
		}

		private void CycleToNextCategory() {
			var nextCategory = SelectedCategory + 1;
			if (nextCategory >= _entries.Count)
				nextCategory = 0;
			
			SetCategory(nextCategory);
		}
		
		private void CycleToPreviousCategory() {
			var nextCategory = SelectedCategory - 1;
			if (nextCategory < 0)
				nextCategory = _entries.Count - 1;
			
			SetCategory(nextCategory);
		}
	}
}