using System;
using System.Collections.Generic;

namespace ItemBrowser.Api.Entries {
	public abstract class ObjectEntryDisplayBase : UIelement {
		public abstract Type AssociatedEntry { get; }
		
		public abstract void SetEntry(ObjectEntry entry, ObjectData objectData);

		public abstract IEnumerable<ObjectEntry> SortEntries(IEnumerable<ObjectEntry> entries);
		
		public abstract void Render();

		public abstract float CalculateHeight();
	}
}