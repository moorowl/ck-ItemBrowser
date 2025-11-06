using System.Collections.Generic;
using System.Linq;
using ModIO;
using PugMod;

namespace ItemBrowser.Utilities {
	public static class ModUtils {
		private static readonly Dictionary<long, string> DisplayNames = new();
		private static readonly Dictionary<long, HashSet<ObjectID>> AssociatedObjects = new();
		private static readonly Dictionary<ObjectID, long> AssociatedMod = new();

		private const long UnknownModId = -1;
		private const string UnknownModName = "(Unknown Mod)";
		private const long CoreKeeperModId = 0;
		private const string CoreKeeperModName = "Core Keeper";
		
		internal static void InitOnModLoad() {
			SetupDisplayNames();
			SetupAssociatedObjects();
		}
		
		public static string GetDisplayName(long mod) {
			return mod switch {
				CoreKeeperModId => CoreKeeperModName,
				UnknownModId => UnknownModName,
				_ => DisplayNames.GetValueOrDefault(mod, mod.ToString())
			};
		}
		
		public static HashSet<ObjectID> GetAssociatedObjects(long mod) {
			return AssociatedObjects.TryGetValue(mod, out var value) ? value : new HashSet<ObjectID>();
		}
		
		public static long GetAssociatedMod(ObjectID id) {
			return AssociatedMod.GetValueOrDefault(id, CoreKeeperModId);
		}
		
		private static void SetupDisplayNames() {
			DisplayNames.Clear();
			
			foreach (var mod in API.ModLoader.LoadedMods)
				DisplayNames[mod.ModId] = mod.Metadata.name;
			
			var subscribedMods = ModIOUnity.GetSubscribedMods(out var result);
			if (!result.Succeeded())
				return;

			foreach (var subscribedMod in subscribedMods) {
				var profile = subscribedMod.modProfile;
				DisplayNames[profile.id.id] = profile.name;
			}
		}

		private static void SetupAssociatedObjects() {
			AssociatedObjects.Clear();
			AssociatedMod.Clear();

			foreach (var authoring in Manager.mod.ExtraAuthoring) {
				var gameObject = authoring.gameObject;

				var associatedModId = UnknownModId;
				var objectId = ObjectID.None;
				
				if (gameObject.TryGetComponent<ObjectAuthoring>(out var objectAuthoring)) {
					var internalName = objectAuthoring.objectName;
					if (internalName.Contains(":")) {
						var sourceMod = ProcessModInternalName(internalName.Split(':')[0]);
						associatedModId = API.ModLoader.LoadedMods.FirstOrDefault(mod => ProcessModInternalName(mod.Metadata.name) == sourceMod)?.ModId ?? UnknownModId;
						objectId = API.Authoring.GetObjectID(internalName);
					}
				}

				if (objectId == ObjectID.None)
					continue;
				
				if (!AssociatedObjects.ContainsKey(associatedModId))
					AssociatedObjects[associatedModId] = new HashSet<ObjectID>();
				
				AssociatedObjects[associatedModId].Add(objectId);
				AssociatedMod.TryAdd(objectId, associatedModId);
			}

			return;

			string ProcessModInternalName(string name) {
				return name.ToLowerInvariant().Replace("-", "").Replace("_", "").Replace(" ", "");
			}
		}
	}
}