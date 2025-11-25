using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;

namespace ItemBrowser.Browser {
	public class ItemListWindow : ObjectListWindow {
		protected override List<Sorter<ObjectDataCD>> GetSorters() {
			return ItemBrowserAPI.ItemSorters;
		}

		protected override List<(string Group, Filter<ObjectDataCD> Filter)> GetFilters() {
			return ItemBrowserAPI.ItemFilters;
		}

		protected override List<ObjectDataCD> GetIncludedObjects() {
			return ObjectUtils.GetAllObjects().Where(ItemBrowserAPI.ShouldItemBeIncluded).ToList();
		}
	}
}