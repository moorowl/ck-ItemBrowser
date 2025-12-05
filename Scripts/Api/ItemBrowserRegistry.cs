using System;
using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities.DataStructures;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using UnityEngine;

namespace ItemBrowser.Api {
	public class ItemBrowserRegistry {
		internal readonly HashSet<ObjectDataCD> Items = new();
		internal readonly HashSet<ObjectDataCD> Creatures = new();
		
		internal readonly List<(string Group, Filter<ObjectDataCD> Filter)> ItemFilters = new();
		internal readonly List<(string Group, Filter<ObjectDataCD> Filter)> CreatureFilters = new();

		internal readonly List<Sorter<ObjectDataCD>> ItemSorters = new();
		internal readonly List<Sorter<ObjectDataCD>> CreatureSorters = new();

		internal readonly List<ObjectEntryProvider> EntryProviders = new();
		internal readonly Dictionary<Type, GameObject> EntryDisplays = new();
		
		internal readonly Dictionary<ObjectDataCD, string> ObjectNameOverrides = new();
		internal readonly Dictionary<ObjectDataCD, Sprite> ObjectIconOverrides = new();
		internal readonly Dictionary<ObjectDataCD, string> ObjectNameNotes = new();

		public void AddItem(ObjectDataCD item) {
			Items.Add(item);
		}

		public void AddCreature(ObjectDataCD item) {
			Creatures.Add(item);
		}

		public void RemoveItem(ObjectDataCD item) {
			Items.Remove(item);
		}
		
		public void RemoveCreature(ObjectDataCD item) {
			Creatures.Remove(item);
		}
		
		public void AddEntryProvider(ObjectEntryProvider provider) {
			EntryProviders.Add(provider);
		}

		public void AddEntryDisplay(ObjectEntryDisplayBase component) {
			if (component == null)
				return;

			var gameObject = component.gameObject;
			foreach (var sr in gameObject.GetComponentsInChildren<SpriteRenderer>())
				sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
			foreach (var pugText in gameObject.GetComponentsInChildren<PugText>())
				pugText.style.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

			EntryDisplays.TryAdd(component.AssociatedEntry, gameObject);
		}

		public void AddObjectNameAndIconOverride(ObjectNameAndIconOverride overrides) {
			var objectData = overrides.AppliesToObjectData;
				
			if (overrides.overrideName && !string.IsNullOrWhiteSpace(overrides.name))
				ObjectNameOverrides.TryAdd(objectData, overrides.name);
				
			if (overrides.overrideIcon)
				ObjectIconOverrides.TryAdd(objectData, overrides.icon);
				
			if (overrides.showNameNote)
				ObjectNameNotes.TryAdd(objectData, overrides.nameNote);
		}

		public void AddItemFilter(string group, Filter<ObjectDataCD> filter) {
			ItemFilters.Add((group, filter));
		}

		public void AddCreatureFilter(string group, Filter<ObjectDataCD> filter) {
			CreatureFilters.Add((group, filter));
		}
		
		public void AddItemSorter(Sorter<ObjectDataCD> sorter) {
			ItemSorters.Add(sorter);
		}

		public void AddCreatureSorter(Sorter<ObjectDataCD> sorter) {
			CreatureSorters.Add(sorter);
		}
	}
}