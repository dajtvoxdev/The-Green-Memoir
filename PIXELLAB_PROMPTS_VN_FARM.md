# PixelLab Prompts - Vietnamese Countryside Farm

## Checklist TODO

### Terrain Tilesets (A)
- [ ] **TILESET_RICE** - Ruong lua nuoc (rice paddy water + embankment + mud)
- [ ] **TILESET_FARMSOIL** - Dat trong (tilled soil + grass transition)
- [ ] **TILESET_PATH** - Duong lang (dirt/mud path + grass)
- [ ] **TILESET_CANAL** - Kenh nuoc (irrigation canal + bank)

### Props Tileset (B)
- [ ] **TILESET_PROPS** - Hang rao, bui co, da, goc ra, bui cay nho

### Sprite Props Roi (C)
- [ ] **SPRITE_HOUSE** - Nha mai ngoi do
- [ ] **SPRITE_BAMBOO** - Bui tre
- [ ] **SPRITE_BANANA** - Cay chuoi
- [ ] **SPRITE_BUFFALO** - Trau
- [ ] **SPRITE_DOG** - Cho
- [ ] **SPRITE_CAT** - Meo
- [ ] **SPRITE_CHICKEN** - Ga

### Map Layout (D)
- [ ] **MAP_VN_FARM** - Layout tong the vuon/ruong Viet Nam

### Export & Unity Import
- [ ] Export all tileset PNGs (32x32 per tile)
- [ ] Slice spritesheets in Unity (Sprite Editor, 32x32 grid)
- [ ] Create Tile Palette per terrain type
- [ ] Paint Tilemap layers: Ground > Terrain > Props > Objects

---

## (A) TERRAIN TILESETS

### TILESET_RICE - Ruong Lua Nuoc

```
Tool: create_topdown_tileset
```

| Parameter | Value |
|-----------|-------|
| `lower_description` | `"shallow greenish rice paddy water with subtle reflections, Vietnamese rice field flooded terrain"` |
| `upper_description` | `"compact wet brown earth, muddy path soil around rice paddies"` |
| `transition_description` | `"muddy embankment rice paddy edge, wet brown mud with tiny grass tufts"` |
| `transition_size` | `0.3` |
| `tile_size` | `{"width": 32, "height": 32}` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |
| `text_guidance_scale` | `7.0` |

**Full prompt (copy-paste ready):**
> Top-down 2D pixel art tileset, 32x32, seamless, Vietnamese rice paddy terrain.
> Inner tile: shallow greenish rice paddy water with subtle reflections.
> Transition: muddy embankment rice paddy edge, wet brown mud, tiny grass tufts.
> Outer tile: muddy path/soil around paddies, compact wet earth.
> Clean readable shapes, consistent lighting.
> Negative: city, cars, asphalt road, modern buildings, sci-fi, fantasy.

---

### TILESET_FARMSOIL - Dat Trong

```
Tool: create_topdown_tileset
```

| Parameter | Value |
|-----------|-------|
| `lower_description` | `"dark brown tilled farm soil with faint plowed rows, Vietnamese vegetable garden plot"` |
| `upper_description` | `"lush green countryside grass, rural Vietnamese grassland"` |
| `transition_description` | `"soil to grass edge with small dirt clumps and scattered pebbles"` |
| `transition_size` | `0.25` |
| `tile_size` | `{"width": 32, "height": 32}` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |
| `text_guidance_scale` | `7.0` |

**Full prompt (copy-paste ready):**
> Top-down pixel art tileset, 32x32, seamless, Vietnamese farm soil plots.
> Inner tile: dark brown tilled soil with faint rows.
> Transition: soil to grass edge with small dirt clumps.
> Outer tile: lush green countryside grass.
> Clean readable shapes.
> Negative: city, cars, asphalt road, modern buildings, sci-fi, fantasy.

---

### TILESET_PATH - Duong Lang

```
Tool: create_topdown_tileset
```

| Parameter | Value |
|-----------|-------|
| `lower_description` | `"light brown dirt path with subtle wheel ruts, rural Vietnamese village trail"` |
| `upper_description` | `"green grass, Vietnamese countryside grassland"` |
| `transition_description` | `"path edge with grass encroaching, tiny stones and sparse weeds"` |
| `transition_size` | `0.2` |
| `tile_size` | `{"width": 32, "height": 32}` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |
| `text_guidance_scale` | `7.0` |

**Full prompt (copy-paste ready):**
> Top-down pixel art tileset, 32x32, seamless, rural Vietnamese dirt/mud paths.
> Inner tile: light brown dirt path with subtle ruts.
> Transition: path edge with grass encroaching, tiny stones.
> Outer tile: green grass.
> Negative: city, asphalt road, traffic signs, modern objects.

---

### TILESET_CANAL - Kenh Nuoc

```
Tool: create_topdown_tileset
```

| Parameter | Value |
|-----------|-------|
| `lower_description` | `"shallow irrigation canal water, slightly darker blue-green than paddy water, gentle flow line hints"` |
| `upper_description` | `"green grass or mud path, Vietnamese countryside ground"` |
| `transition_description` | `"canal bank mud with sparse grass and small exposed roots"` |
| `transition_size` | `0.25` |
| `tile_size` | `{"width": 32, "height": 32}` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |
| `text_guidance_scale` | `7.0` |

**Full prompt (copy-paste ready):**
> Top-down pixel art tileset, 32x32, seamless, Vietnamese irrigation canal.
> Inner tile: shallow canal water, slightly darker than paddy water, gentle flow hints.
> Transition: canal bank mud with grass.
> Outer tile: grass or mud path.
> Negative: city, modern, fantasy.

---

## (B) PROPS TILESET

### TILESET_PROPS - Chi Tiet Dong Que

```
Tool: create_topdown_tileset
```

| Parameter | Value |
|-----------|-------|
| `lower_description` | `"transparent or plain green grass background"` |
| `upper_description` | `"Vietnamese countryside detail props: wood fence segments, small rocks, grass tufts, straw pile, small bushes"` |
| `transition_description` | `"clean edges, readable silhouettes for each prop"` |
| `transition_size` | `0.15` |
| `tile_size` | `{"width": 32, "height": 32}` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |

**Full prompt (copy-paste ready):**
> Top-down pixel art props tileset, 32x32, Vietnamese countryside details:
> wood fence segments, small rocks, grass tufts, straw pile, small bushes.
> Seamless placement, clean silhouettes.
> Negative: modern objects, city.

---

## (C) SPRITE PROPS ROI (Transparent Background)

### SPRITE_HOUSE - Nha Mai Ngoi Do

```
Tool: create_map_object
```

| Parameter | Value |
|-----------|-------|
| `description` | `"Top-down pixel art Vietnamese rural house with red clay tiled roof, simple wooden walls, small front porch with columns, readable silhouette, no extra objects, no modern elements"` |
| `width` | `96` |
| `height` | `96` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |

---

### SPRITE_BAMBOO - Bui Tre

```
Tool: create_map_object
```

| Parameter | Value |
|-----------|-------|
| `description` | `"Top-down pixel art bamboo cluster (tre), tall green bamboo grove for Vietnamese countryside, readable silhouette, no extra objects"` |
| `width` | `64` |
| `height` | `96` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |

---

### SPRITE_BANANA - Cay Chuoi

```
Tool: create_map_object
```

| Parameter | Value |
|-----------|-------|
| `description` | `"Top-down pixel art banana tree (cay chuoi), broad green leaves, Vietnamese tropical plant, readable silhouette, no extra objects"` |
| `width` | `48` |
| `height` | `64` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |

---

### SPRITE_BUFFALO - Trau

```
Tool: create_map_object
```

| Parameter | Value |
|-----------|-------|
| `description` | `"Top-down pixel art Vietnamese water buffalo (trau) standing, dark gray body, large horns, readable silhouette, no extra objects"` |
| `width` | `48` |
| `height` | `48` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |

---

### SPRITE_DOG - Cho

```
Tool: create_map_object
```

| Parameter | Value |
|-----------|-------|
| `description` | `"Top-down pixel art rural Vietnamese dog, medium size, golden-brown coat, standing pose, readable silhouette, no extra objects"` |
| `width` | `32` |
| `height` | `32` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |

---

### SPRITE_CAT - Meo

```
Tool: create_map_object
```

| Parameter | Value |
|-----------|-------|
| `description` | `"Top-down pixel art cat, small tabby cat sitting, Vietnamese countryside pet, readable silhouette, no extra objects"` |
| `width` | `24` |
| `height` | `24` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |

---

### SPRITE_CHICKEN - Ga

```
Tool: create_map_object
```

| Parameter | Value |
|-----------|-------|
| `description` | `"Top-down pixel art chicken, Vietnamese countryside rooster/hen, small body, readable silhouette, no extra objects"` |
| `width` | `24` |
| `height` | `24` |
| `view` | `"high top-down"` |
| `outline` | `"single color"` |
| `shading` | `"selective outlining"` |
| `detail` | `"medium"` |

---

## (D) MAP LAYOUT TONG THE

### MAP_VN_FARM - Layout Vuon Ruong Viet Nam

**Tilemap Layer Order (Unity):**

```
Layer 0 - Ground:     Grass (base, fill entire map)
Layer 1 - Terrain:    Dirt paths, farm soil, rice paddy water, canal
Layer 2 - Props:      Fences, rocks, grass tufts, straw
Layer 3 - Objects:    House, bamboo, banana trees, animals (sprites)
```

**Layout Grid (approx 30x20 tiles = 960x640 px):**

```
+--------------------------------------------------------------+
|  tre  tre                    RUONG LUA (rice paddy)     tre  |
|  tre       +-kenh-nuoc-+   +-------------------------+      |
|            |  canal     |   | lua | lua | lua | lua   |      |
|   chuoi    |            |   | lua | lua | lua | lua   |      |
|            +-----+------+   +-------------------------+      |
|                  |               bo ruong (embankment)        |
|   +--------+    duong dat                                    |
|   |  NHA   |-----(path)--------+                             |
|   |  (san) |    duong dat      |                             |
|   +--------+         +--------+--------+                     |
|      ga ga cho       | DAT TRONG       |   bui co   da      |
|      meo             | (farm soil)     |                     |
|                      | luong | luong   |                     |
|   hang rao           +--------+--------+        tre          |
|                           duong dat                          |
|  co   co   co   co   co   co   co   co   co   co   co   co |
+--------------------------------------------------------------+
```

**Map Design Rules:**
- House + yard: center-left, 6x6 tile area
- Farm soil plots: adjacent to house, 8x4 tile area
- Rice paddies: top-right quadrant, 8x4 tile area
- Canal: runs between house area and paddies, 2 tiles wide
- Dirt paths: 2 tiles wide, connect all areas
- Bamboo/banana: scattered around house edges and map borders
- Animals: buffalo near paddies, chickens near yard, dog+cat near house
- Grass: fills all remaining space (base layer)

**Full prompt (copy-paste ready):**
> Top-down pixel art Vietnamese countryside farm map, tilemap-ready.
> Layout: small house yard near center-left, farm soil plots near the house,
> rice paddies and irrigation canals to the right/top, muddy village paths
> connecting all areas, lush grass borders, bamboo and banana trees around
> the house edges.
> Place a few animals: one water buffalo near paddies, chickens near the yard,
> a dog and a cat near the house.
> Keep it clean and playable for farming gameplay loop.
> Negative: city, cars, asphalt road, modern buildings, fantasy, sci-fi.

---

## Export & Import Unity

1. **Export PNGs** - Download each tileset PNG from PixelLab (`get_topdown_tileset` / `get_map_object`)
2. **Import to Unity** - Drag PNGs into `Assets/Sprites/Tilesets/` folder
3. **Sprite Settings** - Inspector > Texture Type: Sprite > Sprite Mode: Multiple > Pixels Per Unit: 32 > Filter Mode: Point (no filter) > Compression: None
4. **Slice** - Sprite Editor > Slice > Grid by Cell Size > 32x32 > Apply
5. **Create Tile Palette** - Window > 2D > Tile Palette > Create New Palette per terrain type (Ground, Path, Rice, Canal, Props)
6. **Create Tilemap Layers** - In Hierarchy: Grid > create child Tilemaps for each layer (Ground, Terrain, Props, Objects) > set Sorting Layer order
7. **Paint** - Select palette > paint tiles onto appropriate Tilemap layer
8. **Sprite Props** - For house/trees/animals: drag sprite directly into scene as GameObjects on the Objects layer

**Recommended folder structure:**
```
Assets/
  Sprites/
    Tilesets/
      tileset_grass_path.png
      tileset_farmsoil.png
      tileset_rice_paddy.png
      tileset_canal.png
      tileset_props.png
    Objects/
      house_red_roof.png
      bamboo_cluster.png
      banana_tree.png
      water_buffalo.png
      dog.png
      cat.png
      chicken.png
  Tiles/
    Palettes/
      Ground.asset
      Terrain.asset
      Props.asset
```
