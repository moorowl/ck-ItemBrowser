using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Config {
	public class ConfigFile {
		private const string FilePath = Main.InternalName + "/Config.json";
		private const int CurrentVersion = 0;
		
		public bool CheatMode { get; set; }
		public bool ShowTechnicalInfo { get; set; }
		public bool DefaultDiscoveredFilter { get; set; }
		public HashSet<ObjectDataCD> FavoritedObjects { get; set; }

		public void ResetToDefault() {
			CheatMode = true;
			ShowTechnicalInfo = false;
			DefaultDiscoveredFilter = false;
			FavoritedObjects = new HashSet<ObjectDataCD>();

			Save();
		}
		
		public void Save() {
			Main.Log(nameof(ConfigFile), "Saving file...");
			
			if (!API.ConfigFilesystem.DirectoryExists(Main.InternalName))
				API.ConfigFilesystem.CreateDirectory(Main.InternalName);
			
			try {
				var serializedData = JsonUtility.ToJson(SerializeData());
				API.ConfigFilesystem.Write(FilePath, Encoding.UTF8.GetBytes(serializedData));
			} catch (Exception ex) {
				Main.Log(nameof(ConfigFile), "Error while saving file");
				Main.Log(ex);
			}
		}
		
		public void Load() {
			Main.Log(nameof(ConfigFile), "Loading file...");
			
			if (!API.ConfigFilesystem.FileExists(FilePath)) {
				ResetToDefault();
				return;
			}
			
			try {
				var serializedData = JsonUtility.FromJson<SerializedData>(Encoding.UTF8.GetString(API.ConfigFilesystem.Read(FilePath)));
				DeserializeData(serializedData);
			} catch (Exception ex) {
				Main.Log(nameof(ConfigFile), "Error while loading file, using defaults");
				Main.Log(ex);
				ResetToDefault();
			}
		}

		private SerializedData SerializeData() {
			return new SerializedData {
				Version = CurrentVersion,
				CheatMode = CheatMode,
				ShowTechnicalInfo = ShowTechnicalInfo,
				DefaultDiscoveredFilter = DefaultDiscoveredFilter,
				FavoritedObjects = FavoritedObjects.Select(x => new SerializedData.FavoritedObject {
					InternalName = API.Authoring.ObjectProperties.GetPropertyString(x.objectID, "name"),
					Variation = x.variation,
				}).ToList()
			};
		}

		private void DeserializeData(SerializedData data) {
			CheatMode = data.CheatMode;
			ShowTechnicalInfo = data.ShowTechnicalInfo;
			DefaultDiscoveredFilter = data.DefaultDiscoveredFilter;
			FavoritedObjects = data.FavoritedObjects.Select(x => new ObjectDataCD {
				objectID = API.Authoring.GetObjectID(x.InternalName),
				variation = x.Variation
			}).ToHashSet();
		}

		private class SerializedData {
			public class FavoritedObject {
				public string InternalName;
				public int Variation;
			}

			public int Version;
			public bool CheatMode;
			public bool ShowTechnicalInfo;
			public bool DefaultDiscoveredFilter;
			public List<FavoritedObject> FavoritedObjects;
		}
	}
}