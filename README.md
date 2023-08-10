# TTMod-EquinoxsModUtils
A collection of utilities for modding the game Techtonica that make it easier to interact with Resources, Schematics and Unlocks

## Objects & Variables

```ModUtils.ResourceNames``` and ```ModUtils.UnlockNames``` are two static classes that contain const strings of the names of Resources and Unlocks. These have been extracted from the game, so they are a perfect match. Using these allows you to avoid typos, which is especially important when using functions like ```GetResourceInfoByName(string name)```.

* ```public static bool shouldLog``` sets whether \[EMU\] messages should be logged. **Note:** other mods may overwrite your value for this, depending on load order.  
* ```public static bool hasGameStateLoaded``` set to true once ```GameState.instance``` has loaded. Useful for checking if data you need has loaded yet.  
* ```public static bool hasGameDefinesLoaded``` set to true once ```GameDefines.instance``` has loaded. Useful for checking if data you need has loaded yet.  
* ```public static bool hasSaveStateLoaded``` set to true once ```SaveState.instance``` has loaded. Useful for checking if data you need has loaded yet.  
* ```public static bool hasTechTreeStateLoaded``` set to true once ```TechTreeState.instance``` has loaded. Useful for checking if data you need has loaded yet.  

## Functions

### Resources

```public static ResourceInfo GetResourceInfoByName(string name)```

This function allows you to find the ResourceInfo of any resource by using it's display name.
Example use:

```csharp
ResourceInfo info = ModUtils.GetResourceInfoByName(ModUtils.ResourceNames.IronFrame);
if (info != null) {
  info.ingredient1Quantity = 2;
}
```

```public static int GetResourceIDByName(string name)```

This function finds the unique resource ID for the given resource name.
Example use:

```csharp
int limestoneID = ModUtils.GetResourceIDByName(ModUtils.ResourceNames.Limestone);
if (limestoneID != -1) {
  chestInstance.GetInventory(0).AddResourcesToSlot(limestoneID, slotNum, out remainingCount, quantity);
}
```
### Schematics

```public static SchematicsRecipeData FindThresherRecipeFromOutputs(string output1Name, string output2Name)```

This function finds a thresher recipe based on it's two outputs. This does not work for recipes with one output.
Example use:

```csharp
SchematicsRecipeData recipe = ModUtils.FindThresherRecipeFromOutputs(ModUtils.ResourceNames.KindlevineSeed, ModUtils.ResourceNames.KindlevineStems);
if (recipe != null) {
  recipe.outputQuantities[0] = 2;
}
```
### Unlocks (Techs)

```public static Unlock GetUnlockByName(string name)```

This function finds the Unlock that matches the name given in the argument. Returns null if not found.
Example use:

```csharp
Unlock stackInserterUnlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.StackInserter);
if (stackInserterUnlock != null) {
  Sprite stackInserterSprite = stackInserterUnlock.sprite;
}
```
```public static void addNewUnlock(NewUnlockDetails details)```

This function takes basic information about a new Unlock / Tech and registers it to be added at the correct times during the load cycle. Handles all the heavy lifting for you.
See the end section of this readme for a guide on adding new Unlocks.

```public static void UpdateUnlockSprite(int unlockID, Sprite sprite)```

This function is used to change the sprite of an unlock. Useful for if you want your new Unlock to use the same sprite as a vanilla Unlock.
Example use:

**Note:** Currently there isn't a way to get the unique ID of new Unlocks. This will be added in a future update.

```csharp
Unlock conveyor2Unlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.ConveyorBeltMKII);
if (conveyor2Unlock != null) {
  ModUtils.UpdateUnlockSprite(myCustomUnlockID, conveyor2Unlock.sprite);
}
```

```public static void UpdateUnlockSprite(string displayName, Sprite sprite)```

This function does the same as above, but takes the ```displayName``` of the Unlock to find the Unlock to update.
Exmaple use:

```csharp
Unlock conveyor2Unlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.ConveyorBeltMKII);
if (conveyor2Unlock != null) {
  ModUtils.UpdateUnlockSprite("My Custom Unlock", conveyor2Unlock.sprite);
}
```

```public static void UpdateUnlockTreePosition(int unlockID, int treePosition)```

This function updates the horizontal position of the Unlock on the Tech Tree. Useful for aligning new Unlocks with vanilla ones.
Example use:

**Note:** Currently there isn't a way to get the unique ID of new Unlocks. This will be added in a future update.

```csharp
Unlock conveyor2Unlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.ConveyorBeltMKII);
if (conveyor2Unlock != null) {
  ModUtils.UpdateUnlockTreePosition(myCustomUnlockID, conveyor2Unlock.treePosition);
}
```

```public static void UpdateUnlockTreePosition(string displayName, int treePosition)```

This function does the same as above, but takes the ```displayName``` of the Unlock to find the Unlock to update.
Example use:

```csharp
Unlock conveyor2Unlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.ConveyorBeltMKII);
if (conveyor2Unlock != null) {
  ModUtils.UpdateUnlockTreePosition("My Custom Unlock", conveyor2Unlock.treePosition);
}
```

## Events

```public static event EventHandler GameStateLoaded;```

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

```public static event EventHandler GameDefinesLoaded;```

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

```public static event EventHandler SaveStateLoaded;```

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

```public static event EventHandler TechTreeStateLoaded;```

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

Next, call ```ModUtils.addNewTech(NewUnlockDetails details)```, passing in your ```NewUnlockDetails```.

```csharp
void Awake() {
  NewUnlockDetails details = new NewUnlockDetails(){...};
  ModUtils.AddNewUnlock(details);
}
```

This function registers the new Unlock to be added to the game at the correct time using the events above. If you used your own Sprite, you're done now. EMU will handle adding the Unlock and tracking its UnlockState from here.
You can use the ```UpdateUnlock...``` functions to alter the new ```Unlock``` once GameDefines has loaded.

To assign a vanilla sprite to the new unlock, we need to find the sprite once GameDefines has loaded. Create an event called ```OnGameDefinesLoaded``` and bind it to ModUtils.GameDefinesLoaded.
In the example below, we'll use the sprite of the Stack Inserter Unlock.

```csharp
void Awake() {
  NewUnlockDetails details = new NewUnlockDetails(){...};
  ModUtils.AddNewUnlock(details);

  ModUtils.GameDefinesLoaded += OnGameDefinesLoaded;
}

private void OnGameDefinesLoaded(object sender, EventArgs e) {
  Unlock stackInserterUnlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.StackInserter);
  if (stackInserterUnlock != null) {
    ModUtils.UpdateUnlockSprite("Name of my new Unlock", stackInserterUnlock.sprite);
  }
}
```
You can also use this event to horizontally align your new Unlock with vanilla ones on the TechTree:

```csharp
private void OnGameDefinesLoaded(object sender, EventArgs e) {
  Unlock stackInserterUnlock = ModUtils.GetUnlockByName(ModUtils.UnlockNames.StackInserter);
  if (stackInserterUnlock != null) {
    ModUtils.UpdateUnlockSprite("Name of my new Unlock", stackInserterUnlock.sprite);
    ModUtils.UpdateUnlockTreePosition("Name of my new Unlock", stackInserterUnlock.treePosition);
  }
}
```

Just make sure that your new unlock requires a tier above the one you referenced in the function, or they'll overlap.
More UpdateUnlock... functions will be added in the future as required.
