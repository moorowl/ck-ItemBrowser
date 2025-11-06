using PugMod;

namespace ItemBrowser.Utilities {
	public class WorldUtils {
		public static int ClientPlayerCount => Manager.main.allPlayers != null ? Manager.main.allPlayers.Count : 1;
		public static WorldInfoCD ClientWorldInfo => API.Client.World.GetExistingSystemManaged<WorldInfoSystem>().WorldInfo;
	}
}