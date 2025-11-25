using System.Collections.Generic;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class FilterButton : BasicButton {
		public Filter<ObjectDataCD> Filter { get; set; }

		private FilterState _currentState;
		public FilterState CurrentState {
			get => _currentState;
			set {
				if (value == _currentState)
					return;
				
				_currentState = value;
				objectListWindow.RequestListRefresh(false);
			}
		}
		
		[SerializeField]
		private ObjectListWindow objectListWindow;
		[SerializeField]
		private FiltersPanel filtersPanel;
		[SerializeField]
		private SpriteRenderer toggledBackground;
		[SerializeField]
		private BoxCollider boxCollider;
		
		public void SetFilter(Filter<ObjectDataCD> filter) {
			Filter = filter;
			ResetState();
		}

		public void ResetState() {
			CurrentState = Filter.DefaultState();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			CurrentState = CurrentState switch {
				FilterState.None => FilterState.Include,
				FilterState.Exclude => FilterState.Include,
				_ => FilterState.None
			};
		}
		
		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			CurrentState = CurrentState switch {
				FilterState.None => FilterState.Exclude,
				FilterState.Include => FilterState.Exclude,
				_ => FilterState.None
			};
		}

		protected override void LateUpdate() {
			IsToggled = CurrentState != FilterState.None;
			toggledBackground.color = GetStateColor(CurrentState).ColorWithNewAlpha(toggledBackground.color.a);
			
			base.LateUpdate();
		}

		public override UIelement GetAdjacentUIElement(Direction.Id direction, Vector3 currentPosition) {
			return filtersPanel.GetClosestUIElement(currentPosition + direction switch {
				Direction.Id.forward => new Vector3(0, boxCollider.size.y, 0f),
				Direction.Id.left => new Vector3(-boxCollider.size.x, 0f, 0f),
				Direction.Id.back => new Vector3(0, -boxCollider.size.y, 0f),
				Direction.Id.right => new Vector3(boxCollider.size.x, 0f, 0f),
				_ => Vector3.zero
			});
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = Filter.Name,
				formatFields = Filter.NameFormatFields,
				dontLocalizeFormatFields = !Filter.LocalizeNameFormatFields
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			return new List<TextAndFormatFields> {
				new() {
					text = Filter.Description,
					formatFields = Filter.DescriptionFormatFields,
					dontLocalizeFormatFields = !Filter.LocalizeDescriptionFormatFields,
					color = UserInterfaceUtils.DescriptionColor
				},
				new() {
					text = $"ItemBrowser:FilterState/{CurrentState}",
					color = GetStateColor(CurrentState)
				}
			};
		}

		private static Color GetStateColor(FilterState state) {
			return state switch {
				FilterState.Include => Color.green,
				FilterState.Exclude => Manager.ui.brokenColor,
				_ => Color.white
			};
		}
	}
}