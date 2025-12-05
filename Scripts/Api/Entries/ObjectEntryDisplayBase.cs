using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemBrowser.Api.Entries {
	public abstract class ObjectEntryDisplayBase : UIelement {
		[SerializeField]
		[HideInInspector]
		private UIelement[] topMostUIElements;
		[SerializeField]
		[HideInInspector]
		private UIelement[] bottomMostUIElements;
		
		public IEnumerable<UIelement> TopMostUIElements => topMostUIElements;
		public IEnumerable<UIelement> BottomMostUIElements => bottomMostUIElements;
		
		public abstract Type AssociatedEntry { get; }
		
		public abstract void SetEntry(ObjectEntry entry, ObjectData objectData);

		public abstract IEnumerable<ObjectEntry> SortEntries(IEnumerable<ObjectEntry> entries);
		
		public abstract void Render();

		public abstract float CalculateHeight();

		private void OnValidate() {
			topMostUIElements = GetComponentsInChildren<UIelement>().Where(element => element != this && element.topUIElements.Count == 0).ToArray();
			bottomMostUIElements = GetComponentsInChildren<UIelement>().Where(element => element != this && element.bottomUIElements.Count == 0).ToArray();
		}
	}
}