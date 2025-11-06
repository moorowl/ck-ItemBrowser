using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Entries {
	public abstract class ObjectEntryProvider {
		public abstract void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects);
	}
}