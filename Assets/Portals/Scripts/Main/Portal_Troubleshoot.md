### Portal Troubleshooting Guide

## Issue 1: "Scene ... couldn't be loaded because it has not been added to the build settings or the AssetBundle has not been loaded"
- Sometimes when you move scenes to different folders, the build settings do not update.
- This causes the error: "Scene ... couldn't be loaded because it has not been added to the build settings or the AssetBundle has not been loaded"
- FIX: Clear the build settings, then drag the scenes back into the build settings.


## Issue 2: "Could not find destination portal with id = ... in scene ..."
- This error occurs when the game cannot find a portal in the destination scene with the specified id
- FIX 1: Add a new GameObject to the destination scene, and attach a PortalComponent. Assign the "id" variable of the PortalComponent to the specified id.