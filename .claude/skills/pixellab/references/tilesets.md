# Tilesets Reference (Top-Down & Sidescroller)

## Top-Down Tilesets

### create_topdown_tileset

Generates a 16-tile Wang tileset for overhead terrain with seamless corner combinations.

#### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `lower_description` | string | No | - | Primary terrain (e.g. "green grass", "sand", "water") |
| `upper_description` | string | No | - | Secondary terrain (e.g. "dirt path", "stone road") |
| `transition_description` | string | No | - | Decorative transition layer (e.g. "small stones", "flowers") |
| `transition_size` | float | No | - | Blend intensity 0.0-0.5 (higher = wider transition) |
| `tile_size` | object | No | - | `{"width": 32, "height": 32}` - tile pixel dimensions |
| `outline` | string | No | - | `"lineless"` or `"single color"` |
| `shading` | string | No | - | `"flat"`, `"selective outlining"`, `"full shading"` |
| `detail` | string | No | - | `"low"`, `"medium"`, `"high"` |
| `view` | string | No | - | `"high top-down"` or `"low top-down"` |
| `tile_strength` | float | No | - | Tile emphasis/contrast |
| `lower_base_tile_id` | string | No | - | UUID of previous tileset for visual consistency (lower layer) |
| `upper_base_tile_id` | string | No | - | UUID of previous tileset for visual consistency (upper layer) |
| `tileset_adherence` | float | No | - | How closely to match reference tileset pattern |
| `tileset_adherence_freedom` | float | No | - | Variation allowed from reference pattern |
| `text_guidance_scale` | float | No | - | How strongly the text description influences output |

#### Wang Tileset Concept
The 16 tiles cover all possible corner combinations of two terrain types meeting. This ensures any tile can seamlessly connect to any adjacent tile without visible seams - essential for procedural map generation.

#### Chaining Tilesets
To build a consistent tileset library:
1. Create your base terrain (e.g. grass)
2. Use the returned tileset ID as `lower_base_tile_id` in the next tileset
3. This ensures grass-to-dirt and grass-to-water transitions all share the same grass style

---

### get_topdown_tileset

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `tileset_id` | string | No | - | Tileset UUID |

Returns tileset data, generation status, tile images, and download links.

### list_topdown_tilesets

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `limit` | int | No | 10 | Results per page |
| `offset` | int | No | 0 | Pagination offset |

### delete_topdown_tileset

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `tileset_id` | string | No | - | Tileset UUID to delete |

---

## Sidescroller Tilesets

### create_sidescroller_tileset

Generates a 16-tile set for 2D platformer games with side-view perspective and transparent backgrounds.

#### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `lower_description` | string | No | - | Platform material (e.g. "mossy stone", "wooden planks", "ice") |
| `transition_description` | string | No | - | Surface decoration (e.g. "grass tufts", "snow", "vines") |
| `transition_size` | float | No | - | Decoration coverage 0.0-0.5 |
| `tile_size` | object | No | - | `{"width": 32, "height": 32}` |
| `outline` | string | No | - | `"lineless"` or `"single color"` |
| `shading` | string | No | - | `"flat"`, `"selective outlining"`, `"full shading"` |
| `detail` | string | No | - | `"low"`, `"medium"`, `"high"` |
| `tile_strength` | float | No | - | Material emphasis |
| `base_tile_id` | string | No | - | Previous tileset UUID for consistency |
| `tileset_adherence` | float | No | - | Pattern consistency with reference |
| `tileset_adherence_freedom` | float | No | - | Variation allowance |
| `text_guidance_scale` | float | No | - | Description influence weight |
| `seed` | int | No | - | For reproducible generation |

#### Key Difference from Top-Down
- Only one terrain type (`lower_description`) instead of two
- Transparent background (no `upper_description`)
- Designed for platformer physics (top surfaces, sides, bottoms, corners)

---

### get_sidescroller_tileset

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `tileset_id` | string | No | - | Tileset UUID |
| `include_example_map` | bool | No | false | Show a generated platform layout example |

### list_sidescroller_tilesets

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `limit` | int | No | 10 | Results per page |
| `offset` | int | No | 0 | Pagination offset |

### delete_sidescroller_tileset

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `tileset_id` | string | No | - | Tileset UUID to delete |
