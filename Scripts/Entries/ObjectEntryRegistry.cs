using System;
using System.Collections.Generic;
using System.Linq;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Entries {
	public class ObjectEntryRegistry {
		private readonly Dictionary<ObjectDataCD, EntryLookup> _entries = new();

		public IEnumerable<ObjectEntry> GetEntries(ObjectID id, int variation) {
			var objectData = new ObjectDataCD {
				objectID = id,
				variation = variation,
			};
			return !_entries.TryGetValue(objectData, out var entries) ? Array.Empty<ObjectEntry>() : entries.GetEntries();
		}
		
		public IEnumerable<T> GetEntriesOfType<T>(ObjectID id, int variation) where T : ObjectEntry {
			var objectData = new ObjectDataCD {
				objectID = id,
				variation = variation,
			};
			return !_entries.TryGetValue(objectData, out var entries) ? Array.Empty<T>() : entries.GetEntriesOfType<T>();
		}
		
		public void Register(ObjectID id, int variation, ObjectEntry entry) {
			if (id == ObjectID.GiantMushroom)
				id = ObjectID.GiantMushroom2;
			
			var objectData = new ObjectDataCD {
				objectID = id,
				variation = variation,
			};
			if (!_entries.ContainsKey(objectData))
				_entries[objectData] = new EntryLookup();
			
			_entries[objectData].Add(entry);
		}

		internal void RegisterFromProviders(List<ObjectEntryProvider> providers) {
			_entries.Clear();
			
			var allObjects = DatabaseConversionUtility.GetPrefabList(Manager.ecs.pugDatabase).Select(prefabData => {
				var objectData = new ObjectData {
					objectID = prefabData.ObjectInfo.objectID,
					variation = prefabData.ObjectInfo.variation
				};

				return (objectData, prefabData.ObjectInfo.prefabInfos[0].ecsPrefab);
			}).ToList();

			foreach (var provider in providers) {
				try {
					provider.Register(this, allObjects);
				} catch (Exception ex) {
					Main.Log("ObjectEntryRegistry", $"Error while registering entries from provider {provider.GetType().GetNameChecked()}");
					Debug.LogException(ex);
				}
			}
		}
		
		private class EntryLookup {
			private readonly List<ObjectEntry> _entries = new();
			private readonly Dictionary<Type, List<ObjectEntry>> _entriesByType = new();

			public void Add(ObjectEntry entry) {
				_entries.Add(entry);

				var type = entry.GetType();
				if (!_entriesByType.ContainsKey(type))
					_entriesByType[type] = new List<ObjectEntry>();
				
				_entriesByType[type].Add(entry);
			}

			public IEnumerable<ObjectEntry> GetEntries() {
				return _entries;
			}

			public IEnumerable<T> GetEntriesOfType<T>() where T : ObjectEntry {
				return !_entriesByType.ContainsKey(typeof(T)) ? Array.Empty<T>() : _entriesByType[typeof(T)].Cast<T>();
			}
		}
	}
}