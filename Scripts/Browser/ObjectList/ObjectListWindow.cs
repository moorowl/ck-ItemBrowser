using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Browser.ObjectList;
using ItemBrowser.Config;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Browser {
	public abstract class ObjectListWindow : ItemBrowserWindow {
		[SerializeField]
		private VirtualObjectList objectList;
		[SerializeField]
		private TextInputField searchInput;
		[SerializeField]
		private TextInputField[] otherSearchInputsToSync;
		[SerializeField]
		private SpriteMask searchInputMask;
		[SerializeField]
		private FiltersPanel[] filtersPanels;
		public Transform tabButtonsAnchor;
		
		private bool _refreshItemList;
		private float _refreshedItemListTime;

		public int IncludedObjects { get; private set; }
		public int ExcludedObjects { get; private set; }
		
		private string SearchTerm => searchInput.GetInputText();
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

		private FiltersPanel PrimaryFiltersPanel => filtersPanels[0];
		
		protected override void OnShow(bool isFirstTimeShowing) {
			objectList.ShowContainerUI();
			if (isFirstTimeShowing) {
				SetupFiltersAndSorting();
				RefreshItemList();
				PrimaryFiltersPanel.IsShowing = false;
			}
			
			AdjustWindowPosition();
		}

		protected override void OnHide() {
			objectList.HideContainerUI();
		}

		private void LateUpdate() {
			var currentSearchTerm = SearchTerm;
			if (currentSearchTerm != _lastSearchTerm) {
				foreach (var otherSearchInput in otherSearchInputsToSync)
					otherSearchInput.SetInputText(currentSearchTerm);
				
				AdjustSearchFieldPosition();
				RequestItemListRefresh();
				_lastSearchTerm = currentSearchTerm;
			}
			
			if (PrimaryFiltersPanel.HasDynamicFiltersEnabled && Time.time >= _refreshedItemListTime + 1f)
				RequestItemListRefresh();
			
			if (_refreshItemList) {
				RefreshItemList();
				_refreshItemList = false;
			}
		}
		
		private void SetupFiltersAndSorting() {
			// Setup sorters
			_currentSorterIndex = 0;
			_sorters = GetSorters();
			UseReverseSorting = true;
			
			// Setup filters
			PrimaryFiltersPanel.Clear();
			var filterGroups = GetFilters().GroupBy(x => x.Group)
				.ToDictionary(group => group.Key, group => group.Select(x => x.Filter).ToList());

			foreach (var group in filterGroups) {
				PrimaryFiltersPanel.AddHeader(group.Key);
				foreach (var filter in group.Value)
					PrimaryFiltersPanel.AddFilter(filter);
			}
		}
		
		protected abstract List<Sorter<ObjectDataCD>> GetSorters();
		
		protected abstract List<(string Group, Filter<ObjectDataCD> Filter)> GetFilters();
		
		protected abstract List<ObjectDataCD> GetIncludedObjects();
		
		public void ToggleFiltersPanel() {
			var shouldShow = !PrimaryFiltersPanel.IsShowing;
			foreach (var panel in filtersPanels)
				panel.IsShowing = shouldShow;
			
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
			foreach (var otherSearchInput in otherSearchInputsToSync)
				otherSearchInput.ResetText();
		}
		
		private void AdjustWindowPosition() {
			transform.localPosition = new Vector3(PrimaryFiltersPanel.IsShowing ? -((PrimaryFiltersPanel.WindowWidth / 2f) + (1f / 16f)) : 0f, transform.localPosition.y, transform.localPosition.z);
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
			var allItems = GetIncludedObjects();
			var filteredItems = allItems
				.Where(MatchesFilters)
				.OrderBy(item => ConfigFile.FavoritedObjects.Contains(item) ? 1 : 0)
				.ThenBy(item => CurrentSorter.Function(item))
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
			       && PrimaryFiltersPanel.FiltersToInclude.All(filter => filter.Function(objectData))
			       && !PrimaryFiltersPanel.FiltersToExclude.Any(filter => filter.Function(objectData));
		}
	}
}