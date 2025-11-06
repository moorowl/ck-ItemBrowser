using System.Linq;
using PugMod;

namespace ItemBrowser.DataStructures {
	public static class ReflectionExtensions {
		public static T GetValue<T>(this object obj, string memberName, params object[] parameters) {
			var member = typeof(T).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == memberName);
			if (member == null)
				return default;
			
			return (T) API.Reflection.Invoke(member, obj, parameters);
		}
		
		public static void SetValue<T>(this object obj, string memberName, T value) {
			var member = typeof(T).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == memberName);
			if (member == null)
				return;
			
			API.Reflection.SetValue(member, obj, value);
		}
	}
}