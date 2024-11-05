# Contents

# Equinox's Mod Utils (EMU)
A collection of utilities for modding the game Techtonica that make it easier to interact with Resources, Schematics and Unlocks. EMU and it's child branches [EMUAdditions](https://thunderstore.io/c/techtonica/p/Equinox/EMUAdditions/) and [EMUBuilder]() are a replacement for a modding API.

**Note:** As of V6.0.0, functions for adding content to the game has been moved to [EMUAdditions](https://thunderstore.io/c/techtonica/p/Equinox/EMUAdditions/) and functions for building machines have been moved to [EMUBuilder]()

## How To Use EMU

1 - Download and install EMU from Thunderstore like any other mod.
1 - In Visual Studio 22, in the Solution Explorer, right click 'References'
1 - Click 'Add Reference'
1 - Navigate to 'Techtonica/BepInEx/plugins/EquinoxsModUtils/EquinoxsModUtils.dll'

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

**Note:** Don't use `EMU.Events.GameSaved` to call functions from `EMUAdditions.CustomData`, see [EMUAdditions Documentation]() for more info.

- `EMU.Events.GameStateLoaded` - Fires when `EMU.LoadingStates.hasGameStateLoaded` is set to `true`
- `EMU.Events.GameDefinesLoaded` - Fires when `EMU.LoadingStates.hasGameDefinesLoaded` is set to `true`
- `EMU.Events.MachineManagerLoaded` - Fires when `EMU.LoadingStates.hasMachineManagerLoaded` is set to `true`
- `EMU.Events.SaveStateLoaded` - Fires when `EMU.LoadingStates.hasSaveStateLoaded` is set to `true`
- `EMU.Events.TechTreeStateLoaded` - Fires whe `EMU.LoadingStates.hasTechTreeStateLoaded` is set to `true`
- `EMU.Events.GameSaved` - Fires whenever the game is saved. 
- `EMU.Events.GameLoaded` - Fires when `EMU.LoadingStates.hasGameLoaded` is set to `true`
- `EMU.Events.GameUnloaded` - Fires when the player loads a save while in-game

## Resources

EMU provides serveral functions that help you work with resources:

##### GetResourceInfoByName

```csharp
public static ResourceInfo GetResourceInfoByName (
  string name,
  bool shouldLog = false
)
```

This function allows you to find the ResourceInfo of any item by using its display name. Returns `null` if not found. `shouldLog` controls whether info messages should be logged for this call.

Example use:

```csharp
ResourceInfo ironFrame = EMU.Resources.GetResourceInfoByName(EMU.Names.Resources.IronFrame);
Player.instance.inventory.AddResources(ironFrame, 1);
```

##### GetResourceInfoByNameUnsafe

```csharp
public static ResourceInfo GetResourceInfoByNameUnsafe(
  string name,
  bool shouldLog = false
)
```

Does the same as [GetResourceInfoByName](#getresourceinfobyname), but won't check `EMU.LoadingStates.hasGameDefinesLoaded` first. Only ever use this in patches of `GameDefines` functions.

##### GetResourceIDByName

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