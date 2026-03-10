# Vietnamese Farmer Character Setup

## Overview
This tool set creates a complete Animator Controller for the Vietnamese Farmer character with:
- **7 Actions**: Idle, Walk, PickUp, Dig, Plant, Water, Harvest
- **8 Directions**: South, South-East, East, North-East, North, North-West, West, South-West
- **56 Total Animation Clips** (7 actions × 8 directions)

## Files Created
1. `Assets/Editor/VietnameseFarmerSetup.cs` - Main setup editor script
2. `Assets/Animations/VietnameseFarmer/` - Folder containing all animation clips
3. `Assets/Animations/VietnameseFarmer/VietnameseFarmerController.controller` - Animator Controller

## How to Use

### Step 1: Open Unity Editor
Make sure Unity is open and the project is loaded.

### Step 2: Run the Setup Tool
In Unity Editor menu bar:
```
Tools > Vietnamese Farmer > Complete Setup
```

This will:
1. Create the `Assets/Animations/VietnameseFarmer/` folder
2. Generate all 56 animation clips from the sprite sheets
3. Create the Animator Controller with proper states and transitions
4. Apply the controller to the Player GameObject in the scene

### Step 3: Verify Setup
After setup completes:
1. Select the Player GameObject in the Hierarchy
2. Check that the Animator component has the `VietnameseFarmerController` assigned
3. Open the Animator window (Window > Animation > Animator) to see the state machine

### Step 4: Test in Play Mode
1. Enter Play Mode
2. Use WASD or Arrow Keys to move the player
3. The character should animate based on movement direction

## Using Action Animations

The PlayerMovement script now includes methods to trigger action animations:

```csharp
// Get the PlayerMovement component
PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();

// Trigger action animations
playerMovement.DoPickUp();   // Pick up item
playerMovement.DoDig();      // Dig soil
playerMovement.DoPlant();    // Plant seeds
playerMovement.DoWater();    // Water crops
playerMovement.DoHarvest();  // Harvest crops
```

## Animator Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Horizontal | Float | Horizontal movement (-1 to 1) |
| Vertical | Float | Vertical movement (-1 to 1) |
| Speed | Float | Movement speed (0 when idle) |
| PickUp | Trigger | Trigger pick up animation |
| Dig | Trigger | Trigger dig animation |
| Plant | Trigger | Trigger plant animation |
| Water | Trigger | Trigger water animation |
| Harvest | Trigger | Trigger harvest animation |

## Animation Clip Naming Convention

Clips are named: `{Action}_{Direction}`

Examples:
- `Idle_south` - Idle animation facing south
- `Walk_north-east` - Walking animation facing north-east
- `Dig_west` - Dig animation facing west
- `Harvest_south-east` - Harvest animation facing south-east

## Troubleshooting

### Animation clips not created
- Check that the character sprite folder exists at `Assets/A_Vietnamese_farmer_from_the_countryside_standing/`
- Verify that animation subfolders contain PNG files

### Player not animating
- Ensure the Player GameObject has:
  - Animator component
  - SpriteRenderer component
  - PlayerMovement script with animator reference set
- Check that the Animator Controller is assigned

### Wrong animation playing
- Check the Animator window for transition conditions
- Verify Horizontal/Vertical parameters are being set correctly

## Rebuilding

If you need to rebuild specific parts:

```
Tools > Vietnamese Farmer > Rebuild Animation Clips Only
Tools > Vietnamese Farmer > Rebuild Controller Only
```

## Source Asset Structure

The tool expects the following folder structure:
```
Assets/A_Vietnamese_farmer_from_the_countryside_standing/
├── animations/
│   ├── breathing-idle/
│   │   ├── south/
│   │   ├── south-east/
│   │   ├── east/
│   │   ├── north-east/
│   │   ├── north/
│   │   ├── north-west/
│   │   ├── west/
│   │   └── south-west/
│   ├── walking-6-frames/
│   │   └── [same 8 direction folders]
│   ├── picking-up/
│   │   └── [same 8 direction folders]
│   └── [other actions...]
└── rotations/
    └── [direction sprites]
```

## Support

For issues or questions, check the Unity Console for error messages during setup.