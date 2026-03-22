# MOONLIT GARDEN (Field) — Tai Lieu Tinh Nang / Feature Document

> **Ngay cap nhat**: 2026-03-11
> **Trang thai**: Prototype giai doan dau (Early Alpha)
> **Tong ket nhanh**: 7 ✅ Hoan thanh | 19 🔨 Dang lam | 2 ❌ Chua bat dau (trong 28 tinh nang goc)
> **Gameplay Loop**: 85% — Tat ca break points da sua; character animation + starter seeds + quickbar highlight hoan chinh
> **Break Points**: 0 diem gay con lai — tat ca BP1/2/3/4/5/6/7 da sua xong

---

## 1. Tong Quan Du An / Project Overview

| Muc | Chi tiet |
|-----|---------|
| **Ten du an** | Moonlit Garden (codename: Field) |
| **Engine** | Unity 6 — URP 2D (Universal Render Pipeline) |
| **Backend** | Firebase Authentication + Realtime Database |
| **Serialization** | Newtonsoft JSON (com.unity.nuget.newtonsoft-json 3.2.2) |
| **Art Pack** | Tiny RPG Forest (hero, 2 enemies, environment tiles, UI sprites) |
| **Font** | Cherry Bomb One Regular |
| **Platform** | PC (chua co mobile build) |

### Scene Flow

```
LoginScene → FakeLoading → PlayScene
   (Auth)     (Transition)    (Gameplay)
```

### Cau Truc Script (42+ files)

```
Assets/Scripts/
├── Core/                              # [Phase 1+2] Core systems
│   ├── AsyncLoadingManager.cs         # Async scene loading + progress tracking
│   ├── AudioManager.cs               # BGM + SFX + time-of-day music (#30)
│   ├── DayNightController.cs          # Light2D day-night cycle driver
│   ├── GameManager.cs                 # Global game state management
│   ├── GameTimeManager.cs             # In-game clock + time events
│   └── LoadProgress.cs                # Progress data struct
├── Data/                              # [Phase 1+2] ScriptableObject definitions
│   ├── CropDefinition.cs             # Crop properties + growth stages
│   ├── ItemDefinition.cs             # Item properties + types
│   └── ToolDefinition.cs             # Tool properties + types (#26)
├── Economy/                           # [Phase 2] Economy system
│   ├── PlayerEconomyManager.cs        # Gold/Diamond earn/spend + Firebase sync (#9)
│   ├── ShopCatalog.cs                 # ShopCatalog ScriptableObject + ShopEntry (#21)
│   └── ShopManager.cs                 # Buy/sell transaction logic (#21)
├── Entities/                          # Data models
│   ├── InvenItems.cs                  # Item: itemId, name, quantity, type
│   ├── Map.cs                         # Map: List<TilemapDetail>
│   ├── TilemapDetail.cs               # Tile: x, y, state + crop persistence
│   └── User.cs                        # User: Name, Gold, Diamond, MapInGame, Version (#22)
├── Farm/                              # [Phase 2] Farming system
│   ├── CropGrowthManager.cs          # Plant growth state machine
│   └── TileCoordinate.cs             # Tile coordinate struct + conversions
├── Firebase/                          # [Phase 1] Firebase utilities
│   ├── FirebaseTransactionManager.cs  # Retry + atomic transactions + versioned save (#22)
│   └── FirebaseResponse.cs           # Generic response wrapper
├── Inventory/                         # [Phase 1] Inventory logic
│   └── ItemStack.cs                   # Stack/merge logic
├── Player/                            # [Phase 1+2] Player systems
│   ├── CameraFollow.cs               # Smooth follow + boundary + zoom
│   └── EquipmentManager.cs           # Tool equip/unequip + tool switching (#26)
├── UI/                                # [Phase 2] UI framework
│   ├── EconomyHUD.cs                  # Gold/Diamond display (#9)
│   ├── InventoryActionPanel.cs        # Context actions: Use/Equip/Drop/Split (#28)
│   ├── ItemTooltip.cs                 # Item tooltip popup on hover/click (#28)
│   ├── NotificationManager.cs         # Toast notification system
│   ├── PanelBase.cs                   # Base class for all UI panels
│   ├── QuickbarSlot.cs                # Single quickbar slot (icon+key+highlight) (#27)
│   ├── QuickbarUI.cs                  # Horizontal quickbar toolbar (#27)
│   ├── ShopPanel.cs                   # Shop buy/sell UI panel (#23)
│   ├── TimeHUD.cs                     # In-game clock display (TMP)
│   ├── ToolHUD.cs                     # Current tool display (#26)
│   └── UIManager.cs                   # Singleton panel stack manager
├── RecyclableScrollView/              # Inventory UI
│   ├── CelllItemData.cs               # Cell renderer + click/hover interactions (#28)
│   └── RecyclableInventoryManager.cs  # Scroll list + AddItem + stacking
├── Wizards/
│   └── UsernameWizard.cs              # First-login username popup
├── FakeLoading.cs                     # Button → LoadScene("PlayScene")
├── FirebaseDatabaseManager.cs         # Write/Read Firebase DB
├── FirebaseLoginManager.cs            # Register + Sign-in
├── LoadDataManager.cs                 # Firebase user data + real-time listener (#10)
├── PlayerFarmController.cs            # C/V/B/M keys → farm actions (refactored)
├── PlayerMovement.cs                  # WASD movement (diagonal normalized)
├── PlayerMovement_Mouse.cs            # Click-to-move
└── TileMapManager.cs                  # Tilemap ↔ Firebase sync (Dict cache + granular)

Assets/Data/Crops/                     # [Phase 2] CropDefinition ScriptableObject assets
├── crop_tomato.asset                  # Regrowable, yield 2-5, sell 35g, stageSprites[5] wired
└── crop_wheat.asset                   # One-time, yield 3-6, sell 25g, stageSprites[5] wired

Assets/Data/Tools/                     # [Phase 2] ToolDefinition ScriptableObject assets
├── BasicHoe.asset                     # Hoe, tier 1, buy 50g
├── BasicWateringCan.asset             # WateringCan, tier 1, buy 50g
├── BasicSickle.asset                  # Sickle, tier 1, buy 50g
└── TomatoSeedBag.asset                # SeedBag, cropId=crop_tomato, buy 10g

Assets/Data/Items/                     # [Phase 2.5] ItemDefinition ScriptableObject assets
├── seed_tomato.asset                  # Seed, buy 20g, sell 5g, cropId=tomato
└── seed_wheat.asset                   # Seed, buy 15g, sell 3g, cropId=wheat

Assets/Data/Shop/                      # [Phase 2.5] ShopCatalog ScriptableObject assets
└── GeneralStore.asset                 # "Cua Hang Tap Hoa", 2 entries: seed_tomato + seed_wheat

Assets/Sprites/Crops/                  # [Phase 2.5] PixelLab-generated crop sprites (32x32, Point filter)
├── Tomato/tomato_0_seed.png           # Stage 0 — Seed
├── Tomato/tomato_1_sprout.png         # Stage 1 — Sprout
├── Tomato/tomato_2_growing.png        # Stage 2 — Growing
├── Tomato/tomato_3_mature.png         # Stage 3 — Mature
├── Tomato/tomato_4_harvestable.png    # Stage 4 — Harvestable (red tomatoes)
├── Wheat/wheat_0_seed.png             # Stage 0 — Seed
├── Wheat/wheat_1_sprout.png           # Stage 1 — Sprout
├── Wheat/wheat_2_growing.png          # Stage 2 — Growing
├── Wheat/wheat_3_mature.png           # Stage 3 — Mature
└── Wheat/wheat_4_harvestable.png      # Stage 4 — Harvestable (golden wheat)

Assets/Tests/                          # [Phase 1] Unit tests
├── FirebaseRetryTests.cs              # Retry logic + exponential backoff
├── TileCoordinateTests.cs             # Coordinate conversion + equality
└── ItemStackTests.cs                  # Stack merge logic
```

### Assets Co San

- **Sprites**: Hero (idle/walk/attack), Mole, Treant, coins, gems, hearts
- **Environment**: tileset.png (1100+ sliced tiles), trees, bushes, rocks, waterfall
- **Animations**: Player.controller (Idle, WalkUp, WalkDown, WalkLeft, WalkRight)
- **Prefabs**: Canvas.prefab, DemoTilemap.prefab
- **UI**: Login backgrounds, icons (user, password, Google, Facebook)
- **Packages**: URP 17.3, 2D Animation/Tilemap/Sprite, Input System 1.17, Timeline

---

## 2. Bang Tong Hop Tinh Nang / Feature Status Summary

### Ghi chu trang thai
- ✅ **Done** — Hoan thanh, hoat dong duoc
- 🔨 **Partial** — Co code nhung thieu tinh nang hoac co bug
- ❌ **Not Started** — Chua co code nao

### Core Gameplay

| # | Tinh nang | Trang thai | Uu tien |
|---|-----------|:----------:|---------|
| 1 | Player top-down movement (mouse) + animation | 🔨 | High |
| 2 | Tilemap multi-layer + tile interaction | 🔨 | High |
| 3 | Farm actions: till / plant / water / harvest | 🔨 | Critical |
| 4 | Plant growth state machine | 🔨 | Critical |
| 5 | Day-Night cycle (2D Light) | 🔨 | Medium |

### Data & Backend

| # | Tinh nang | Trang thai | Uu tien |
|---|-----------|:----------:|---------|
| 6 | Firebase setup + auth (register/login) | ✅ | — |
| 7 | Realtime DB sync cho map state | 🔨 | Critical |
| 8 | Load/save map theo user | 🔨 | Critical |
| 9 | UserInGame profile (gold, diamond, username) | 🔨 | High |
| 10 | Conflict-safe load (real-time sync) | 🔨 | Medium |

### UI/UX

| # | Tinh nang | Trang thai | Uu tien |
|---|-----------|:----------:|---------|
| 11 | Inventory system (scroll toi uu) | 🔨 | High |
| 12 | Add harvested items + stack | ✅ | High |
| 13 | Custom UI framework (HUD + popup + panel) | 🔨 | High |
| 14 | Loading bar / async scene loading | ✅ | High |

### Production Features

| # | Tinh nang | Trang thai | Uu tien |
|---|-----------|:----------:|---------|
| 15 | Map upload/download pipeline | 🔨 | Medium |
| 16 | Error handling + retry Firebase calls | ✅ | Critical |
| 17 | Data model chuan hoa | ✅ | Critical |
| 18 | Git workflow cho team | ❌ | Medium |

### Vietnamese Art Direction

| # | Tinh nang | Trang thai | Uu tien |
|---|-----------|:----------:|---------|
| 19 | VN Art Direction + Asset list + Style guide | ❌ | Low |
| 20 | Map layout VN (nha–vuon–cho/tiem) | ❌ | Low |

### Shop/Trading System

| # | Tinh nang | Trang thai | Uu tien |
|---|-----------|:----------:|---------|
| 21 | Shop/Trading System | 🔨 | High |
| 22 | Transaction integrity (versioned save) | 🔨 | High |
| 23 | Shop UI (panel + list + confirm) | 🔨 | High |

### Crop System

| # | Tinh nang | Trang thai | Uu tien |
|---|-----------|:----------:|---------|
| 24 | Crop Data Library (CropDefinition) | ✅ | Critical |
| 25 | Growth persistence (plantedAt, stage) | 🔨 | Critical |

### Item/Tool System

| # | Tinh nang | Trang thai | Uu tien |
|---|-----------|:----------:|---------|
| 26 | Item use & tool equip | 🔨 | High |
| 27 | Quickbar + input binding | 🔨 | Medium |
| 28 | Inventory interactions (click/tooltip/split) | 🔨 | Medium |

### Tinh Nang Bo Sung / Additional Features (De xuat moi)

| # | Tinh nang | Trang thai | Uu tien |
|---|-----------|:----------:|---------|
| 29 | Camera follow system (smooth + boundary) | ✅ | Critical |
| 30 | Audio System (BGM + SFX) | 🔨 | High |
| 31 | NPC System + Dialogue | ✅ | Medium |
| 32 | Quest/Mission System | ❌ | Medium |
| 33 | Energy/Stamina System | ✅ | Medium |
| 34 | Settings Menu | ✅ | Low |
| 35 | Tutorial/Onboarding | ✅ | Low |
| 36 | Weather System | ✅ | Low |
| 37 | Seasons System | ❌ | Low |
| 38 | Save Slot Management | ❌ | Low |
| 39 | Localization Framework (VN/EN) | ❌ | Low |
| 40 | Enemy/Combat System | ❌ | Low |
| 41 | Mobile Optimization | ❌ | Low |

---

## 3. Chi Tiet Tung Tinh Nang / Detailed Feature Breakdown

---

### NHOM A: CORE GAMEPLAY

---

#### #1 — Player Top-down Movement + Animation 🔨

**Da co:**
- `PlayerMovement.cs`: Di chuyen WASD voi `Rigidbody2D.MovePosition()`, speed = 5f
- `PlayerMovement_Mouse.cs`: Click-to-move, tu dong dung khi den dich (threshold 0.1f)
- 5 animation clips: Idle, WalkUp, WalkDown, WalkLeft, WalkRight
- Animator controller voi params: `Horizontal`, `Vertical`, `Speed`
- ✅ Diagonal movement da normalize (`.Normalize()` trong `PlayerMovement.cs`)

**Con thieu:**
- Khong co collision layer setup ro rang
- Attack animation co trong asset nhung chua duoc hookup vao Animator
- Debug log tieng Viet con sot trong `PlayerMovement_Mouse.cs` ("Toi toi noi roi")
- Chua chuyen sang New Input System (da cai nhung dung legacy `Input.GetAxisRaw`)

---

#### #2 — Tilemap Multi-layer + Tile Interaction 🔨

**Da co:**
- 3 tilemap layers: `tm_Ground`, `tm_Grass`, `tm_Forest` (trong `PlayerFarmController.cs`)
- 1100+ tile assets da slice tu tileset (trong `DemoTilemap/`)
- `DemoTilemap.prefab` — prefab tilemap mau

**Con thieu:**
- ✅ Tile selection cursor: `TileCursor.cs` — snap to grid, 3-color state (yellow/blue/green), pulse animation — wired under Player in PlayScene
- Player position → cell position khong co offset cho sprite pivot
- Khong co Tile Palette tool cho level design trong Editor
- Chua co sorting layer setup ro rang cho cac tilemap layers

---

#### #3 — Farm Actions: Till / Plant / Water / Harvest 🔨

**Da co:**
- `PlayerFarmController.cs` (Phase 2 refactored):
  - Phim `E`: Use equipped tool (context-sensitive via EquipmentManager)
  - Phim `C`: Till — xoa co (Grass → Ground)
  - Phim `V`: Plant — trong crop qua `CropGrowthManager.PlantCrop()`
  - Phim `F`: Water — tuoi cay qua `CropGrowthManager.WaterCrop()`
  - Phim `M`: Harvest — thu hoach qua `CropGrowthManager.HarvestCrop()` + them vao inventory voi quantity
  - Phim `1-6`: Switch tools (via EquipmentManager)
- ✅ Delegate crop logic sang `CropGrowthManager` (Single Responsibility)
- ✅ Harvest tra ve `InvenItems` voi itemId, quantity, type tu `CropDefinition`
- ✅ Ho tro regrowable crops (crop quay lai stage cu sau harvest)
- Moi thay doi tile → sync Firebase qua `TileMapManager`

**Con thieu:**
- **Seed selection** — hien dung `defaultCropId` hardcode, can UI chon seed tu inventory
- Khong co farming animation/particle effect
- Khong co cooldown giua cac action
- ~~Visual: dung Forest tile lam placeholder, can sprite rieng cho tung growth stage~~ ✅ 10 sprites da tao (PixelLab), wire vao CropDefinition.stageSprites[]; CropGrowthManager dung `RefreshCropVisual()` + SpriteRenderer per tile

---

#### #4 — Plant Growth State Machine 🔨

**Da co (Phase 2):**
- ✅ `GrowthStage` enum: `Seed(0) → Sprout(1) → Growing(2) → Mature(3) → Harvestable(4)`
- ✅ `CropGrowthManager.cs`: Singleton runtime manager, coroutine-based growth loop
- ✅ Time-based growth: `CalculateStageFromElapsed()` tu `CropDefinition`
- ✅ `PlantCrop()` / `WaterCrop()` / `HarvestCrop()` public API
- ✅ `activeCropTiles` HashSet — chi scan tiles co crop (O(crop count), khong O(total tiles))
- ✅ Events: `OnCropStageChanged`, `OnCropHarvestable`
- ✅ Tich hop voi `TilemapDetail` (cropId, plantedAt, growthStage, lastWateredAt)
- ✅ Firebase sync qua `TileMapManager.UpdateCropDataAsync()`

**Con thieu:**
- ~~Visual: chua thay doi sprite theo stage (dung Forest tile placeholder)~~ ✅ `CropGrowthManager.RefreshCropVisual()` goi `cropDef.GetStageSprite(stage)` tai moi PlantCrop, UpdateAllCrops, HarvestCrop
- Wither mechanic chua implement (witherTime field co nhung chua check)
- Can test gameplay loop end-to-end

---

#### #5 — Day-Night Cycle (2D Light) 🔨

**Da co (Phase 2):**
- ✅ `GameTimeManager.cs`: In-game clock — configurable `timeScale` (default 10 min/s = 1 day in ~2.4 min)
- ✅ `TimePeriod` enum: Dawn / Morning / Afternoon / Evening / Night
- ✅ Events: `OnHourChanged`, `OnNewDay`, `OnMinuteChanged`
- ✅ API: `SetTime()`, `SkipToMorning()`, `PauseClock()`, `GetFormattedTime()`
- ✅ `DayNightController.cs`: Drives `Light2D` color + intensity qua 5 pha:
  - Night (0-5h): dark blue, 0.3 intensity
  - Dawn (5-8h): warm orange → white transition
  - Day (8-17h): full white, 1.0 intensity
  - Dusk (17-20h): orange → dark blue transition
  - Night (20-24h): dark blue, 0.3 intensity
- ✅ Smooth `Color.Lerp` + `Mathf.Lerp` transition (khong bi giat)
- ✅ `TimeHUD.cs`: Hien thi time (TMP), day count, period name
- ✅ Wired in PlayScene: DayNightController tren Global Light 2D, TimeHUD tren Canvas

**Con thieu:**
- Anh huong gameplay: chua co crop chi trong ban dem/ngay
- Thieu ambient sound thay doi theo thoi gian
- Time scale chua configurable tu Settings UI

---

### NHOM B: DATA & BACKEND

---

#### #6 — Firebase Setup + Auth ✅

**Da co:**
- `FirebaseLoginManager.cs`: Dang ky + dang nhap email/password
- Tao User moi voi 100 Gold, 50 Diamond khi register
- Chuyen scene sau khi auth thanh cong
- Firebase SDK da cai dat day du (Auth + Realtime Database)

**Cai thien nho:**
- Khong validate input (email/password rong van gui duoc)
- Error message chi hien Debug.Log, khong hien tren UI
- Khong co "Quen mat khau" / "Remember me"
- Khong co social login (Google/Facebook icons co nhung chua implement)

---

#### #7 — Realtime DB Sync cho Map State 🔨

**Da co:**
- `TileMapManager.cs`: Ghi/doc tilemap data tu Firebase
- `WriteAllTileMapFireBase()`: Upload toan bo tile khi tao map moi
- `SetStateForTilemapDetail()`: Cap nhat 1 tile → save Firebase
- ✅ **O(1) Dictionary lookup**: `tileCache Dictionary<(int,int), TilemapDetail>` thay O(n)
- ✅ **Granular Firebase path update**: Chi ghi tile cu the, khong overwrite toan bo document
- ✅ **Async methods**: `SetTileStateAsync()`, `UpdateCropDataAsync()` voi retry support
- ✅ **Null checks**: MapInGame null check day du (T10 fix)

**Con thieu:**
- Khong co realtime listener: Khong dung `ValueChanged`, chi doc 1 lan
- Khong co offline queue: Mat mang = mat data

---

#### #8 — Load/Save Map theo User 🔨

**Da co:**
- `LoadDataManager.cs`: Doc user data tu Firebase, luu vao static fields
- `TileMapManager.LoadMapForUser()`: Render tilemap tu saved state
- Du lieu luu tai path: `Users/{userId}`
- ✅ **Race condition fixed**: `AsyncLoadingManager.cs` doi Firebase data xong moi chuyen scene
- ✅ **Null checks**: MapInGame null check trong TileMapManager
- ✅ **Loading indicator**: `LoadProgress` struct + progress bar UI

**Con thieu:**
- Khong co local cache / backup

---

#### #9 — UserInGame Profile 🔨

**Da co:**
- `User.cs`: `Name`, `Gold` (int), `Diamond` (int), `MapInGame`
- `UsernameWizard.cs`: Dat username lan dau + hien thi Gold/Diamond
- Data luu len Firebase
- ✅ `PlayerEconomyManager.cs` (Phase 2): Singleton Gold/Diamond earn/spend API
  - `EarnGold()` / `SpendGold()` / `CanAffordGold()` — full validation
  - `EarnDiamond()` / `SpendDiamond()` — full validation
  - Events: `OnGoldChanged`, `OnDiamondChanged`, `OnTransactionFailed`
  - Batched Firebase save (2s delay to avoid rapid writes)
  - `FlushSave()` for scene transitions
- ✅ `EconomyHUD.cs`: Live Gold/Diamond display (subscribes to events)
- ✅ Harvest → auto earn Gold (cropDef.sellPrice * quantity)
- ✅ Real-time server sync: subscribes to `LoadDataManager.OnServerDataChanged` (#10)

**Con thieu:**
- Khong co Level / XP / Play time
- Khong co profile picture
- Khong co stats tracking (so cay da trong, so lan thu hoach, v.v.)

---

#### #10 — Conflict-safe Load 🔨

**Da co (Phase 1 + 2):**
- ✅ `FirebaseTransactionManager.cs`: Retry voi exponential backoff
- ✅ `FirebaseResponse<T>`: Generic response wrapper (Ok/Fail pattern)
- ✅ `WriteWithRetry()`: Auto-retry Firebase writes voi configurable maxRetries
- ✅ **Real-time listener** (Phase 2): `LoadDataManager.StartListening()` — Firebase `ValueChanged` event
  - Auto-sync khi server data thay doi (tu device khac hoac admin)
  - `OnServerDataChanged` event cho UI updates
  - Server-wins strategy: local data tu dong cap nhat khi server version > local version
  - `StopListening()` cleanup khi logout/destroy
- ✅ **PlayerEconomyManager** subscribes to `OnServerDataChanged` — Gold/Diamond UI auto-sync
- ✅ `ReloadUserData()`: Manual reload API cho user trigger

**Con thieu:**
- Firebase Security Rules file de chan client tu set Gold
- Offline queue: luu tru changes khi mat mang, sync khi co mang lai
- Conflict resolution UI: hien popup cho user chon "Keep local" vs "Use server"

---

### NHOM C: UI/UX

---

#### #11 — Inventory System 🔨

**Da co:**
- `RecyclableInventoryManager.cs`: Dung RecyclableScrollRect (virtualized, hieu nang tot)
- Toggle hien/an voi phim `B` (dung `SetActive()`)
- `CelllItemData.cs`: Hien thi ten + mo ta + quantity badge + click/hover interactions (#28)
- `AddInventoryItem()`: Them item voi stacking support
- ✅ `InventoryActionPanel.cs`: Context actions (Use/Equip/Drop/Split) (#28)
- ✅ `ItemTooltip.cs`: Tooltip popup on hover (#28)

**Con thieu:**
- **Khong persistent**: Inventory reset khi reload scene
- Khong co item icon / sprite
- Khong co categories / filter / sort

---

#### #12 — Add Harvested Items + Stack ✅

**Da co (Phase 1 + 2):**
- ✅ `InvenItems.cs` redesigned: `itemId`, `name`, `description`, `quantity`, `itemType`, `iconName`
- ✅ `ItemStack.cs`: Stack/merge logic voi `CanStackWith()`, `TryMerge()`
- ✅ `ItemStackTests.cs`: Unit tests cho stacking
- ✅ `PlayerFarmController.cs`: Harvest tra ve `InvenItems` voi quantity tu `CropDefinition.RollYield()`
- ✅ `CropGrowthManager.HarvestCrop()`: Tao item voi day du fields (itemId, quantity, type)

**Cai thien them (optional):**
- `RecyclableInventoryManager.AddInventoryItem()` chua integrate `ItemStack.TryMerge()` — hien them entry moi thay vi gom stack

---

#### #13 — Custom UI Framework 🔨

**Da co (Phase 2):**
- ✅ `UIManager.cs`: Singleton panel manager voi stack-based navigation
  - `ShowPanel()` / `HidePanel()` / `TogglePanel()` by panelId
  - `CloseTopPanel()` — Escape key pops top panel
  - `CloseAllPanels()` — reset UI state
  - Dictionary registry cho O(1) panel lookup
- ✅ `PanelBase.cs`: Base class cho tat ca UI panels
  - `CanvasGroup` fade animation (configurable speed)
  - `closeOnEscape`, `pauseGameWhenOpen` options
  - `Show()` / `Hide()` / `Toggle()` / `SetVisibleImmediate()`
  - Override `OnShow()` / `OnHide()` cho custom logic
- ✅ `NotificationManager.cs`: Toast notification system
  - Fade in/out animation voi CanvasGroup
  - Configurable duration + fade speed
  - Tich hop: harvest notification, new day notification
- ✅ HUD: TimeHUD (time + day + period) tren Canvas

**Con thieu:**
- Confirm dialog (popup "Ban co chac khong?")
- Error popup (hien error tu Firebase)
- Panel implementations: Shop, Settings, Quest Log, Pause Menu
- HUD: minimap

---

#### #14 — Loading Bar / Async Scene Loading ✅

**Da co (Phase 1):**
- ✅ `AsyncLoadingManager.cs`: `SceneManager.LoadSceneAsync()` voi `allowSceneActivation = false`
- ✅ `LoadProgress.cs`: Progress data struct (normalized 0-1 + statusMessage)
- ✅ UI: progressBar (Slider), progressText (%), statusText
- ✅ Minimum display time (tranh flash loading): `minLoadingTime = 1f`
- ✅ Firebase data guard: `WaitForData()` doi data xong moi chuyen scene
- ✅ Events: `OnProgressUpdated`, `OnLoadingComplete`
- ✅ Error handling: `OnDataFailed()` hien thi error message

**Cai thien them (optional):**
- Tips/hints hien thi trong luc loading
- `FakeLoading.cs` van ton tai — co the thay the hoan toan bang `AsyncLoadingManager`

---

### NHOM D: PRODUCTION FEATURES

---

#### #15 — Map Upload/Download Pipeline 🔨

**Da co:**
- `WriteAllTileMapFireBase()`: Upload toan bo tilemap
- `LoadMapForUser()` + `MapToUI()`: Download + render

**Con thieu:**
- Khong co Editor tool de design map roi upload
- Khong co map versioning
- Khong co map template system (nhieu map khac nhau)
- `create_moonlit_garden.py` (Python automation) khong tich hop vao Unity workflow

---

#### #16 — Error Handling + Retry Firebase ✅

**Da co (Phase 1):**
- ✅ `FirebaseTransactionManager.cs`: Retry voi exponential backoff (0.5s → 1s → 2s → 4s → cap 10s)
- ✅ `FirebaseResponse<T>`: Generic response: `Ok(data)` / `Fail(message, exception)`
- ✅ `FirebaseResponse` (non-generic): Cho void operations
- ✅ `WriteWithRetry()`: Auto-retry Firebase writes
- ✅ `FirebaseRetryTests.cs`: 10 unit tests cho retry logic + backoff + response pattern
- ✅ `AsyncLoadingManager.OnDataFailed()`: Error message hien thi tren UI

**Cai thien them (optional):**
- User-facing error Toast/popup (hien chi Debug.Log + statusText)
- Offline detection + queue
- Timeout handling per-request

---

#### #17 — Data Model Chuan Hoa ✅

**Da co (Phase 1 + 2):**
- ✅ `User.cs` enhanced: Name, Gold, Diamond, MapInGame, Version (optimistic concurrency #22)
- ✅ `InvenItems.cs` redesigned: itemId, name, description, quantity, itemType, iconName
- ✅ `ItemDefinition.cs` ScriptableObject: itemId, itemName, ItemType enum, icon, stackable, maxStack, buyPrice, sellPrice, cropId
- ✅ `CropDefinition.cs` ScriptableObject: cropId, cropName, stageDurations[], yieldMin/Max, regrowable, stageSprites[], sellPrice
- ✅ `TilemapDetail.cs` expanded: cropId, plantedAt, growthStage, lastWateredAt, HasCrop, ClearCrop()
- ✅ `TileCoordinate.cs`: Struct voi GetHashCode, Equals, operator ==, implicit Vector3Int conversion
- ✅ `ItemStack.cs`: Stacking logic
- ✅ `GrowthStage` enum: Seed(0) → Sprout(1) → Growing(2) → Mature(3) → Harvestable(4)
- ✅ Firebase granular paths: `Users/{userId}/map/tiles/{index}/...`

- ✅ `ToolDefinition.cs` ScriptableObject: toolId, toolType, tier, icon, cropId (for SeedBag)
- ✅ `ShopCatalog.cs` ScriptableObject: shopName, ShopEntry[] (buyPrice, sellPrice, stock)

**Con thieu:**
- Khong co base class / interface chung giua ItemDefinition va ToolDefinition

---

#### #18 — Git Workflow ❌

**Da co:** Khong co tai lieu.

**Can lam:**
- `.gitignore` cho Unity (Library/, Temp/, Logs/, Build/)
- Branch strategy: `main` → `develop` → `feature/*`
- Commit convention: `feat:`, `fix:`, `docs:`, `refactor:`
- PR template + code review process
- Unity `.meta` file handling rules

---

### NHOM E: VIETNAMESE ART DIRECTION

---

#### #19 — VN Art Direction + Style Guide ❌

**Can lam:**
- Color palette: Tone am nong (ngoi do, tre xanh, dat vang)
- Props nhan dien VN: mai ngoi, hang rao tre, cay chuoi, bien hieu tieng Viet
- Character style: ao ba ba, non la
- Font: Tieng Viet co dau (Cherry Bomb One ho tro Unicode)
- Asset list can ve/mua them

---

#### #20 — Map Layout VN ❌

**Can lam:**
- Zone system: Nha (home) → Vuon (garden) → Cho/Tiem (market)
- Set dressing rules: bao nhieu props/tile de "ra chat" voi it asset
- Pathfinding giua cac zone
- Background parallax layers

---

### NHOM F: SHOP/TRADING SYSTEM

---

#### #21 — Shop/Trading System 🔨

**Da co (Phase 2):**
- ✅ `ShopCatalog.cs` ScriptableObject: `ShopEntry[]` voi buyPrice, sellPrice, stock, available
  - `CreateAssetMenu`: `MoonlitGarden/Shop/ShopCatalog`
  - `CanBuy` / `CanSell` computed properties
- ✅ `ShopManager.cs` Singleton:
  - `OpenShop(catalog)` / `CloseShop()` lifecycle
  - `BuyItem(entry, quantity)`: Gold check → inventory check → execute → notify
  - `SellItem(itemId, quantity)`: Inventory check → catalog check → execute → earn Gold
  - Events: `OnShopOpened`, `OnShopClosed`, `OnItemBought`, `OnItemSold`
  - Error notifications qua `NotificationManager`
- ✅ Integration voi `PlayerEconomyManager` (SpendGold/EarnGold)
- ✅ Integration voi `RecyclableInventoryManager` (AddItem/RemoveQuantity)

**Con thieu:**
- ~~ShopCatalog assets chua tao~~ ✅ `GeneralStore.asset` da tao voi seed_tomato + seed_wheat entries
- ~~NPC shop keeper de trigger OpenShop()~~ ✅ `ShopNPC` da co trong PlayScene + shopCatalog da wire
- Price rules: seasonal bonus, level bonus
- Quantity selector (+ / - / max) trong ShopPanel

---

#### #22 — Transaction Integrity 🔨

**Da co (Phase 2):**
- ✅ `FirebaseTransactionManager.RunAtomicTransaction()`: True atomic transaction dung Firebase native `RunTransaction()` API
  - Optimistic concurrency: Firebase tu dong retry neu data thay doi giua read va write
  - `Func<string, string>` update function nhan current JSON, tra ve new JSON
  - Return `null` de abort transaction
- ✅ `SaveUserWithVersion()`: Versioned save voi optimistic concurrency
  - `User.Version` field (long) tang moi lan save
  - Reject save neu server version > local version (stale data)
  - Notification khi conflict xay ra
- ✅ `UpdateFields()`: Granular multi-field update (chi ghi fields cu the, khong overwrite toan bo)
  - Dung `UpdateChildrenAsync()` — an toan hon `SetValueAsync()`
- ✅ `LoadDataManager.SaveUserInGame()` upgraded: dung `SaveWithVersion()` khi `FirebaseTransactionManager` available
- ✅ Backward compatible: fallback to direct write neu `FirebaseTransactionManager` chua setup

**Con thieu:**
- Firebase Security Rules file de chan client tu set Gold truc tiep
- Atomic buy/sell transaction (Gold - price VA Inventory + item trong cung 1 transaction)
- Transaction audit log (logging moi transaction cho review)

---

#### #23 — Shop UI 🔨

**Da co (Phase 2):**
- ✅ `ShopPanel.cs` extends `PanelBase`:
  - panelId = "shop" — mo qua `UIManager.Instance.ShowPanel("shop")`
  - Buy tab: hien ShopCatalog entries, nut Buy, kiem tra CanAfford
  - Sell tab: hien inventory items co sellPrice > 0, nut Sell
  - Gold display live (subscribes to `OnGoldChanged`)
  - Auto-refresh sau moi transaction
  - Fallback UI: tu tao rows neu khong co prefab
- ✅ Integrates voi `ShopManager.BuyItem()` / `SellItem()`

**Con thieu:**
- Quantity selector (+ / - / max) — hien mua/ban 1 cai 1 luc
- Confirm dialog truoc khi mua
- Item icon display
- Shop item row prefab (hien dung fallback)

---

### NHOM G: CROP SYSTEM

---

#### #24 — Crop Data Library ✅

**Da co (Phase 2):**
- ✅ `CropDefinition.cs` ScriptableObject voi day du fields:
  - `cropId`, `cropName`, `description`
  - `seedItemId`, `harvestItemId` (lien ket ItemDefinition)
  - `stageDurations[]` (thoi gian moi stage transition)
  - `stageSprites[]` (Sprite cho 5 stages)
  - `yieldMin`, `yieldMax`, `RollYield()` (random yield)
  - `requiresWater`, `witherTime`
  - `regrowable`, `regrowToStage`
  - `sellPrice`
- ✅ Helper methods: `CalculateStageFromElapsed()`, `GetTimeToReachStage()`, `GetStageSprite()`, `TotalGrowTime`
- ✅ `GrowthStage` enum: Seed → Sprout → Growing → Mature → Harvestable
- ✅ Folder `Assets/Data/Crops/` voi 2 assets: **Tomato** (regrowable) + **Wheat** (one-time)
- ✅ CreateAssetMenu: `MoonlitGarden/Crops/CropDefinition`

**Cai thien them (optional):**
- Them nhieu crop definitions (corn, strawberry, carrot, v.v.)
- Season field (Spring/Summer/Fall/Winter/All)

---

#### #25 — Growth Persistence 🔨

**Da co (Phase 1 + 2):**
- ✅ `TilemapDetail` co fields: `cropId`, `plantedAt` (Unix ms), `growthStage`, `lastWateredAt`
- ✅ `HasCrop` property + `ClearCrop()` method
- ✅ `CropGrowthManager.ScanForActiveCrops()`: Khi load game, scan toan bo tiles co crop
- ✅ `CalculateStageFromElapsed()`: Tinh stage tu `elapsedTime = now - plantedAt`
- ✅ Firebase sync: `TileMapManager.UpdateCropDataAsync()` ghi crop data per-tile
- ✅ Offline growth: Crops "lon" dua tren real-time timestamp, catch-up khi login lai

**Con thieu:**
- Withered state chua implement (field `witherTime` co nhung chua check)
- `lastWateredAt` chua duoc persist rieng len Firebase (chi persist cropId + growthStage)

---

### NHOM H: ITEM/TOOL SYSTEM

---

#### #26 — Item Use & Tool Equip 🔨

**Da co (Phase 2):**
- ✅ `ToolDefinition.cs` ScriptableObject:
  - `ToolType` enum: Hoe, WateringCan, Sickle, SeedBag, Axe, Pickaxe
  - `toolId`, `toolName`, `description`, `tier`, `icon`
  - `cropId` (for SeedBag — determines what gets planted)
  - `CreateAssetMenu`: `MoonlitGarden/Items/ToolDefinition`
- ✅ `EquipmentManager.cs` Singleton:
  - `EquipTool()` / `UnequipTool()` / `GetStarterTool(type)`
  - `CurrentTool`, `CurrentToolType`, `CurrentSeedCropId` properties
  - `OnToolChanged` event for UI updates
  - Number keys 1-6 for quick tool switching
  - `starterTools[]` array for default tools
- ✅ `PlayerFarmController.cs` updated:
  - `E` key: Use equipped tool (context-sensitive)
  - Hoe→till, WateringCan→water, Sickle→harvest, SeedBag→plant
  - Legacy keys C/V/F/M still work as direct shortcuts
- ✅ `ToolHUD.cs`: Displays current tool name (subscribes to OnToolChanged)

**Con thieu:**
- ToolDefinition assets chua tao (can tao trong Unity Inspector)
- Durability system (hien khong co)
- Tool upgrade mechanic
- Farm action animations per tool type

---

#### #27 — Quickbar + Input Binding 🔨

**Da co (Phase 2):**
- ✅ `QuickbarUI.cs`: Horizontal toolbar (Stardew Valley-style) o duoi man hinh
  - Auto-generate slots tu `EquipmentManager.starterTools[]` (4-6 slots)
  - `HorizontalLayoutGroup` + `ContentSizeFitter` cho auto-layout
  - Subscribe `OnToolChanged` event → highlight active slot
  - Fallback UI: tao slots programmatically neu khong co prefab
  - `Refresh()` API cho runtime rebuild
- ✅ `QuickbarSlot.cs`: Moi slot hien thi:
  - Icon (tool sprite)
  - Key binding number (1-6)
  - Tool name (bottom label)
  - Highlight state (normal/selected/empty colors)
- ✅ Input: Phim 1-6 switch tools (via `EquipmentManager.HandleToolSwitchInput()`)

**Con thieu:**
- Migrate tu legacy `Input.GetKeyDown` sang New Input System
- Scroll wheel de chon slot
- Rebindable keys trong Settings
- Slot prefab (hien dung programmatic generation)

---

#### #28 — Inventory Interactions 🔨

**Da co (Phase 2):**
- ✅ `CelllItemData.cs` upgraded: `IPointerClickHandler` + `IPointerEnterHandler` + `IPointerExitHandler`
  - Click → open `InventoryActionPanel` + show `ItemTooltip`
  - Hover → show tooltip, exit → hide tooltip
  - Quantity badge (TMP) cho stacked items
  - Highlight image cho selected state
  - SFX: `AudioManager.PlaySFX("ui_click")` on click
- ✅ `InventoryActionPanel.cs` Singleton: Context action panel
  - Use button: consume item hoac equip seed (auto-switch to SeedBag tool)
  - Equip button: equip tool item (matches by toolId)
  - Drop button: remove 1x item from inventory
  - Split button: split stack in half (chi hien khi quantity > 1)
  - Smart visibility: buttons chi hien dua tren item type (Seed/Tool/Consumable)
  - `OnInventoryAction` event cho UI refresh
- ✅ `ItemTooltip.cs` Singleton: Tooltip popup
  - Hien thi name, description, quantity, item type
  - Follow cursor position voi offset
  - `ClampToScreen()`: tu dong clamp de khong bi tran man hinh
  - `CanvasGroup` fade (khong block raycasts)

**Con thieu:**
- Full drag & drop (keo item giua slots) — phuc tap voi RecyclableScrollRect
- Drop item ra ngoai inventory → tha xuong dat
- Right-click context menu (hien dung click panel)
- Inventory sorting / filtering

---

### NHOM I: TINH NANG BO SUNG (De Xuat Moi)

---

#### #29 — Camera Follow System ✅ `Critical`

**Da co (Phase 1):**
- ✅ `CameraFollow.cs`: Smooth follow player voi `Vector3.Lerp`, configurable `smoothSpeed`
- ✅ Boundary clamp: `useBounds` + `minBounds`/`maxBounds` gioi han camera trong map
- ✅ Zoom support: `targetZoom` + smooth zoom transition
- ✅ `SetTarget()` API: Co the thay doi target runtime

#### #30 — Audio System 🔨 `High`

**Da co (Phase 2):**
- ✅ `AudioManager.cs` Singleton: BGM + SFX manager
  - Separate AudioSource cho BGM (loop) va SFX (one-shot)
  - `PlaySFX(name)` / `PlayBGM(name)` API
  - SFX registry: till, plant, water, harvest, buy, sell, equip, notification, ui_click, new_day
  - Time-of-day BGM: auto-switch day/night music via `GameTimeManager.OnHourChanged`
  - `CrossfadeToBGM()`: Smooth 2s fade transition giua cac track
  - Volume control: `SetBGMVolume()` / `SetSFXVolume()` / `SetMute()`
  - AudioMixer support (optional) cho grouped volume control
- ✅ SFX integrated vao: PlayerFarmController (till/plant/water/harvest), ShopManager (buy/sell), EquipmentManager (equip), GameTimeManager (new_day)
- ✅ Wired in PlayScene: AudioManager GO voi 2 AudioSource components

**Con thieu:**
- AudioClip assets chua co (can import/tao audio files)
- Footstep SFX (player movement)
- Ambient sounds (birds, wind, water)
- AudioMixer asset chua tao

#### #31 — NPC System + Dialogue ❌ `Medium`

Can cho shop keeper, quest giver, story. NPC co schedule (di chuyen theo gio), dialogue tree, portrait UI.

#### #32 — Quest/Mission System ❌ `Medium`

Motivation loop: "Trong 10 cay hoa" → reward Gold. Quest types: thu hoach X crop, ban Y item, dat Z Gold.

#### #33 — Energy/Stamina System ✅ `Medium`

Classic farming mechanic: moi action ton stamina, het stamina → khong lam duoc gi, ngu de hoi phuc. Gioi han so action/ngay.

**Implemented:**
- `StaminaManager.cs` — singleton, 50 max stamina, `TrySpendStamina(cost)` API, restores on `GameTimeManager.OnNewDay`
- `StaminaHUD.cs` — Canvas slider in top-left, color changes: green (>50%), orange (20-50%), red (<20%)
- `PlayerFarmController.cs` — `StaminaCheck()` guards on Till (1), Plant (2), Water (1), Harvest (2)
- Vietnamese notifications: "Hết năng lượng!" + "Ngày X bắt đầu! Năng lượng đã phục hồi..."
- Scene: `StaminaManager` GO + `StaminaHUD` panel wired in PlayScene

#### #34 — Settings Menu ✅ `Low`

Volume sliders (BGM/SFX), resolution, fullscreen, language (VN/EN), controls display.

**Implemented:**
- `SettingsManager.cs` — singleton, persists BGM/SFX/fullscreen to PlayerPrefs, applies on Start
- `SettingsPanel.cs` — ESC to open/close, Time.timeScale=0 while open, sliders sync to SettingsManager
- Scene: dark modal panel (400×280) centered in Canvas with BGM slider, SFX slider, fullscreen toggle, close button
- All refs wired; SettingsPanel script on SettingsPanelController GO

#### #35 — Tutorial/Onboarding ✅ `Low`

Huong dan lan dau: cach di chuyen, cach farm, cach mo inventory. Tooltip arrows, highlight objects.

**Implemented:**
- `TutorialManager.cs` — singleton, 5 steps (WalkAround, TillSoil, PlantSeed, WaterCrop, HarvestCrop)
- Progress persisted to PlayerPrefs; tutorial auto-deactivates when all steps complete
- WalkAround auto-detected via position delta in Update
- Till/Plant/Water/Harvest steps hooked into PlayerFarmController via `TutorialManager.Instance?.CompleteStep()`
- Each completion shows a Vietnamese notification hint; final completion shows "Hướng dẫn hoàn thành!"
- SK-2 starter notification hints for new players (via RecyclableInventoryManager)

#### #36 — Weather System ❌ `Low`

Mua anh huong crop (khong can tuoi khi troi mua). Particle effects: mua, tuyet, nang.

#### #37 — Seasons System ❌ `Low`

Spring/Summer/Fall/Winter → anh huong crop availability, gia ban, visual tileset.

#### #38 — Save Slot Management ❌ `Low`

Nhieu save slots, local backup khi offline, cloud sync indicator.

#### #39 — Localization Framework ❌ `Low`

String table (VN/EN) thay vi hardcode text. Unity Localization package hoac custom.

#### #40 — Enemy/Combat System ❌ `Low`

Mole + Treant sprites co san trong Tiny RPG Forest nhung **chua dung**. Hero attack sprites cung co. Co the them combat don gian: enemies xuat hien ban dem, pha hoai crop.

#### #41 — Mobile Optimization ❌ `Low`

Touch controls (joystick ao), UI scaling cho nhieu kich thuoc man hinh, battery optimization.

---

## 4. Van De Ky Thuat Can Xu Ly / Technical Debt

| # | Van de | File | Muc do | Trang thai |
|---|--------|------|--------|:----------:|
| T1 | `SetStateForTilemapDetail()` dung vong for O(n) de tim tile | `TileMapManager.cs` | Critical | ✅ Fixed |
| T2 | Full-document overwrite Firebase moi khi thay doi 1 tile | `TileMapManager.cs` | Critical | ✅ Fixed |
| T3 | Race condition: scene load truoc khi Firebase data san sang | `AsyncLoadingManager.cs` | Critical | ✅ Fixed |
| T4 | Inventory toggle bang hack Y=1000 thay vi SetActive/CanvasGroup | `RecyclableInventoryManager.cs` | Medium | ✅ Fixed — `ToggleInventory()` dung `SetActive()` |
| T5 | Diagonal movement nhanh hon (thieu normalize) | `PlayerMovement.cs` | Medium | ✅ Fixed |
| T6 | `InvenItems` mutable + thieu fields (id, quantity, type) | `InvenItems.cs` | Critical | ✅ Fixed |
| T7 | Debug.Log tieng Viet con sot trong production code | `PlayerMovement_Mouse.cs` | Low | ✅ Fixed — file da duoc viet lai hoan toan, khong con debug logs |
| T8 | `GameObject.Find()` dung khap noi thay vi DI/SerializeField | Nhieu files | Medium | ❌ |
| T9 | Hardcoded input keys (E, C, V, F, M, 1-6, B, L) — khong rebindable | Nhieu files | Medium | ❌ |
| T10 | Khong co null check khi MapInGame co the null | `TileMapManager.cs` | High | ✅ Fixed |
| T11 | RecyclableScrollRectEditor.cs thieu ']' — block toan bo compilation | `RecyclableScrollRectEditor.cs` | Critical | ✅ Fixed |
| T12 | Shop khong the mo tu gameplay — `OpenShop()` khong duoc goi tu dau | `ShopManager.cs` | Critical | ✅ Fixed — ShopNPC.cs + BoxCollider2D trigger goi `OpenShop()` khi nhan E (BP2) |
| T13 | Inventory khong persist — harvest items chi luu local, khong save Firebase | `RecyclableInventoryManager.cs` | Critical | ✅ Fixed — `LoadInventoryFromFirebase()` on Start + `FlushSave()` sau buy/sell (BP3) |
| T14 | Seed selection bypass — fallback `defaultCropId` hardcode, bo qua inventory | `PlayerFarmController.cs` | Critical | ✅ Fixed — da xoa field `defaultCropId`, bat buoc chon seed qua inventory (BP1-4) |
| T15 | Gold save delay 2s — crash trong 2s = mat gold | `PlayerEconomyManager.cs` | High | ✅ Fixed — `FlushSave()` goi ngay sau harvest trong `PlayerFarmController.HandleHarvest()` (BP4) |
| T16 | Crop visual placeholder — dung Forest tile cho moi stage, khong co sprite rieng | `CropGrowthManager.cs` | High | ✅ Fixed — `_cropVisuals` SpriteRenderer dict + `RefreshCropVisual()` per stage |
| T17 | Khong co water/harvest indicator UI tren tile | `CropIndicatorUI.cs` | High | ✅ Fixed — CropIndicatorUI wired (tileMapManager + groundTilemap) |

---

## 5. Lo Trinh Phat Trien / Development Roadmap

---

### Phase 1 — Nen Tang / Foundation

> Muc tieu: Fix technical debt, chuan hoa data, he thong co ban hoat dong on dinh

**Fix Technical Debt:**
- [x] T1: Thay O(n) lookup bang `Dictionary<(int,int), TilemapDetail>` trong TileMapManager
- [x] T2: Granular Firebase path update thay vi full-document overwrite
- [x] T3: Async loading — doi Firebase data xong moi chuyen scene
- [x] T6: Redesign `InvenItems` → `ItemDefinition` ScriptableObject (id, type, quantity, icon, price)
- [x] T5: Normalize diagonal movement
- [x] T10: Null checks cho MapInGame

**Core Systems:**
- [x] #14: Async scene loading voi progress bar (thay FakeLoading)
- [x] #16: Firebase error handling + retry (exponential backoff)
- [x] #17: Data model chuan hoa (ItemDefinition, CropDefinition ScriptableObjects)
- [x] #29: Camera follow system (custom smooth follow + boundary + zoom)
- [x] #12: Item stacking trong inventory (quantity field + merge logic)

---

### Phase 2 — He Thong Gameplay / Gameplay Systems

> Muc tieu: Core farming loop hoan chinh, shop hoat dong, tool system

**Farming Core:**
- [x] #24: CropDefinition ScriptableObject + crop catalog (Tomato + Wheat assets)
- [x] #4: Plant growth state machine (CropGrowthManager — time-based, event-driven)
- [x] #25: Growth persistence (plantedAt, stage calculation on load, Firebase sync)
- [x] #3: Mo rong farm actions (till/plant/water/harvest qua CropGrowthManager)

**Item & Tool:**
- [x] #26: ToolDefinition + EquipmentManager + E-key tool action + ToolHUD
- [x] #27: QuickbarUI + QuickbarSlot (visual toolbar, key binding display, highlight)
- [x] #28: Inventory interactions (click-to-use, tooltip, split, context panel)

**Economy:**
- [x] #9: PlayerEconomyManager (Gold/Diamond earn/spend + events + Firebase sync)
- [x] #21: ShopManager + ShopCatalog ScriptableObject (buy/sell logic)
- [x] #23: ShopPanel UI (Buy/Sell tabs, Gold display, PanelBase integration)
- [x] #22: Transaction integrity (RunAtomicTransaction + versioned save + UpdateFields)

**UX:**
- [x] #5: Day-Night cycle voi Light2D (GameTimeManager + DayNightController + TimeHUD)
- [x] #13: UI framework (UIManager + PanelBase + NotificationManager)
- [x] #30: Audio system (AudioManager + SFX integration + time-of-day BGM)
- [x] #10: Conflict-safe load (real-time listener + OnServerDataChanged + version check)

---

### Phase 2.5 — Ket Noi Luong Choi / Gameplay Loop Wiring (CRITICAL)

> Muc tieu: Ket noi cac he thong da co thanh vong choi hoan chinh theo FLOW.md
> Trang thai: **Chua bat dau** — Cac he thong da code nhung CHUA KET NOI voi nhau

**Phan tich Gap (2026-03-11):**

Luong choi theo FLOW.md:
```
Di chuyen → Chon hat giong → Gieo hat → Cay lon → Thu hoach → Nhan tien → Mua hat → Lap lai
```

Cac diem gay (break points) trong vong choi:

| # | Break Point | Mo ta | Muc do |
|---|-------------|-------|--------|
| BP1 | Inventory → Equipment | ✅ Fixed — BP1-1/BP1-2/BP1-3/BP1-4 tat ca done | Critical |
| BP2 | Shop khong mo duoc | ✅ Fixed — ShopNPC + GeneralStore.asset + shopCatalog wired | Critical |
| BP3 | Inventory khong luu | ✅ Fixed — `LoadInventoryFromFirebase` on Start + `FlushSave` after buy/sell | Critical |
| BP4 | Gold delay 2s | ✅ Fixed — `FlushSave()` da goi ngay sau harvest trong `PlayerFarmController` | High |
| BP5 | Khong co visual crop | ✅ Fixed — SpriteRenderer per tile, swap by GrowthStage, 10 PixelLab sprites | High |
| BP6 | Khong co chi bao tuoi | ✅ Fixed — `CropIndicatorUI` wired (tileMapManager + groundTilemap) | High |
| BP7 | Khong co chi bao thu hoach | ✅ Fixed — `CropIndicatorUI` xu ly ca water + harvest indicator | Medium |

**Ke hoach sua — Phase 2.5A: Shop Access (Critical)**
- [x] BP2-1: Tao `ShopNPC.cs` — NPC voi BoxCollider2D trigger, nhan E de mo shop
- [x] BP2-2: Tao ShopNPC GameObject trong PlayScene (vi tri gan nha)
- [x] BP2-3: Tao `GeneralStore.asset` ShopCatalog voi seed entries (Tomato, Wheat) — `Assets/Data/Shop/GeneralStore.asset`
- [x] BP2-4: Wire ShopNPC → `ShopManager.OpenShop(catalog)` → ShopPanel hien thi — shopCatalog da assign trong Inspector

**Ke hoach sua — Phase 2.5B: Seed Selection (Critical)**
- [x] BP1-1: Them nut "Equip" trong `InventoryActionPanel` cho seed items — da co `useButton` voi text "Plant" cho type=="seed"
- [x] BP1-2: Khi equip seed → `EquipmentManager.EquipTool(seedBagDef)` voi cropId tuong ung — `EquipSeedFromInventory()` da implement day du
- [x] BP1-3: QuickbarUI highlight slot seed dang equip — OnToolChanged fallback by ToolType cho runtime SeedBag tools
- [x] BP1-4: Bo `defaultCropId` fallback — da xoa khoi PlayerFarmController

**Ke hoach sua — Phase 2.5C: Data Persistence (Critical)**
- [x] BP3-1: `RecyclableInventoryManager.SaveInventoryToFirebase()` — da implement day du (batched 1.5s)
- [x] BP3-2: Goi save sau moi harvest, buy, sell action — `FlushSave()` sau `AddInventoryItem` + `RemoveQuantityAt` trong ShopManager + harvest da co
- [x] BP4-1: `PlayerEconomyManager.FlushSave()` goi ngay sau harvest — da co trong `PlayerFarmController.HandleHarvest()` line 306
- [x] BP3-3: Load inventory tu Firebase khi start PlayScene — `Start()` gio goi `LoadInventoryFromFirebase()` thay vi init empty

**Ke hoach sua — Phase 2.5D: Visual Feedback (High)**
- [x] BP5-1: Tao pixel art sprites cho 5 growth stages (PixelLab MCP) — 10 sprites generated + imported
- [x] BP5-2: `CropGrowthManager` swap SpriteRenderer theo `GrowthStage` — `RefreshCropVisual()` + `RemoveCropVisual()` + `_cropVisuals` dict; `groundTilemap` wired trong scene
- [x] BP6-1: Tao "needs water" icon overlay tren tile chua tuoi — `CropIndicatorUI` da wire tileMapManager + groundTilemap
- [x] BP7-1: Tao "harvestable" icon (dau !) overlay tren tile san sang thu hoach — `CropIndicatorUI` xu ly ca water + harvest indicators
- [x] BP5-3: Tao crop sprites cho Tomato va Wheat (5 stages moi loai) — `Assets/Sprites/Crops/Tomato/` + `Assets/Sprites/Crops/Wheat/`

**Ke hoach sua — Phase 2.5E: Starter Kit**
- [x] SK-1: Khi user moi dang ky, them starter seeds vao inventory (5x Tomato, 3x Wheat) — GiveStarterPackIfNew() + starterPackGiven flag
- [x] SK-2: Hien thi huong dan ngan — 4 sequential notifications sau khi nhan starter pack (ShowTutorialHints coroutine)

---

### Phase 3 — Noi Dung & Hoan Thien / Content & Polish

> Muc tieu: Content phong phu, polish UX, san sang cho beta

**Content:**
- [ ] #19: Vietnamese art direction + custom assets (PixelLab MCP)
- [ ] #20: Map zones (nha, vuon, cho/tiem) + set dressing
- [x] #31: NPC system + dialogue ✅
- [ ] #32: Quest/mission system
- [x] #33: Energy/Stamina system ✅
- [ ] #40: Enemy/Combat (dung Mole + Treant assets)

**Polish:**
- [x] #34: Settings menu ✅
- [x] #35: Tutorial/onboarding ✅
- [ ] #39: Localization framework (VN/EN)
- [ ] #18: Git workflow documentation
- [ ] #15: Map pipeline (editor tool)

**Advanced (Neu co thoi gian):**
- [x] #36: Weather system ✅
- [ ] #37: Seasons system
- [ ] #38: Save slot management
- [ ] #41: Mobile optimization

---

## 6. Quy Uoc Ky Thuat / Technical Conventions

### Folder Structure (De xuat)
```
Assets/Scripts/
├── Core/           # GameManager, TimeManager, AudioManager, SaveManager
├── Data/           # ScriptableObjects (CropDefinition, ItemDefinition, ShopCatalog)
├── Entities/       # Data models (User, Map, TilemapDetail) — da co
├── Farm/           # PlayerFarmController, CropGrowthManager, TileMapManager
├── Firebase/       # FirebaseLoginManager, FirebaseDatabaseManager, FirebaseTransaction
├── Inventory/      # InventoryManager, InventorySlot, ItemStack, DragDropHandler
├── Player/         # PlayerMovement, PlayerAnimator, EquipmentManager
├── Shop/           # ShopManager, ShopUI, TransactionManager
├── UI/             # UIManager, PanelBase, PopupManager, HUD, LoadingScreen
└── Utils/          # Extensions, Constants, Helpers
```

### Naming Conventions
- Scripts: PascalCase (`PlayerMovement.cs`)
- Variables: camelCase (`currentTool`, `plantedAt`)
- Constants: UPPER_SNAKE (`MAX_STACK_SIZE`)
- ScriptableObjects: PascalCase (`Tomato.asset`, `BasicHoe.asset`)
- Scenes: PascalCase (`LoginScene`, `PlayScene`)

### Firebase Data Structure (De xuat)
```
Users/
  {userId}/
    profile/        # name, gold, diamond, level, lastLogin
    inventory/      # itemId → quantity
    map/
      tiles/        # "x_y" → {state, cropId, plantedAt, stage}
    stats/          # totalHarvests, totalSold, playTime
```
