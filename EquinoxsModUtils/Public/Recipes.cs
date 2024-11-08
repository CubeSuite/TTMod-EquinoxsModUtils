using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EquinoxsModUtils.EMULogging;

namespace EquinoxsModUtils
{
    public static partial class EMU 
    {
        /// <summary>
        /// Contains several functions for working with Recipes (SchematicsRecipeData)
        /// </summary>
        public static class Recipes
        {
            /// <summary>
            /// Finds a thresher recipe by the names of it's two outputs. Order does not matter.
            /// Does not work for recipes with one output.
            /// </summary>
            /// <param name="output1Name">The displayName of one of the outputs</param>
            /// <param name="output2Name">The displayName of the other output</param>
            /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
            /// <returns>Thresher recipe if successful, null if not</returns>
            public static SchematicsRecipeData FindThresherRecipeFromOutputs(string output1Name, string output2Name, bool shouldLog = false) {
                if (shouldLog) LogEMUInfo($"Looking for thresher recipe with outputs: '{output1Name}', '{output2Name}'");
                if (!LoadingStates.hasGameDefinesLoaded) {
                    LogEMUWarning("FindThresherRecipeFromOutputs() called before GameDefines.instance has loaded");
                    LogEMUWarning("Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
                    return null;
                }

                foreach (SchematicsRecipeData schematic in GameDefines.instance.schematicsRecipeEntries) {
                    if (schematic.outputTypes.Count() == 2) {
                        if ((schematic.outputTypes[0].displayName == output1Name && schematic.outputTypes[1].displayName == output2Name) ||
                            (schematic.outputTypes[0].displayName == output2Name && schematic.outputTypes[1].displayName == output1Name)) {
                            if (shouldLog) LogEMUInfo("Found thresher recipe");
                            return schematic;
                        }
                    }
                }

                LogEMUWarning("Could not find thresher recipe");
                LogEMUWarning("Try using the resource names in EMU.Names.Resources");
                return null;
            }

            /// <summary>
            /// Tries to find the recipe that matches the ingredient provided in the arguments.
            /// </summary>
            /// <param name="ingredientResID">The ResourceInfo.uniqueId of the ingredient</param>
            /// <param name="shouldLog">Whether EMU Info messages should be logged for this call</param>
            /// <returns>Match recipe if successful, null otherwise</returns>
            public static SchematicsRecipeData TryFindThresherRecipe(int ingredientResID, bool shouldLog = false) {
                if (!LoadingStates.hasGameDefinesLoaded) {
                    LogEMUError($"TryFindThresherRecipe() called before GameDefines.instance loaded");
                    LogEMUWarning($"Try using the event EMU.Events.GameDefinesLoaded or checking LoadingStates.hasGameDefinesLoaded.");
                    return null;
                }

                if (shouldLog) LogEMUInfo($"Attempting to find thresher recipe with ingredient #{ingredientResID}");
                foreach (SchematicsRecipeData recipe in GameDefines.instance.schematicsRecipeEntries) {
                    if (recipe.ingTypes[0].uniqueId == ingredientResID) {
                        if (shouldLog) LogEMUInfo($"Found Thresher recipe for resource #{ingredientResID}");
                        return recipe;
                    }
                }

                LogEMUError($"Could not find a recipe for resource #{ingredientResID}, please check this value.");
                return null;
            }

            /// <summary>
            /// Tries to find the recipe that matches the ingredients and results provided in the arguments.
            /// Save the results if calling more than once.
            /// </summary>
            /// <param name="ingredientIDs">List of the ResourceInfo.uniqueId's of the ingredients</param>
            /// <param name="resultIDs">List of the ResourceInfo.uniqueId's of the results</param>
            /// <param name="shouldLog">Whether EMU Info messages should be logged for this call</param>
            /// <returns>Matching recipe if successful, null otherwise</returns>
            public static SchematicsRecipeData TryFindRecipe(List<int> ingredientIDs, List<int> resultIDs, bool shouldLog = false) {
                if (!LoadingStates.hasGameDefinesLoaded) {
                    LogEMUError($"TryFindRecipe() called before GameDefines.instance loaded");
                    LogEMUWarning($"Try using the event EMU.Events.GameDefinesLoaded or checking LoadingStates.hasGameDefinesLoaded.");
                    return null;
                }

                if (shouldLog) LogEMUInfo("Attempting to find recipe");

                foreach (SchematicsRecipeData recipe in GameDefines.instance.schematicsRecipeEntries) {
                    List<int> recipeIngredients = recipe.ingTypes.Select(item => item.uniqueId).ToList();
                    List<int> recipeResults = recipe.outputTypes.Select(item => item.uniqueId).ToList();

                    if (AreListIntsEqual(ingredientIDs, recipeIngredients, true) &&
                        AreListIntsEqual(resultIDs, recipeResults, true)) {
                        if (shouldLog) LogEMUInfo($"Found recipe");
                        return recipe;
                    }
                }

                LogEMUError($"Could not find recipe, please check the resource IDs passed in the arguments.");
                return null;
            }

            /// <summary>
            /// Tries to find the SchematicsHeader with a title matching the one in the argument.
            /// </summary>
            /// <param name="title">The title to search for.</param>
            /// <param name="shouldLog">Whether an EMU Info message should be logged on success</param>
            /// <returns>The SchematicsHeader if successful, null otherwise</returns>
            public static SchematicsHeader GetSchematicsHeaderByTitle(string title, bool shouldLog = false) {
                if (!LoadingStates.hasGameDefinesLoaded) {
                    LogEMUError("GetSchematicsHeaderByTitle() called before GameDefines.instance has loaded");
                    LogEMUWarning($"Try using the event EMU.Events.GameDefinesLoaded or checking EMU.LoadingStates.hasGameDefinesLoaded.");
                    return null;
                }

                foreach (SchematicsHeader header in GameDefines.instance.schematicsHeaderEntries) {
                    if (header.title == title) {
                        if (shouldLog) LogEMUInfo($"Found SchematicsHeader with title '{title}'");
                        return header;
                    }
                }

                LogEMUError($"Could not find SchematicsHeader with title '{title}'");
                return null;
            }

            /// <summary>
            /// Tries to find the SchematicsHeader with a title matching the one in the argument without checking if GameDefines loaded.
            /// </summary>
            /// <param name="title">The title to search for.</param>
            /// <param name="shouldLog">Whether an EMU Info message should be logged on success</param>
            /// <returns>The SchematicsHeader if successful, null otherwise</returns>
            public static SchematicsHeader GetSchematicsHeaderByTitleUnsafe(string title, bool shouldLog = false) {
                foreach (SchematicsHeader header in GameDefines.instance.schematicsHeaderEntries) {
                    if (header.title == title) {
                        if (shouldLog) LogEMUInfo($"Found SchematicsHeader with title '{title}'");
                        return header;
                    }
                }

                LogEMUError($"Could not find SchematicsHeader with title '{title}'");
                return null;
            }

            /// <summary>
            /// Tries to find the SchematicsSubHeader with a title matching the one in the argument
            /// </summary>
            /// <param name="parentTitle">The title of the parent SchematicsHeader</param>
            /// <param name="title">The title to search for</param>
            /// <param name="shouldLog">Whether an EMU Info message should be logged on success</param>
            /// <returns>The SchematicsSubHeader if successful, null otherwise</returns>
            public static SchematicsSubHeader GetSchematicsSubHeaderByTitle(string parentTitle, string title, bool shouldLog = false) {
                if (!LoadingStates.hasGameDefinesLoaded) {
                    LogEMUError("GetSchematicsSubHeaderByTitle() called before GameDefines.instance has loaded");
                    LogEMUWarning($"Try using the event EMU.Events.GameDefinesLoaded or checking EMU.LoadingStates.hasGameDefinesLoaded.");
                    return null;
                }

                foreach (SchematicsSubHeader subHeader in GameDefines.instance.schematicsSubHeaderEntries) {
                    if (subHeader.title == title && subHeader.filterTag.title == parentTitle) {
                        if (shouldLog) LogEMUInfo($"Found SchematicsSubHeader with title '{title}'");
                        return subHeader;
                    }
                }

                LogEMUError($"Could not find SchematicsSubHeader with title '{title}'");
                return null;
            }

            /// <summary>
            /// Tries to find the SchematicsSubHeader with a title matching the one in the argument without checking if GameDefines has loaded
            /// </summary>
            /// <param name="parentTitle">The title of the parent SchematicsHeader</param>
            /// <param name="title">The title to search for</param>
            /// <param name="shouldLog">Whether an EMU Info message should be logged on success</param>
            /// <returns>The SchematicsSubHeader if successful, null otherwise</returns>
            public static SchematicsSubHeader GetSchematicsSubHeaderByTitleUnsafe(string parentTitle, string title, bool shouldLog = false) {
                foreach (SchematicsSubHeader subHeader in GameDefines.instance.schematicsSubHeaderEntries) {
                    if (subHeader.title == title && subHeader.filterTag.title == parentTitle) {
                        if (shouldLog) LogEMUInfo($"Found SchematicsSubHeader with title '{title}'");
                        return subHeader;
                    }
                }

                LogEMUError($"Could not find SchematicsSubHeader with title '{title}'");
                return null;
            }

            // Private Functions

            private static bool AreListIntsEqual(List<int> first, List<int> second, bool sort) {
                if (first == null && second == null) return true;
                if (first == null || second == null) return false;
                if (first.Count != second.Count) return false;

                if (sort) {
                    first.Sort();
                    second.Sort();
                }

                for (int i = 0; i < first.Count; i++) {
                    if (first[i] != second[i]) return false;
                }

                return true;
            }
        }
    }
}
