# Contents

- [Contents](#contents)
- [Equinox's Mod Utils (EMU)](#equinoxs-mod-utils-emu)
  - [How To Use EMU](#how-to-use-emu)
  - [Names](#names)
  - [Game Loading State Bools](#game-loading-state-bools)
  - [Events](#events)
  - [Functions](#functions)
    - [Misc](#misc)
      - [GetPrivateField](#getprivatefield)
      - [SetPrivateField](#setprivatefield)
      - [IsModInstalled](#ismodinstalled)
      - [CloneObject](#cloneobject)
      - [Free Cursor](#free-cursor)
      - [Notify](#notify)
    - [Resources](#resources)
      - [GetResourceInfoByName](#getresourceinfobyname)
      - [GetResourceInfoByNameUnsafe](#getresourceinfobynameunsafe)
      - [GetResourceIDByName](#getresourceidbyname)
      - [GetMachineTypeFromResID](#getmachinetypefromresid)
      - [Update Resource Unlock](#update-resource-unlock)
      - [UpdateResourceHeaderType](#updateresourceheadertype)
    - [Recipes](#recipes)
      - [TryFindRecipe](#tryfindrecipe)
      - [TryFindThresherRecipe](#tryfindthresherrecipe)
      - [TryFindThresherRecipeFromOutputs](#tryfindthresherrecipefromoutputs)
      - [GetSchematicsHeaderByTitle](#getschematicsheaderbytitle)
      - [GetSchematicsHeaderByTitleUnsafe](#getschematicsheaderbytitleunsafe)
      - [GetSchematicsSubHeaderByTitle](#getschematicssubheaderbytitle)
      - [GetSchematicsSubHeaderByTitleUnsafe](#getschematicssubheaderbytitleunsafe)
    - [Unlocks](#unlocks)
      - [GetUnlockByID](#getunlockbyid)
      - [GetUnlockByName](#getunlockbyname)
      - [GetUnlockByNameUnsafe](#getunlockbynameunsafe)
      - [UpdateUnlockSprite](#updateunlocksprite)
      - [UpdateUnlockTreePosition](#updateunlocktreeposition)
      - [UpdateUnlockTier](#updateunlocktier)
    - [Images](#images)
      - [GetImageForResource](#getimageforresource)
      - [LoadTexture2DFromFile](#loadtexture2dfromfile)
      - [LoadSpriteFromFile](#loadspritefromfile)


# Equinox's Mod Utils (EMU)
A collection of utilities for modding the game Techtonica that make it easier to interact with Resources, Schematics and Unlocks. EMU and it's child branches [EMUAdditions](https://thunderstore.io/c/techtonica/p/Equinox/EMUAdditions/) and [EMUBuilder](missinglink) are a replacement for a modding API.

**Note:** As of V6.0.0, functions for adding content to the game has been moved to [EMUAdditions](https://thunderstore.io/c/techtonica/p/Equinox/EMUAdditions/) and functions for building machines have been moved to [EMUBuilder](missinglink)

## How To Use EMU

1. Download and install EMU from Thunderstore like any other mod.
1. In Visual Studio 22, in the Solution Explorer, right click 'References'
1. Click 'Add Reference'
1. Navigate to 'Techtonica/BepInEx/plugins/EquinoxsModUtils/EquinoxsModUtils.dll'

## Names

EMU contains a const string of the name of every item and every unlock in the game. You can use these with other functions that rely on names to avoid typos. You can access them here:

- `EMU.Names.Resources`
- `EMU.Names.Unlocks`

See example uses in [GetResourceInfoByName](#getresourceinfobyname) and [GetUnlockByName](#getunlockbyname)

**Note:** `EMU.Names.Resources.SafeResources` is a list of all resource names with any entries that aren't actually in the game removed. 

## Game Loading State Bools

EMU features several public static bools that can be used to detect whether certain parts of the game have loaded yet. You can use these to make sure you don't try access those parts of the game before they are ready.

- `EMU.LoadingStates.hasGameStateLoaded` - set to true once `GameState.instance` is no longer `null`
- `EMU.LoadingStates.hasGameDefinesLoaded` - set to true once `GameDefines.instance` is no longer `null`
- `EMU.LoadingStates.hasMachineManagerLoaded` - set to true once `MachineManager.instance` is no long `null`
- `EMU.LoadingStates.hasSaveStateLoaded` - set to true once `SaveState.instance.metadata.worldName` is no longer `null`
- `EMU.LoadingStates.hasTechTreeStateLoaded` - set to true once `TechTreeState.instance` is no longer `null`
- `EMU.LoadingStates.hasGameLoaded` - set to true once the loading screen has been closed by the player pressing any key

Example use:

```csharp
private void OnGUI() {
  if(!EMU.LoadingStates.hasGameLoaded) return;
  DrawGUI(); // Draw GUI now that the player is in game
}
```

## Events

Similar to the `EMU.LoadingState` bools, EMU provides several events that fire during different stages of the loading cycle and during play. You can use these to execute code with the correct timing, e.g. editing recipes once they've loaded.

**Note:** Don't use `EMU.Events.GameSaved` to call functions from `EMUAdditions.CustomData`, see [EMUAdditions Documentation](missinglink) for more info.

- `EMU.Events.GameStateLoaded` - Fires when `EMU.LoadingStates.hasGameStateLoaded` is set to `true`
- `EMU.Events.GameDefinesLoaded` - Fires when `EMU.LoadingStates.hasGameDefinesLoaded` is set to `true`
- `EMU.Events.MachineManagerLoaded` - Fires when `EMU.LoadingStates.hasMachineManagerLoaded` is set to `true`
- `EMU.Events.SaveStateLoaded` - Fires when `EMU.LoadingStates.hasSaveStateLoaded` is set to `true`
- `EMU.Events.TechTreeStateLoaded` - Fires whe `EMU.LoadingStates.hasTechTreeStateLoaded` is set to `true`
- `EMU.Events.GameSaved` - Fires whenever the game is saved. 
- `EMU.Events.GameLoaded` - Fires when `EMU.LoadingStates.hasGameLoaded` is set to `true`
- `EMU.Events.GameUnloaded` - Fires when the player loads a save while in-game

## Functions

EMU's many helpful functions have now been split up into sub classes to help organise them more. Many of these functions have an optional bool argument as their last one called `shouldLog`. When this is true, extra Info messages will be logged during the call for help with debugging. Don't forget to change these to false / omit them before releasing your mod.

### Misc

These functions don't fit into the other categories, so they are defined inside the `EMU` class.

---
#### GetPrivateField
```csharp
public static void GetPrivateField<T>(
  string name, 
  T instance
)
```

Gets the value of a private field from an instance of a non-static class.

Example use:

```csharp
public void DebugJetpack(){
  bool isActive = EMU.GetPrivateField<bool>("_isActive", Player.instance.equipment.hoverPack);
  Debug.Log($"Jetpack Active: {isActive}");
}
```

---
#### SetPrivateField

```csharp
public static void SetPrivateField<T>(
  string name,
  T instance,
  object value
)
```

Sets the value of a private field of an instance of a non-static class.

Example use:

```csharp
public void DisableJetpack(){
  EMU.SetPrivateField<bool>("_isActive", Player.instance.equipment.hoverPack, false);
}
```

---
#### IsModInstalled

```csharp
public static bool IsModInstalled(
  string dllName,
  bool shouldLog = false
)
```

Checks if a mod is installed by searched the plugins folder for `dllName`. You do not need to include '.dll' at the end of the argument.

Example use:

```csharp
public void AddToQuickBuild(){
  if(EMU.IsModInstalled("QuickBuild")){
    QuickBuild.AddItem("Root/Production/Power", reactorMenuItem);
  }
}
```

---
#### CloneObject

```csharp
public static void CloneObject<T>(
  T original, 
  ref T target
)
```

Copies the values of all members from `original` onto `target`.

Example use:

**Note:** This is just an example of how to use the function, this is not how you should create new Resources. Use [EMUAdditions](https://thunderstore.io/c/techtonica/p/Equinox/EMUAdditions/) for that.

```csharp
public void CreateNewResource(){
  ResourceInfo newResource = ScriptableObject.CreateInstance<ResourceInfo>();
  ResourceInfo assembler = EMU.Resources.GetResourceByName(EMU.Names.Resources.Assembler);
  EMU.CloneObject(assembler, ref newResource);
}
```

---
#### Free Cursor

```csharp
public static void FreeCursor(
  bool free
)
```

Unlocks the cursor from the center of the screen and allows the player to move it around. Also notifies the game that the cursor is free. Use the `free` bool argument to toggle whether the cursor is free or not. This function mostly exists to be used by [CaspuinoxGUI](missinglink), consider using that for any GUI needs.

Example use:

```csharp
private void OnGUI(){
  if(UnityInput.Current.GetKey(toggleGuiShortcut)){
    showGUI = !showGUI;
    EMU.FreeCursor(showGUI);

    if(showGUI) DrawMyGUI();
  }
}
```

---
#### Notify
```csharp
public static void Notify(
  string message
)
```

Displays a notification at the top of the screen of the text passed in the `message` argument.

Example use:

```csharp
private void OnFinishedPastingBlueprint(){
  EMU.Notify("Blueprint Paste Finished!");
}
```

---
### Resources

EMU provides serveral functions that help you work with resources:

---
#### GetResourceInfoByName

```csharp
public static ResourceInfo GetResourceInfoByName (
  string name,
  bool shouldLog = false
)
```

This function allows you to find the ResourceInfo of any item by using its display name. Returns `null` if not found.

Example use:

```csharp
ResourceInfo ironFrame = EMU.Resources.GetResourceInfoByName(EMU.Names.Resources.IronFrame);
Player.instance.inventory.AddResources(ironFrame, 1);
```

---
#### GetResourceInfoByNameUnsafe

```csharp
public static ResourceInfo GetResourceInfoByNameUnsafe(
  string name,
  bool shouldLog = false
)
```

Does the same as [GetResourceInfoByName()](#getresourceinfobyname), but won't check `EMU.LoadingStates.hasGameDefinesLoaded` first. Only ever use this in patches of `GameDefines` functions.

---
#### GetResourceIDByName

```csharp
public static int GetResourceIDByName(
  string name,
  bool shouldLog = false
)
```

Finds a Resource by it's `displayName` field and returns it's `uniqueId` field.

Example use:

```csharp
int ironFrameID = EMU.Resources.GetResourceIDNyName(EMU.Names.Resources.IronFrame);
Player.instance.inventory.AddResources(ironFrameID, 1);
```
---
#### GetMachineTypeFromResID

```csharp
pubilc static MachineTypeEnum GetMachineTypeFromResID(
  int resID
)
```

Returns the matching `MachineTypeEnum` entry for the provided Resource ID if there is one, returns `MachineTypeEnum.NONE` for failed calls.

Example use:


```csharp
private bool IsStructure(int resID) {
  MachineTypeEnum type = EMU.Resources.GetMachineTypeFromResID(resID);
  return type == MachineTypeEnum.Structure;
}
```
---
#### Update Resource Unlock

```csharp
public static void UpdateResourceUnlock(
  string resourceName, 
  string unlockName, 
  bool shouldLog = false
)
```

Finds the `ResourceInfo` and `Unlock` specified in the arguments, then updates the `.unlock` field of the `ResourceInfo` to the specified `Unlock`.

Example use:

```csharp
public void EarlyBaseBuild(){
  EMU.Resources.UpdateResourceUnlock(EMU.Names.Resources.CalycitePlatform5x5, EMU.Names.Unlocks.PlatformsI);
}
```
---
#### UpdateResourceHeaderType

```csharp
public static UpdateResourceHeaderType(
  string resourceName, 
  SchematicsSubHeader header,
  bool shouldLog = false  
)
```

Finds the `ResourceInfo` specified in the arugments, then updates the `.subHeader` field of it.

Example use:

```csharp
public void MoveChestNextToDrill() {
  ResourceInfo drill = EMU.Resources.GetResourceInfoByName(EMU.Names.Resources.MiningDrill);
  EMU.Resources.UpdateResourceHeaderType(EMU.Names.Resources.Container, drill.headerType);
} 
```

---
### Recipes

EMU provides some functions that allow you to find and modify existing recipes.

---
#### TryFindRecipe

```csharp
public static SchematicsRecipeData TryFindRecipe(
  List<int> ingredientIDs,
  List<int> resultIDs,
  bool shouldLog = false
)
```

This function tries to find a recipe that matches the ingredients and results provided in the arguments. Returns `null` if no recipe is found.

Example use:

```csharp
private void MakeIronFrameCheap(){
  ResourceInfo ironIngot = EMU.Resources.GetResourceInfoByName(EMU.Names.Resources.IronIngot);
  ResourceInfo ironFrame = EMU.Resources.GetResourceInfoByName(EMU.Names.Resources.IronFrame);

  List<int> ingredients = new List<int>(){ ironIngot.uniqueId };
  List<int> results = new List<int>(){ ironFrame.uniqueId };

  SchematicsRecipeData recipe = EMU.Recipes.TryFindRecipe(ingredients, results);
  if(recipe != null){
    recipe.runtimeIngQuantities[0] = 1;
  }
}
```
---
#### TryFindThresherRecipe

```csharp
public static SchematicsRecipeData TryFindThresherRecipe(
  int ingredientResID, 
  bool shouldLog = false
)
```

This functions looks through Thresher recipes to find one for the ingredient provided in the argument. Returns `null` if no recipe is found.

Example use:

```csharp
public void DoubleKindleOutput(){
  ResourceInfo kindlevine = EMU.Resources.GetResourceInfoByName(EMU.Names.Resources.Kindlevine);
  SchematicsRecipeData thresherRecipe = EMU.Recipes.TryFindThresherRecipe(kindlevine.uniqueId);
  thresherRecipe.outputQuantities[0] *= 2;
  thresherRecipe.outputQuantities[1] *= 2;
}
```
---
#### TryFindThresherRecipeFromOutputs

```csharp
public static SchematicsRecipeData FindThresherRecipeFromOutputs(
  string output1Name, 
  string output2Name, 
  bool shouldLog = false
)
```

This function looks through Thresher recipes to find one that matches the outputs specified in the arugments. The order of `outpu1Name` and `output2Name` doesn't matter. Does not work for recipes with one output. Returns `null` if no recipe is found.

Example use:

```csharp
public void DoubleKindleOutput(){
  SchematicsRecipeData thresherRecipe = EMU.Recipes.FindThresherRecipeByOutputs(EMU.Names.Resources.KindlevineSeed, EMU.Names.Resources.KindlevineStems);
  thresherRecipe.outputQuantities[0] *= 2;
  thresherRecipe.outputQuantities[1] *= 2;
}
```
---
#### GetSchematicsHeaderByTitle

```csharp
public static SchematicsHeader GetSchematicsHeaderByTitle(
  string title,
  bool shouldLog = false
)
```

This function searches for the SchematicsHeader with the `.title` member that matches the `title` argument. Returns `null` if no `SchematicsHeader` is found.

Example use:

```csharp
public static void ChangeHeaderOrder(){
  SchematicsHeader production = EMU.Recipes.GetSchematicsHeaderByTitle("Production");
  SchematicsHeader logistics = EMU.Recipes.GetSchematicsHeaderByTitle("Logistcs");

  production.order = 1;
  logistics.order = 0;
}
```
---
#### GetSchematicsHeaderByTitleUnsafe

```csharp
public static SchematicsHeader GetSchematicsHeaderByTitleUnsafe(
  string title,
  bool shouldLog = false
)
```

Does the same as [GetSchematicsHeaderByTitle()](#getschematicsheaderbytitle), but doesn't check `EMU.LoadingStates.hasGameDefinesLoaded` first. Only use this in patches of 'GameDefines' functions.

---
#### GetSchematicsSubHeaderByTitle

```csharp
public static SchematicsSubHeader GetSchematicsSubHeaderByTitle(
  string parentTitle,
  string title,
  bool shouldLog = false
) 
```

This function searches for the SchematicsSubHeader that has a `.title` member that matches the `title` argument and is a child of the SchematicsHeader specified by the `parentTitle` argument.

Example use:

```csharp
public void ReorderLogistics(){
  SchematicsSubHeader utility = EMU.Recipes.GetSchematicsSubHeaderByTitle("Logistics", "Utility");
  SchematicsSubHeader power = EMU.Recipes.GetSchematicsSubHeaderByTitle("Logistics", "Power");

  utility.order = 3;
  power.order = 2;
}
```
---
#### GetSchematicsSubHeaderByTitleUnsafe

```csharp
public static SchematicsSubHeader GetSchematicsSubHeaderByTitleUnsafe(
  string parentTitle,
  string title,
  bool shouldLog = false
) 
```

Does the same as [GetSchematicsSubHeaderByTitle()](#getschematicssubheaderbytitle), but doesn't check `EMU.LoadingStates.hasGameDefinesLoaded` first. Only use this in patches of 'GameDefines' functions.

---
### Unlocks

EMU provides some functions that allow you to find and modify Unlocks.

---
#### GetUnlockByID

```csharp
public static Unlock GetUnlockByID(
  int id, 
  bool shouldLog = false
)
```

This function finds the `Unlock` with the `.uniqueId` given in the `id` argument. Returns `null` if no `Unlock` is found.

Example use:

```csharp
private void DebugUnlock(int id){
  Unlock unlock = EMU.Unlocks.GetUnlockByID(id);
  string name = LocsUtility.TranslateStringFromHash(unlock.displayNameHash);
  Debug.Log($"Unlock {name} is unlocked: {TechTreeState.instance.IsUnlockActive(unlock)}");
}
```

---
#### GetUnlockByName

```csharp
public static Unlock GetUnlockByName(
  string name,
  bool shouldLog = false
)
```

This function searches for the `Unlock` that matches the name given in the argument. 

Example use:

```csharp
public static Unlock ActivateUnlock(string name){
  Unlock unlock = EMU.Unlocks.GetUnlockByName(name);
  TechTreeState.instance.ActivateUnlock(unlock.uniqueId);
}
```

---
#### GetUnlockByNameUnsafe

```csharp
public static Unlock GetUnlockByNameUnsafe(
  string name,
  bool shouldLog = false
)
```

Does the same as [GetUnlockByName()](#getunlockbyname), but without checking `EMU.LoadingStates.hasGameDefinesLoaded` first. Only use this in patches to `GameDefines` functions.

---
#### UpdateUnlockSprite

```csharp
public static void UpdateUnlockSprite(
  int unlockID, 
  Sprite sprite,
  bool shouldLog = false
)
```

```csharp
public static void UpdateUnlockSprite(
  string displayName, 
  Sprite sprite,
  bool shouldLog = false
)
```

This function changes the `.sprite` member of the `Unlock` specified with either the `unlockID` or `displayName` arguments.

Example use:

```csharp
public static void OnGameDefinesLoaded(){
  Sprite sprite = EMU.Images.LoadSpriteFromFile("MyMod.Images.UnlockSprite.png");
  EMU.Unlocks.UpdateUnlockSprite(myUnlockName, sprite);
}
```

---
#### UpdateUnlockTreePosition

```csharp
public static void UpdateUnlockTreePosition(
  int unlockID,
  int treePosition,
  bool shouldLog = false
)
```

```csharp
public static void UpdateUnlockTreePosition(
  string displayName,
  int treePosition,
  bool shouldLog = false
)
```

This function changes the `.treePosition` member of the `Unlock` specified with either the `unlockID` or `displayName` arguments. Use this to move Unlocks horizontally on the Tech Tree.

Exmaple use:

```csharp 
public void AlignMyNewUnlock(){
  Unlock conveyorMk2 = EMU.Unlocks.GetUnlockByName(EMU.Names.Unlocks.ConveyorBeltMKII);
  EMU.Unlocks.UpdateUnlockTreePosition(myNewUnlockName, conveyorMk2.treePosition);
}
```

---
#### UpdateUnlockTier

```csharp
public static void UpdateUnlockTier(
  int unlockID,
  TechTreeState.ResearchTier tier,
  bool shouldLog = false
)
```

```csharp
public static void UpdateUnlockTier(
  string displayName,
  TechTreeState.ResearchTier tier,
  bool shouldLog = false
)
```

This function changes the  `.requiredTier` member of the `Unlock` specified with either the `unlockID` or `displayName` arguments. Use this to move Unlocks vertically on the Tech Tree.

Example use:

```csharp
public void AlignMyNewUnlock(){
  Unlock conveyorMk2 = EMU.Unlocks.GetUnlockByName(EMU.Names.Unlocks.ConveyorBeltMKII);
  EMU.Unlocks.UpdateUnlockTier(myNewUnlockName, conveyorMk2.requiredTier);
}
```

---
### Images

These functions allow you to load images from files to use with either new content or with GUI. 

**Note:** You must set the Build Action of image files to 'Embedded Resource' for these functions to work. See tutorials for [EMUAdditions](missinglink) and [CaspuinoxGUI](missinglink) for more info.

---
#### GetImageForResource

```csharp
public static Texture2D GetImageForResource(
  string name, 
  bool shouldLog = false
)
```

This function creates a `Texture2D` from a `ResourceInfo`'s `.sprite` member. Use this for when you want to include an image of an item in your GUI. 

**Notes:**
- Consider using a `ResourceButton` from [CaspuinoxGUI](missinglink) instead.
- Save the result to a variable to save performance.

Example use:

```csharp
Texture2D limestoneIcon;

public void Awake(){
  limestoneIcon = EMU.Images.GetImageForResource(EMU.Names.Resources.Limestone);
}

public void OnGUI(){
  GUI.Box(new Rect(0, 0, 100, 100), "", new GUIStyle(){ normal = { background = limestoneIcon } });
}
```

---
#### LoadTexture2DFromFile

```csharp
public static Texture2D LoadTexture2DFromFile(
  string path, 
  bool shouldLog = false
)
```

This functions reads the content of the image file specified by the `path` argument and converts it to a `Texture2D`. You can use this function for custom controls with [CaspuinoxGUI](missinglink), see [Tutorials](missinglink) for more information. There is a third argument called `assembly` which is for internal use only.

Example use:

```csharp
Texture2D oneToOneRecipe;

public void Awake(){
  oneToOneRecipe = EMU.Images.LoadTexture2DFromFile("RecipeBook.Images.OneToOne.png");
}

public void OnGUI(){
  GUI.Box(new Rect(0, 0, 100, 100), "", new GUIStyle(){ normal = { background = oneToOneRecipe } });
}
```

---
#### LoadSpriteFromFile

```csharp
public static Sprite LoadSpriteFromFile(
  string path,
  bool shouldLog = false
)
```

This function reads the content of the image file specified by the `path` argument and converts it to a `Sprite`. You can use this function for custom icons for new content added by [EMUAdditions](https://thunderstore.io/c/techtonica/p/Equinox/EMUAdditions/).

Example use:

```csharp
public static void OnGameDefinesLoaded(){
  Sprite sprite = EMU.Images.LoadSpriteFromFile("MyMod.Images.UnlockSprite.png");
  EMU.Unlocks.UpdateUnlockSprite(myUnlockName, sprite);
}
```