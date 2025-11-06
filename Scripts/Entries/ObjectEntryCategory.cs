namespace ItemBrowser.Entries {
	public struct ObjectEntryCategory {
		public readonly ObjectEntryType Type;
		public readonly string Title;
		public readonly ObjectID Icon;
		public readonly int Priority;
		
		public ObjectEntryCategory(ObjectEntryType type, string title, ObjectID icon, int priority = 0) {
			Type = type;
			Title = title;
			Icon = icon;
			Priority = priority;
		}
	}
}