using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public class CookingIngredientDisplay : ObjectEntryDisplay<CookingIngredient> {
		[SerializeField]
		private BasicItemSlot ingredientSlot;
		[SerializeField]
		private BasicItemSlot turnsIntoFoodSlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			ingredientSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Ingredient
			});
			turnsIntoFoodSlot.DisplayedObject = new DisplayedObject.CookedFood(Entry.TurnsIntoFood, Entry.Ingredient, ObjectID.Egg);
		}
		
		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/CookingIngredient_0",
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}