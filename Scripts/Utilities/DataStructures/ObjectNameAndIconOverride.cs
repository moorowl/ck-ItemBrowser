using NaughtyAttributes;
using Pug.UnityExtensions;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Utilities.DataStructures {
	public class ObjectNameAndIconOverride : MonoBehaviour {
		[SerializeField]
		[PickStringFromEnum(typeof(ObjectID))]
		private string appliesToObject;
		[SerializeField]
		private int appliesToObjectVariation; 
		
		public bool overrideName = true;
		[ShowIf("overrideName")]
		public bool setNameManually;
		[ShowIf("setNameManually")]
		public string name;
		
		public bool overrideIcon = true;
		[ShowIf("overrideIcon")]
		public Sprite icon;

		public ObjectDataCD ObjectData => new() {
			objectID = API.Authoring.GetObjectID(appliesToObject),
			variation = appliesToObjectVariation
		};

		private void OnValidate() {
			if (overrideName && !setNameManually)
				name = $"ItemBrowser:ItemNameOverrides/{appliesToObject}_{appliesToObjectVariation}";
		}
	}
}