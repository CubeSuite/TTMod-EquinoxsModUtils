# Contents

- [EquinoxsModUtils](#equinoxsmodutils)
  - [Objects & Variables](#objects--variables)
  - [Functions](#functions)
    - [Resources](#resources)
      - [GetResourceInfoByName](#getresourceinfobyname)
      - [GetResourceIDByName](#getresourceidbyname)
    - [Schematics](#schematics)
      - [FindThresherRecipeFromOutputs](#findthresherrecipefromoutputs)
    - [Unlocks (Techs)](#unlocks-techs)
      - [GetUnlockById](#getunlockbyid)
      - [GetUnlockByName](#getunlockbyname)
      - [AddNewUnlock](#addnewunlock)
      - [UpdateUnlockSprite](#updateunlocksprite)
    - [Reflection And Misc](#reflection-and-misc)
      - [GetPrivateField](#getprivatefield) 
      - [SetPrivateField](#setprivatefield)
      - [NullCheck](#nullcheck)
    - [Machine Building](#machine-building)
      - [BuildMachine](#buildmachine)
        - [Building A Simple Machine](#building-a-simple-machine)
        - [Building A Filter Inserter / Assembler With Recipe](#building-a-filter-inserter--assembler-with-recipe)
        - [Building A Conveyor](#building-a-conveyor)
  - [Events](#events)
    - [GameStateLoaded](#gamestateloaded)
    - [GameDefinesLoaded](#gamedefinesloaded)
    - [SaveStateLoaded](#savestateloaded)
    - [TechTreeStateLoaded](#techtreestateloaded)
  - [How To Add A New Unlock / Tech](#how-to-add-a-new-unlock--tech)

# EquinoxsModUtils
A collection of utilities for modding the game Techtonica that make it easier to interact with Resources, Schematics and Unlocks. Can be used to add new Unlocks to the Tech Tree.

## Objects & Variables

```ModUtils.ResourceNames``` and ```ModUtils.UnlockNames``` are two static classes that contain const strings of the names of ```Resources``` and ```Unlocks```. These have been extracted from the game, so they are a perfect match. Using these allows you to avoid typos, which is especially important when using functions like ```GetResourceInfoByName(string name)```.

* ```public static bool hasGameStateLoaded``` set to true once ```GameState.instance``` has loaded. Useful for checking if data you need has loaded yet.  
* ```public static bool hasGameDefinesLoaded``` set to true once ```GameDefines.instance``` has loaded. Useful for checking if data you need has loaded yet.  
* ```public static bool hasSaveStateLoaded``` set to true once ```SaveState.instance``` has loaded. Useful for checking if data you need has loaded yet.  
* ```public static bool hasTechTreeStateLoaded``` set to true once ```TechTreeState.instance``` has loaded. Useful for checking if data you need has loaded yet.  

## Functions

### Resources

#### GetResourceInfoByName

```csharp
public static ResourceInfo GetResourceInfoByName(
  string name,
  bool shouldLog = false
)
```

This function allows you to find the ```ResourceInfo``` of any resource by using its display name. Returns ```null``` if not found.
Example use:

```csharp
ResourceInfo info = ModUtils.GetResourceInfoByName(ModUtils.ResourceNames.IronFrame);
if (info != null) {
  info.ingredient1Quantity = 2;
}
```
#### GetResourceIDByName

```csharp
public static int GetResourceIDByName(
  string name,
  bool shouldLog = false
)
```

This function finds the unique resource ID for the given resource name. Returns ```null``` if not found.
Example use:

```csharp
int limestoneID = ModUtils.GetResourceIDByName(ModUtils.ResourceNames.Limestone);
if (limestoneID != -1) {
  chestInstance.GetInventory(0).AddResourcesToSlot(limestoneID, slotNum, out remainingCount, quantity);
}
```
### Schematics

#### FindThresherRecipeFromOutputs

```csharp
public static SchematicsRecipeData FindThresherRecipeFromOutputs(
  string output1Name,
  string output2Name,
  bool shouldLog = false
)
```

This function finds a thresher recipe based on its two outputs. This does not work for recipes with one output. Returns ```null``` if not found.
Example use:

```csharp
SchematicsRecipeData recipe = ModUtils.FindThresherRecipeFromOutputs(ModUtils.ResourceNames.KindlevineSeed, ModUtils.ResourceNames.KindlevineStems);
if (recipe != null) {
  recipe.outputQuantities[0] = 2;
}
```

#### TryFindRecipe

```csharp
public static SchematicsRecipeData TryFindRecipe(
  List<int> ingredientIDs,
  List<int> resultIDs,
  bool shouldLog = false
)
```

This function tries to find the recipe that takes the ingredients provided in the argument and produces the results provided in the argument.
Example use:

```csharp
ResourceInfo copperIngot = ModUtils.GetResourceInfoByName(ResourceNames.CopperIngot);
ResourceInfo ironIngot = ModUtils.GetResourceInfoByName(ResourceNames.IronIngot);
ResourceInfo mechComps = ModUtils.GetResourceInfoByName(ResourceNames.MechanicalComponents);

List<int> ingredients = new List<int>() {
    copperIngot.uniqueId,
    ironIngot.uniqueId,
};
List<int> results = new List<int>() { mechComps.uniqueId };
SchematicsRecipeData recipe = ModUtils.TryFindRecipe(ingredients, results);
recipe.ingQuantities[1] = 2;

```

### Unlocks (Techs)

#### GetUnlockById

```csharp
public static Unlock GetUnlockByID(
  int id,
  bool shouldLog = false
)
```

This function finds the ```Unlock``` with the id given in the argument. Returns ```null``` if not found.
Example use:

```csharp
Unlock firstUnlock = ModUtils.GetUnlockByID(0);
if (firstUnlock != null){
  Debug.Log($"First Unlock: {firstUnlock.displayName}");
}
```

#### GetUnlockByName

```csharp
public static Unlock GetUnlockByName(
  string name,
  bool shouldLog = false
)
```

This function finds the ```Unlock``` that matches the name given in the argument. Returns ```null``` if not found.
Example use:

```csharp
Unlock stackInserterUnlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.StackInserter);
if (stackInserterUnlock != null) {
  Sprite stackInserterSprite = stackInserterUnlock.sprite;
}
```
#### AddNewUnlock

```csharp
public static void AddNewUnlock(
  NewUnlockDetails details,
  bool shouldLog = false
)
```

This function takes basic information about a new ```Unlock``` and registers it to be added at the correct times during the load cycle. Handles all the heavy lifting for you.
See the end section of this readme for a guide on adding new Unlocks.

#### UpdateUnlockSprite

```csharp
public static void UpdateUnlockSprite(
  int unlockID,
  Sprite sprite,
  bool shouldLog = false
)
```

This function is used to change the sprite of an unlock. Useful for if you want your new Unlock to use the same sprite as a vanilla Unlock.
Example use:

**Note:** Currently there isn't a way to get the unique ID of new Unlocks. This will be added in a future update.

```csharp
Unlock conveyor2Unlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.ConveyorBeltMKII);
if (conveyor2Unlock != null) {
  ModUtils.UpdateUnlockSprite(myCustomUnlockID, conveyor2Unlock.sprite);
}
```

```csharp
public static void UpdateUnlockSprite(
  string displayName,
  Sprite sprite,
  bool shouldLog = false
)
```

This function does the same as above, but takes the ```displayName``` of the Unlock to find the Unlock to update.
Exmaple use:

```csharp
Unlock conveyor2Unlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.ConveyorBeltMKII);
if (conveyor2Unlock != null) {
  ModUtils.UpdateUnlockSprite("My Custom Unlock", conveyor2Unlock.sprite);
}
```

#### UpdateUnlockTreePosition

```csharp
public static void UpdateUnlockTreePosition(
  int unlockID,
  int treePosition,
  bool shouldLog = false
)
```

This function updates the horizontal position of the Unlock on the Tech Tree. Useful for aligning new Unlocks with vanilla ones.
Example use:

**Note:** Currently there isn't a way to get the unique ID of new Unlocks. This will be added in a future update.

```csharp
Unlock conveyor2Unlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.ConveyorBeltMKII);
if (conveyor2Unlock != null) {
  ModUtils.UpdateUnlockTreePosition(myCustomUnlockID, conveyor2Unlock.treePosition);
}
```

```csharp
public static void UpdateUnlockTreePosition(
  string displayName,
  int treePosition,
  bool shouldLog = false
)
```

This function does the same as above, but takes the ```displayName``` of the Unlock to find the Unlock to update.
Example use:

```csharp
Unlock conveyor2Unlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.ConveyorBeltMKII);
if (conveyor2Unlock != null) {
  ModUtils.UpdateUnlockTreePosition("My Custom Unlock", conveyor2Unlock.treePosition);
}
```

### Reflection And Misc

#### GetPrivateField

```csharp
public static object GetPrivateField<T>(
  string name,
  T instance
)
```

This function retrieves the value of a private field of a non-static class ```T``` from the instance of that class given in the arguments. Note that you do not need to include the ```<T>``` after the function name, this is provided by the ```T``` argument.
Returns null if the field cannot be found (and logs an error), can also return null if the value of the field is null for the given instance.
Example use:

```csharp
object _currentBuilderValue = ModUtils.GetPrivateField("_currentBuilder", Player.instance.builder);
if (object != null){
  PlayerBuilder builder = (PlayerBuilder)_currentBuilderValue;
}
```

#### SetPrivateField

```csharp
public static void SetPrivateField<T>(
  string name,
  T instance,
  object value
)
```

This function sets the value of a private field of a non-static class ```T``` of the instance of that class given in the arguments. Note that you do not need to include the ```<T>``` after the function name, this is provided by the ```T``` argument.
Logs an error if the field cannot be found.
Example use:

```csharp
ProceduralBuilder builder = Player.instance.GetBuilderForType(...);
ModUtils.SetPrivateField("_currentBuilder", Player.instance.builder, builder);
```

#### NullCheck

```csharp
public static bool NullCheck(
  object obj,
  string name,
  bool shouldLog = false
)
```

This function can be used to check if the provided object is null and log the result. It's a little cleaner to use this than multiple manual null checks if there's more than one to do in a function.
Returns ```true``` if ```obj``` is *not* null. Think of this function as a test, see example use for a better explanation.

Note: If ```obj``` is null, a warning level messaeg will always be logged, regardless of ```shouldLog```.

Example use:

```csharp
if(NullCheck(UIManager.instance, "UI Manager"){
  if(NullCheck(UIManager.instance.techTreeMenu, "Tech Tree Menu")){
    TechTreeNode node = UIManager.instance.techTreeMenu.GridUI.GetNodeByUnlock(...);
  }
}
```
This could also be written using guard clauses:
```csharp
if(!NullCheck(UIManager.instance, "UI Manager")) return;
if(!NullCheck(UIManager.instance.techTreeMenu, "Tech Tree Menu")) return;
TechTreeNode node = UIManager.instance.techTreeMenu.GridUI.GetNodeByUnlock(...);
```
Read those first two lines as 'If the null check fails, return'

### Machine Building

#### BuildMachine

```csharp
public static void BuildMachine(
  int resID,
  GridInfo gridInfo,
  bool shouldLog = false,
  int variationIndex = -1,
  int recipe = -1,
  ConveyorBuildInfo.ChainData? chainData = null,
  bool reverseConveyor = false
)
```

```csharp
public static void BuildMachine(
  string resourceName,
  GridInfo gridInfo,
  bool shouldLog = false,
  int variationIndex = -1,
  int recipe = -1,
  ConveyorBuildInfo.ChainData? chainData = null,
  bool reverseConveyor = false
)
```

These functions are used to build new machines in the correct way to avoid machines being built with invisible static meshes. Note, if you try build an Assembler with a recipe set, it will still be partially invisible.
For most machines, you only need to provide the first two arguments. For building an Assembler with the recipe set, or a Fitler Inserter with the filter set, provide the ```shouldLog``` and ```recipe``` arguments.
For building Conveyors, provide all arguments. Use ```-1``` for the ```variationIndex``` and ```recipe``` fields. I recommend enabling logging while developing your mod and turning it off when you are ready to release.

Example uses:

##### Building A Simple Machine
```csharp
bool shouldLog = true;
Vector3Int position = new Vector3Int(100, 10, 100);
float yawRotation = 0;
GridInfo gridInfo = new GridInfo(){
  minPos = position,
  yawRot = yawRotation
};
 
ModUtils.BuildMachine(ResourceNames.Smelter, gridInfo, shouldLog);
```

##### Building A Structure
```csharp
bool shouldLog = true;
Vector3Int position = new Vector3Int(100, 10, 100);
float yawRotation = 0;
GridInfo gridInfo = new GridInfo(){
  minPos = position,
  yawRot = yawRotation
};

int variationIndex = structureInstance.GetCommonInfo().variationIndex;
 
ModUtils.BuildMachine(ResourceNames.Smelter, gridInfo, shouldLog, variationIndex);
```

#### Building A Filter Inserter / Assembler With Recipe
```csharp
bool shouldLog = true;
Vector3Int position = new Vector3Int(100, 10, 100);
float yawRotation = 0;
GridInfo gridInfo = new GridInfo(){
  minPos = position,
  yawRot = yawRotation
};

int recipe = assemblerInstance.targetRecipe;
int filter = filterInserterInstance.filterType;

ModUtils.BuildMachine(ResourceNames.Assembler, gridInfo, shouldLog, -1, recipe);
ModUtils.BuildMachine(ResourceNames.FilterInserter, inserterGridInfo, shouldLog, -1, filter);
```

#### Building A Conveyor

```csharp
bool shouldLog = true;
Vector3Int position = new Vector3Int(100, 10, 100);
float yawRotation = 0;
GridInfo gridInfo = new GridInfo(){
  minPos = position,
  yawRot = yawRotation
};

ChainData chainData = new ChainData(){
  count = 1,
  shape = beltShape,
  rotation = yawRotation,
  start = position,
  height = height,
  topYawRot = topYawRot,
  inputBottom = inputBottom
};

bool buildInReverse = true;
 
ModUtils.BuildMachine(ResourceNames.Smelter, gridInfo, shouldLog, -1, -1, chainData, buildInReverse);
```

## Events

### GameStateLoaded

```csharp
public static event EventHandler GameStateLoaded;
```

This event is fired when ```GameState.instance``` is no longer null. Use this event to run code that needs to access ```GameState.instance``` at the correct time.
Example use:

**Note:** the ```sender``` argument is ```GameState.instance```, so you can access it through that or the normal approach. Both used in the example below.

```csharp
void Awake() {
  ModUtils.GameStateLoaded += OnGameStateLoaded;
}

private void OnGameStateLoaded(object sender, EventArgs e) {
  GameState instance = sender as GameState;
  int stackInserterSize = GameState.instance.stackInserterSize;
  ...
}
```

### GameDefinesLoaded

```csharp
public static event EventHandler GameDefinesLoaded;
```

This event is fired when ```GameDefines.instance``` is no longer null. Use this event to run code that needs to access ```GameDefines.instace``` at the correct time.
Example use:

**Note:** the ```sender``` argument is ```GameDefines.instance```, so you can access it through that or the normal approach. Both used in the example below.

```csharp
void Awake() {
  ModUtils.GameDefinesLoaded += OnGameDefinesLoaded;
}

private void OnGameDefinesLoaded(object sender, EventArgs e) {
  GameDefines instance = sender as GameDefines;
  foreach (ResourceInfo info in GameDefines.instance.resources) {
    Debug.Log(info.displayName);
  }
}
```

### SaveStateLoaded

```csharp
public static event EventHandler SaveStateLoaded;
```

This event is fired when ```SaveState.instance``` is no longer null. Use this event to run code that needs to access ```SaveState.instance``` at the correct time.
Example use:

**Note:** the ```sender``` argument is ```SaveSate.instance```, so you can access it through that or the normal approach. Both used in the example below.

```csharp
void Awake() {
  ModUtils.SaveSateLoaded += OnSaveStateLoaded;
}

private void OnSaveStateLoaded(object sender, EventArgs e) {
  SaveState instance = sender as SaveState;
  string worldName = SaveState.instance.levelName;
}
```

### TechTreeStateLoaded

```csharp
public static event EventHandler TechTreeStateLoaded;
```

This event is fired when ```TechTreeState.instance``` is no longer null. Use this event to run code that needs to access ```TechTreeState.instance``` at the correct time.
Example use:

**Note:** the ```sender``` argument is ```TechTreeState.instance```, so you can access it through that or the noraml approach. Both used in the example below.

```csharp
void Awake() {
  ModUtils.TechTreeStateLoaded += OnTechTreeStateLoaded;
}

private void OnTechTreeStateLoaded(object sender, EventArgs e) {
  TechTreeState instance = sender as TechTreeState;
  if (TechTreeState.instance.IsUnlockActive(myUnlockID) {
    ...
  }
}
```

## How To Add A New Unlock / Tech

First, in your ```Awake()``` function, create an instance of the ```NewUnlockDetails``` class. Ensure that you provide data for every field like in the example below.
If your new Unlock does not depend on any others, do not include the ```dependencyNames``` field. An Unlock is limited to two dependencies. EMU will not add the new Unlock if you provide more than this.
If you want to set the Sprite to a vanilla one, leave that field for now and continue this guide to the end.
You must use CoreType.Red (Tier 1, purple in-game) or CoreType.Blue (Tier 2, blue in-game). Attempting to use another colour will result in the new Unlock not being added.

```csharp
void Awake() {
  NewUnlockDetails details = new NewUnlockDetails() {
    category = Unlock.TechCategory.Terraforming,
    coreTypeNeeded = ResearchCoreDefinition.CoreType.Blue,
    coreCountNeeded = 1500,
    dependencyNames = new List<string>(){ ModUtils.UnlockNames.MiningDrill },
    description = "Description of my new Unlock",
    displayName = "Name of my new Unlock",
    numScansNeeded = 0,
    requiredTier = TechTreeState.ResearchTier.Tier0,
    treePosition = 50
  };
}
```

Next, call ```ModUtils.addNewTech(NewUnlockDetails details)```, passing in your instance of ```NewUnlockDetails```.

```csharp
void Awake() {
  NewUnlockDetails details = new NewUnlockDetails(){...};
  ModUtils.AddNewUnlock(details);
}
```

This function registers the new Unlock to be added to the game at the correct time using the events above. If you used your own Sprite, you're done now. EMU will handle adding the Unlock and tracking its UnlockState from here.
You can use the ```UpdateUnlock...``` functions to alter the new ```Unlock``` once ```GameDefines``` has loaded.

To assign a vanilla sprite to the new unlock, we need to find the sprite once ```GameDefines``` has loaded. Create an event called ```OnGameDefinesLoaded``` and bind it to ```ModUtils.GameDefinesLoaded```.
In the example below, we'll use the sprite of the Stack Inserter Unlock.

```csharp
void Awake() {
  NewUnlockDetails details = new NewUnlockDetails(){...};
  ModUtils.AddNewUnlock(details);

  ModUtils.GameDefinesLoaded += OnGameDefinesLoaded;
}

private void OnGameDefinesLoaded(object sender, EventArgs e) {
  Unlock stackInserterUnlock = ModUtils.GetUnlockByName(UnlockNames.StackInserter);
  if (stackInserterUnlock != null) {
    ModUtils.UpdateUnlockSprite("Name of my new Unlock", stackInserterUnlock.sprite);
  }
}
```

You can also use this event to horizontally align your new Unlock with vanilla ones on the TechTree:

```csharp
private void OnGameDefinesLoaded(object sender, EventArgs e) {
  Unlock stackInserterUnlock = ModUtils.GetUnlockByName(UnlockNames.StackInserter);
  if (stackInserterUnlock != null) {
    ModUtils.UpdateUnlockSprite("Name of my new Unlock", stackInserterUnlock.sprite);
    ModUtils.UpdateUnlockTreePosition("Name of my new Unlock", stackInserterUnlock.treePosition);
  }
}
```

Just make sure that your new unlock requires a tier above the one you referenced in the function, or they'll overlap.
More ```UpdateUnlock...``` functions will be added in the future as required.
