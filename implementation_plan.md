# Implementation Plan - Phase 1: Foundation

[Overview]
Phase 1 focuses on fixing critical technical debt and establishing core systems for stable gameplay foundation in Moonlit Garden.

This implementation addresses 6 critical technical debt items (T1-T6, T10) and implements 5 core systems (#14, #16, #17, #29, #12) as defined in GAME_FEATURES.md. The scope includes: (1) Optimizing TileMapManager with Dictionary lookup instead of O(n) linear search, (2) Implementing granular Firebase updates instead of full-document overwrite, (3) Fixing race condition with async scene loading, (4) Normalizing player diagonal movement, (5) Redesigning InvenItems with quantity and stacking support, (6) Adding null safety checks, (7) Creating Camera follow system, (8) Building async loading manager with progress bar, (9) Implementing Firebase retry pattern with exponential backoff, and (10) Creating ItemDefinition ScriptableObject architecture.

[Types]
This section defines all new data structures, enums, interfaces, and ScriptableObjects required for Phase 1.

## New Types

### 1. ItemDefinition (ScriptableObject)
File: `Assets/Scripts/Data/ItemDefinition.cs`

```csharp
public enum ItemType
{
    Seed,
    Crop,
    Tool,
    Consumable,
    Material
}

[CreateAssetMenu(fileName = "NewItem", menuName = "MoonlitGarden/Items/ItemDefinition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;           // Unique identifier: "seed_tomato_001"
    public string itemName;         // Display name: "Tomato Seed"
    [TextArea]
    public string description;      // Item description
    
    [Header("Type")]
    public ItemType itemType;       // Category
    public Sprite icon;             // UI icon
    
    [Header("Stacking")]
    public bool stackable;          // Can stack in inventory
    public int maxStack = 99;       // Max stack size
    
    [Header("Economy")]
    public int buyPrice;            // Shop buy price
    public int sellPrice;           // Shop sell price
    
    [Header("Crop Specific")]
    public string cropId;           // If Seed type, links to CropDefinition
    public int yieldMin;            // Min harvest yield
    public int yieldMax;            // Max harvest yield
}
```

### 2. ItemStack
File: `Assets/Scripts/Inventory/ItemStack.cs`

```csharp
[System.Serializable]
public class ItemStack
{
    public string itemId;
    public int quantity;
    
    public ItemStack(string itemId, int quantity = 1)
    {
        this.itemId = itemId;
        this.quantity = quantity;
    }
    
    public bool CanStack(int additionalQty, int maxStack)
    {
        return quantity + additionalQty <= maxStack;
    }
    
    public int GetSpaceRemaining(int maxStack)
    {
        return maxStack - quantity;
    }
}
```

### 3. TileCoordinate (Struct)
File: `Assets/Scripts/Farm/TileCoordinate.cs`

```csharp
[System.Serializable]
public struct TileCoordinate
{
    public int x;
    public int y;
    
    public TileCoordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    
    public override string ToString() => $"{x}_{y}";
    
    public static implicit operator Vector3Int(TileCoordinate tc) 
        => new Vector3Int(tc.x, tc.y, 0);
}
```

### 4. TilemapDetail Extended
File: `Assets/Scripts/Entities/TilemapDetail.cs` (Modified)

Add fields for crop state persistence:
```csharp
public class TilemapDetail 
{
    public int x { get; set; }
    public int y { get; set; }
    public TilemapState tilemapState { get; set; }
    
    // New fields for Phase 1
    public string cropId { get; set; }           // null if no crop
    public long plantedAt { get; set; }          // Unix timestamp
    public int growthStage { get; set; }         // 0=Seed, 1=Sprout, etc.
    public long lastWateredAt { get; set; }      // Unix timestamp
    
    // Constructor with defaults
    public TilemapDetail(int x, int y, TilemapState state)
    {
        this.x = x;
        this.y = y;
        this.tilemapState = state;
        this.cropId = null;
        this.plantedAt = 0;
        this.growthStage = 0;
        this.lastWateredAt = 0;
    }
}
```

### 5. InvenItems Redesigned
File: `Assets/Scripts/Entities/InvenItems.cs` (Modified)

```csharp
public class InvenItems 
{
    public string itemId { get; set; }        // Links to ItemDefinition
    public string name { get; set; }          // Display name (cached)
    public string description { get; set; }   // Description (cached)
    public int quantity { get; set; }         // Stack quantity
    public string itemType { get; set; }      // "Seed", "Crop", etc.
    public string iconName { get; set; }      // Sprite name
    
    // Constructors
    public InvenItems() { quantity = 1; }
    
    public InvenItems(string itemId, string name, string description, int quantity = 1)
    {
        this.itemId = itemId;
        this.name = name;
        this.description = description;
        this.quantity = quantity;
    }
}
```

### 6. FirebaseResponse (Class)
File: `Assets/Scripts/Firebase/FirebaseResponse.cs`

```csharp
public class FirebaseResponse<T>
{
    public bool Success { get; private set; }
    public T Data { get; private set; }
    public string ErrorMessage { get; private set; }
    public Exception Exception { get; private set; }
    
    public static FirebaseResponse<T> Ok(T data) 
        => new FirebaseResponse<T> { Success = true, Data = data };
    
    public static FirebaseResponse<T> Fail(string message, Exception ex = null) 
        => new FirebaseResponse<T> { Success = false, ErrorMessage = message, Exception = ex };
}
```

### 7. LoadProgress (Struct)
File: `Assets/Scripts/Core/LoadProgress.cs`

```csharp
public struct LoadProgress
{
    public float normalized;      // 0.0 to 1.0
    public string statusMessage;  // "Loading map...", "Fetching user data..."
    public bool isComplete;       // true when done
    
    public LoadProgress(float normalized, string status)
    {
        this.normalized = Mathf.Clamp01(normalized);
        this.statusMessage = status;
        this.isComplete = normalized >= 1.0f;
    }
}
```

[Files]
This section details all file modifications, creations, and deletions.

## New Files to Create

| File Path | Purpose |
|-----------|---------|
| `Assets/Scripts/Core/GameManager.cs` | Singleton GameManager for global state |
| `Assets/Scripts/Core/AsyncLoadingManager.cs` | Async scene loading with progress bar |
| `Assets/Scripts/Core/LoadProgress.cs` | Progress tracking struct |
| `Assets/Scripts/Data/ItemDefinition.cs` | ScriptableObject for item definitions |
| `Assets/Scripts/Inventory/ItemStack.cs` | Stack data structure |
| `Assets/Scripts/Inventory/InventoryManager.cs` | Refactored inventory system |
| `Assets/Scripts/Firebase/FirebaseTransactionManager.cs` | Retry pattern + transactions |
| `Assets/Scripts/Firebase/FirebaseResponse.cs` | Response wrapper class |
| `Assets/Scripts/Player/CameraFollow.cs` | 2D camera follow system |
| `Assets/Scripts/Farm/TileCoordinate.cs` | Tile coordinate struct |
| `Assets/Scenes/AsyncLoadingScene.unity` | New loading scene |

## Existing Files to Modify

| File | Changes |
|------|---------|
| `Assets/Scripts/Entities/InvenItems.cs` | Add quantity, itemId, itemType, iconName fields |
| `Assets/Scripts/Entities/TilemapDetail.cs` | Add cropId, plantedAt, growthStage, lastWateredAt |
| `Assets/Scripts/TileMapManager.cs` | Replace O(n) lookup with Dictionary, granular Firebase paths |
| `Assets/Scripts/PlayerMovement.cs` | Normalize diagonal movement |
| `Assets/Scripts/FakeLoading.cs` | Replace with AsyncLoadingManager reference |
| `Assets/Scripts/LoadDataManager.cs` | Add async completion callback, null checks |
| `Assets/Scripts/FirebaseDatabaseManager.cs` | Add retry logic, error callbacks |
| `Assets/Scripts/RecyclableScrollView/RecyclableInventoryManager.cs` | Integrate stacking, remove dummy data |
| `Assets/Scripts/PlayerMovement_Mouse.cs` | Remove debug logs |

## Configuration Updates

| File | Update |
|------|--------|
| `ProjectSettings/TagManager.asset` | Add "Player" tag if not exists |
| `ProjectSettings/Physics2DSettings.asset` | Configure collision layers |

[Functions]
This section details all function modifications and new functions.

## New Functions

### AsyncLoadingManager.cs
| Function | Signature | Purpose |
|----------|-----------|---------|
| LoadSceneAsync | `public void LoadSceneAsync(string sceneName)` | Start async load |
| UpdateProgress | `private void UpdateProgress(float value, string message)` | Update UI |
| WaitForData | `private IEnumerator WaitForData()` | Wait for Firebase |

### FirebaseTransactionManager.cs
| Function | Signature | Purpose |
|----------|-----------|---------|
| WriteWithRetry | `public Task<FirebaseResponse<bool>> WriteWithRetry(string path, string data, int maxRetries = 3)` | Retry write |
| ReadWithRetry | `public Task<FirebaseResponse<T>> ReadWithRetry<T>(string path, int maxRetries = 3)` | Retry read |
| RunTransaction | `public Task<FirebaseResponse<bool>> RunTransaction(string path, Func<string, string> updateFunc)` | Atomic update |

### CameraFollow.cs
| Function | Signature | Purpose |
|----------|-----------|---------|
| Follow | `private void LateUpdate()` | Smooth follow player |
| ClampToBounds | `private Vector3 ClampToBounds(Vector3 pos)` | Keep in bounds |

### TileMapManager.cs
| Function | Signature | Purpose |
|----------|-----------|---------|
| InitializeTileCache | `private void InitializeTileCache()` | Build Dictionary cache |
| GetTileAt | `public TilemapDetail GetTileAt(int x, int y)` | O(1) lookup |
| SetTileStateAsync | `public Task SetTileStateAsync(int x, int y, TilemapState state)` | Async granular update |

## Modified Functions

### PlayerMovement.cs
| Function | Current | Modified |
|----------|---------|----------|
| Update | `movement.x = Input.GetAxisRaw("Horizontal");` | Add `movement.normalized` for diagonal |

### LoadDataManager.cs
| Function | Current | Modified |
|----------|---------|----------|
| GetUserInGame | Returns void, no callback | Add `Action<bool> onComplete` callback |

### FirebaseDatabaseManager.cs
| Function | Current | Modified |
|----------|---------|----------|
| WriteDatabase | Debug.Log only | Add `Action<bool, string> onComplete` callback |

[Classes]
This section details class modifications.

## New Classes

| Class | File | Inheritance | Key Methods |
|-------|------|-------------|-------------|
| GameManager | Core/GameManager.cs | MonoBehaviour (Singleton) | Instance, Initialize |
| AsyncLoadingManager | Core/AsyncLoadingManager.cs | MonoBehaviour | LoadSceneAsync, GetProgress |
| CameraFollow | Player/CameraFollow.cs | MonoBehaviour | LateUpdate, SetBounds |
| FirebaseTransactionManager | Firebase/FirebaseTransactionManager.cs | MonoBehaviour | WriteWithRetry, RunTransaction |
| InventoryManager | Inventory/InventoryManager.cs | MonoBehaviour, IRecyclableScrollRectDataSource | AddItem, RemoveItem, StackItem |

## Modified Classes

| Class | File | Changes |
|-------|------|---------|
| TileMapManager | Farm/TileMapManager.cs | Add Dictionary cache, async methods |
| InvenItems | Entities/InvenItems.cs | Add quantity, itemId fields |
| TilemapDetail | Entities/TilemapDetail.cs | Add crop persistence fields |
| LoadDataManager | LoadDataManager.cs | Add async callback support |

[Dependencies]
No new Unity packages required for Phase 1. All functionality uses existing packages:
- com.unity.2d.tilemap (existing)
- com.unity.nuget.newtonsoft-json (existing)
- Firebase SDK (existing)

Optional: Cinemachine can be added for CameraFollow if preferred over custom implementation.

[Testing]
Phase 1 testing focuses on validation of core systems and regression prevention.

## Test Files to Create

| File | Tests |
|------|-------|
| `Assets/Tests/ItemStackTests.cs` | Stack merge, split, quantity limits |
| `Assets/Tests/TileCoordinateTests.cs` | Coordinate conversion, hashing |
| `Assets/Tests/FirebaseRetryTests.cs` | Retry logic, timeout handling |

## Manual Testing Checklist

- [ ] Player moves at consistent speed in all 8 directions
- [ ] Camera smoothly follows player without jitter
- [ ] Loading scene shows progress bar during Firebase load
- [ ] Inventory correctly stacks identical items
- [ ] Tile changes persist after scene reload
- [ ] No crashes when Firebase is unavailable (error handling)
- [ ] Map loads correctly for new user (null MapInGame case)

[Implementation Order]
Follow this sequence to minimize conflicts and ensure each step builds on stable foundation.

## Phase 1 Implementation Sequence

1. **Data Models First** (no dependencies)
   - Create ItemDefinition.cs ScriptableObject
   - Modify InvenItems.cs (add quantity, itemId)
   - Modify TilemapDetail.cs (add crop fields)
   - Create ItemStack.cs
   - Create TileCoordinate.cs

2. **Firebase Infrastructure** (required by TileMapManager)
   - Create FirebaseResponse.cs
   - Create FirebaseTransactionManager.cs with retry logic
   - Modify FirebaseDatabaseManager.cs to use callbacks

3. **TileMapManager Optimization** (depends on Firebase)
   - Add Dictionary<(int,int), TilemapDetail> cache
   - Implement GetTileAt() O(1) lookup
   - Implement SetTileStateAsync() granular update
   - Add null checks for MapInGame

4. **Player Systems** (independent)
   - Fix PlayerMovement.cs diagonal normalization
   - Create CameraFollow.cs
   - Remove debug logs from PlayerMovement_Mouse.cs

5. **Loading System** (depends on Firebase)
   - Create AsyncLoadingScene.unity with UI
   - Create AsyncLoadingManager.cs
   - Create LoadProgress.cs
   - Modify LoadDataManager.cs for async callback
   - Update FakeLoading.cs or replace scene reference

6. **Inventory System** (depends on Data Models)
   - Modify RecyclableInventoryManager.cs for stacking
   - Remove dummy data generation
   - Fix inventory toggle (use SetActive instead of Y position hack)

7. **Integration Testing**
   - Test full flow: Login → Load → Farm → Save → Reload
   - Verify no race conditions
   - Test error scenarios (network loss)