using FluffyUnderware.DevTools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils
{
    public class NewRecipeDetails
    {
        public CraftingMethod craftingMethod;
        public int craftTierRequired;
        public int duration;
        public List<RecipeResourceInfo> ingredients = new List<RecipeResourceInfo>();
        public List<RecipeResourceInfo> outputs = new List<RecipeResourceInfo>();
        public int sortPriority;
        public string unlockName;

        public SchematicsRecipeData ConvertToRecipe() {
            SchematicsRecipeData recipe = (SchematicsRecipeData)ScriptableObject.CreateInstance("SchematicsRecipeData");
            recipe.craftingMethod = craftingMethod;
            recipe.craftTierRequired = craftTierRequired;
            recipe.duration = duration;
            recipe.sortPriority = sortPriority;
            recipe.ingQuantities = new int[ingredients.Count];
            recipe.ingTypes = new ResourceInfo[ingredients.Count];
            recipe.outputQuantities = new int[outputs.Count];
            recipe.outputTypes = new ResourceInfo[outputs.Count];

            for (int i = 0; i < ingredients.Count; i++) {
                recipe.ingQuantities[i] = ingredients[i].quantity;
                ResourceInfo ingredient = ModUtils.GetResourceInfoByNameUnsafe(ingredients[i].name);
                ModUtils.NullCheck(ingredient, $"New recipe ingredient {ingredients[i].name}");
                recipe.ingTypes[i] = ingredient;
            }

            for (int i = 0; i < outputs.Count; i++) {
                recipe.outputQuantities[i] = outputs[i].quantity;
                ResourceInfo output = ModUtils.GetResourceInfoByNameUnsafe(outputs[i].name);
                ModUtils.NullCheck(output, $"New recipe output {outputs[i].name}");
                recipe.outputTypes[i] = output;
            }

            return recipe;
        }

        public override string ToString()
        {
            string ingredientsList = string.Join(", ", ingredients);
            string outputsList = string.Join(", ", outputs);
            return $"({{{ingredientsList}}} -> {{{outputsList}}})";
        } 
    }

    public struct RecipeResourceInfo{
        public string name;
        public int quantity;

        public RecipeResourceInfo(string _name, int _quantity) {
            name = _name;
            quantity = _quantity;
        }

        public override string ToString()
        {
            return $"({quantity} {name})";
        }
    }
}
