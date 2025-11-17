using System.Collections.Generic;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.Browser {
	public class FilterButton : BasicButton {
		public Filter<ObjectDataCD> Filter { get; set; }

		private FilterState _currentState;
		public FilterState CurrentState {
			get => _currentState;
			set {
				if (value == _currentState)
					return;
				
				_currentState = value;
				UpdateVisuals();
				//filtersPanel.OnFilterStateChanged(Filter);
				objectListWindow.RequestItemListRefresh();
			}
		}
		
		[SerializeField]
		private ObjectListWindow objectListWindow;
		[SerializeField]
		private FiltersPanel filtersPanel;
		[SerializeField]
		private SpriteRenderer toggledBackground;
		
		public void SetFilter(Filter<ObjectDataCD> filter) {
			Filter = filter;
			ResetState();
		}

		public void ResetState() {
			_currentState = Filter.DefaultState();
			UpdateVisuals();
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
			
			base.LateUpdate();
		}

		private void UpdateVisuals() {
			Title = new TextAndFormatFields {
				text = Filter.Name,
				formatFields = Filter.NameFormatFields,
				dontLocalizeFormatFields = !Filter.LocalizeNameFormatFields
			};
			Description = new List<TextAndFormatFields> {
				new() {
					text = Filter.Description,
					formatFields = Filter.DescriptionFormatFields,
					dontLocalizeFormatFields = !Filter.LocalizeDescriptionFormatFields,
					color = TextUtils.DescriptionColor
				},
				new() {
					text = $"ItemBrowser:FilterState/{CurrentState}",
					color = GetStateColor(CurrentState)
				}
			};
			
			toggledBackground.color = GetStateColor(CurrentState).ColorWithNewAlpha(toggledBackground.color.a);
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