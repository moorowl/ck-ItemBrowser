using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class FilterHeader : MonoBehaviour {
		[SerializeField]
		private PugText text;

		public void SetTerm(string term) {
			text.Render(term);
		}
	}
}