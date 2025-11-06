using Pug.UnityExtensions;

namespace ItemBrowser.Utilities {
	public static class StructureUtils {
		public static string GetPersistentSceneName(string sceneName) {
			// This is to turn SceneBuilder's runtime names (e.g. SB/318923147) into its identifier
			return new SceneReference {
				ScenePath = sceneName
			}.SceneName;
		}

		public static bool CanSceneSpawn(string sceneName) {
			return true;
		}
	}
}