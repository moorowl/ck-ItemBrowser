namespace ItemBrowser.UserInterface.Browser {
	public class DetailsPanel : UIelement, IScrollable {
		public void UpdateContainingElements(float scroll) { }

		public bool IsBottomElementSelected() {
			return false;
		}

		public bool IsTopElementSelected() {
			return false;
		}

		public float GetCurrentWindowHeight() {
			return 0f;
		}
	}
}