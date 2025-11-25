using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;

namespace ItemBrowser.Browser {
	public class CreatureListWindow : ObjectListWindow {
		protected override List<Sorter<ObjectDataCD>> GetSorters() {
			return ItemBrowserAPI.CreatureSorters;
		}

		protected override List<(string Group, Filter<ObjectDataCD> Filter)> GetFilters() {
			return ItemBrowserAPI.CreatureFilters;
		}

		protected override List<ObjectDataCD> GetIncludedObjects() {
			return ObjectUtils.GetAllObjects().Where(ItemBrowserAPI.ShouldCreatureBeIncluded).ToList();
		}
	}
}