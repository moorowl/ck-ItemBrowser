namespace ItemBrowser.Api.Entries {
	public abstract record ObjectEntry {
		public abstract ObjectEntryCategory Category { get; }
	}
}