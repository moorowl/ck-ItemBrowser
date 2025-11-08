using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemBrowser.Browser.ObjectList;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Browser {
	public class ObjectListWindow : ItemBrowserWindow {
		private enum Mode {
			Items,
			Creatures,
			Cooking
		}
		
		[SerializeField]
		private VirtualObjectList objectList;
		[SerializeField]
		private TextInputField searchInput;
		[SerializeField]
		private SpriteMask searchInputMask;
		[SerializeField]
		private BasicButton itemsButton;
		[SerializeField]
		private BasicButton creaturesButton;
		[SerializeField]
		private BasicButton cookingButton;
		[SerializeField]
		private FiltersPanel filtersPanel;

		private Mode _currentMode;
		private bool _refreshItemList;
		private float _refreshedItemListTime;

		public int IncludedObjects { get; private set; }
		public int ExcludedObjects { get; private set; }
		
		private string SearchTerm => searchInput.pugText.GetText();
		private string _lastSearchTerm = string.Empty;
		
		private List<Sorter<ObjectDataCD>> _sorters;
		private int _currentSorterIndex;
		public bool UseReverseSorting { get; set; }
		public Sorter<ObjectDataCD> CurrentSorter => _sorters[_currentSorterIndex];
		
		private readonly SearchFilter<ObjectDataCD> _searchFilter = new((objectData, names) => {
			var displayName = ObjectUtils.GetLocalizedDisplayName(objectData.objectID, objectData.variation);
			names.Add(displayName ?? objectData.objectID.ToString());
			if (displayName == null)
				names.Add(objectData.objectID.ToString());
			names.Add(((int) objectData.objectID).ToString());
		});
		
		protected override void OnShow(bool isFirstTimeShowing) {
			objectList.ShowContainerUI();
			if (isFirstTimeShowing) {
				SetMode(Mode.Items);
				filtersPanel.IsShowing = false;
			}
			
			AdjustWindowPosition();
		}

		protected override void OnHide() {
			objectList.HideContainerUI();
		}

		private void LateUpdate() {
			var currentSearchTerm = SearchTerm;
			if (currentSearchTerm != _lastSearchTerm) {
				AdjustSearchFieldPosition();
				RequestItemListRefresh();
				_lastSearchTerm = currentSearchTerm;
			}
			
			itemsButton.IsToggled = _currentMode == Mode.Items;
			creaturesButton.IsToggled = _currentMode == Mode.Creatures;
			cookingButton.IsToggled = _currentMode == Mode.Cooking;

			if (filtersPanel.HasDynamicFiltersEnabled && Time.time >= _refreshedItemListTime + 1f)
				RequestItemListRefresh();
			
			if (_refreshItemList) {
				RefreshItemList();
				_refreshItemList = false;
			}
		}
		
		private void SetMode(Mode mode) {
			_currentMode = mode;

			// Setup sorters
			_currentSorterIndex = 0;
			_sorters = _currentMode switch {
				Mode.Items => ItemBrowserAPI.ItemSorters,
				Mode.Creatures => ItemBrowserAPI.CreatureSorters,
				Mode.Cooking => ItemBrowserAPI.CookingSorters,
				_ => throw new ArgumentOutOfRangeException()
			};
			UseReverseSorting = true;
			
			// Setup filters
			filtersPanel.Clear();
			var filters = _currentMode switch {
				Mode.Items => ItemBrowserAPI.ItemFilters,
				Mode.Creatures => ItemBrowserAPI.CreatureFilters,
				Mode.Cooking => ItemBrowserAPI.CookingFilters,
				_ => throw new ArgumentOutOfRangeException()
			};
			var filterGroups = filters.GroupBy(x => x.Group)
				.ToDictionary(group => group.Key, group => group.Select(x => x.Filter).ToList());

			foreach (var group in filterGroups) {
				filtersPanel.AddHeader(group.Key);
				foreach (var filter in group.Value)
					filtersPanel.AddFilter(filter);
			}
			
			RequestItemListRefresh();
		}
		
		public void ToggleFiltersPanel() {
			filtersPanel.IsShowing = !filtersPanel.IsShowing;
			Manager.ui.DeselectAnySelectedUIElement();
			Manager.ui.mouse.UpdateMouseUIInput(out _, out _);
			AdjustWindowPosition();
		}

		public void NextSort() {
			_currentSorterIndex++;
			if (_currentSorterIndex >= _sorters.Count)
				_currentSorterIndex = 0;
			
			RequestItemListRefresh();
		}
		
		public void PrevSort() {
			_currentSorterIndex--;
			if (_currentSorterIndex < 0)
				_currentSorterIndex = _sorters.Count - 1;
			
			RequestItemListRefresh();
		}

		public void CycleSortOrder() {
			UseReverseSorting = !UseReverseSorting;
			RequestItemListRefresh();
		}

		public void ClearSearch() {
			searchInput.ResetText();
		}
		
		public void SetItemsMode() {
			SetMode(Mode.Items);
		}
		
		public void SetCreaturesMode() {
        	SetMode(Mode.Creatures);
        }
		
        public void SetCookingMode() {
	        SetMode(Mode.Cooking);
        }

		private void AdjustWindowPosition() {
			transform.localPosition = new Vector3(filtersPanel.IsShowing ? -((filtersPanel.WindowWidth / 2f) + (1f / 16f)) : 0f, transform.localPosition.y, transform.localPosition.z);
		}
		
		private void AdjustSearchFieldPosition() {
			var maskUnitWidth = searchInputMask.transform.localScale.x / 16f;
			var searchInputPosition = searchInput.pugText.transform.localPosition;
			searchInputPosition.x = -1f * Mathf.Max(0f, searchInput.pugText.dimensions.width - maskUnitWidth);
			searchInput.pugText.transform.localPosition = searchInputPosition;

			var member = typeof(TextInputField).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "Update");
			API.Reflection.Invoke(member, searchInput);
		}

		public void RequestItemListRefresh() {
			_refreshItemList = true;
		}

		private void RefreshItemList() {
			_searchFilter.Term = SearchTerm;
			
			// Filtering
			var allItems = _currentMode switch {
				Mode.Items => PugDatabase.objectsByType.Keys.Where(ItemBrowserAPI.ShouldItemBeIncluded).ToList(),
				Mode.Creatures => PugDatabase.objectsByType.Keys.Where(ItemBrowserAPI.ShouldCreatureBeIncluded).ToList(),
				_ => new List<ObjectDataCD>()
			};
			var filteredItems = allItems
				.Where(MatchesFilters)
				.OrderBy(item => CurrentSorter.Function(item))
				.ThenBy(item => _sorters[0].Function(item))
				.ToList();
			
			if (UseReverseSorting)
				filteredItems.Reverse();
				
			objectList.SetObjects(filteredItems);
			_refreshedItemListTime = Time.time;
			
			IncludedObjects = filteredItems.Count;
			ExcludedObjects = allItems.Count - IncludedObjects;
		}

		private bool MatchesFilters(ObjectDataCD objectData) {
			return _searchFilter.Function(objectData)
			       && filtersPanel.FiltersToInclude.All(filter => filter.Function(objectData))
			       && !filtersPanel.FiltersToExclude.Any(filter => filter.Function(objectData));
		}
	}
}