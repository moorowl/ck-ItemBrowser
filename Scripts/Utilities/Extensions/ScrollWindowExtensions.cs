using System.Linq;
using PugMod;

namespace ItemBrowser.Utilities.Extensions {
	public static class ScrollWindowExtensions {
		private static readonly MemberInfo MiIsMouseWithinScrollArea = typeof(UIScrollWindow).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "IsMouseWithinScrollArea");
		
		public static bool IsMouseWithinScrollArea(this UIScrollWindow scrollWindow) {
			return (bool) API.Reflection.Invoke(MiIsMouseWithinScrollArea, scrollWindow);
		}
	}
}