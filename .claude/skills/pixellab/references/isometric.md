# Isometric Tiles Reference

## create_isometric_tile

Creates individual 3D-looking pixel art tiles with adjustable thickness for game assets.

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `description` | string | No | - | Tile appearance (e.g. "wooden crate", "stone floor", "lava pool") |
| `size` | int | No | 64 | Tile pixel dimension |
| `tile_shape` | string | No | - | Height profile: `"thin"` (flat), `"thick"` (raised), `"block"` (cube-like) |
| `outline` | string | No | - | `"lineless"` or `"single color"` |
| `shading` | string | No | - | `"flat"`, `"selective outlining"`, `"full shading"` |
| `detail` | string | No | - | `"low"`, `"medium"`, `"high"` |
| `text_guidance_scale` | float | No | - | How strongly description influences output |
| `seed` | int | No | - | For reproducible generation |

### Tile Shape Guide
- **thin**: Flat ground tiles (floors, roads, water surfaces)
- **thick**: Slightly raised tiles (platforms, raised terrain, shallow walls)
- **block**: Full cube height (walls, crates, buildings, tall objects)

### Tips
- Use `size: 64` or larger for isometric tiles - they need more detail than top-down
- Combine thin (ground) + block (walls) tiles for building interiors
- Use the same `outline` and `shading` across all tiles for consistent style
- Set a `seed` value to generate variations of the same tile

---

## get_isometric_tile

Retrieves tile data with PNG image and generation status.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `tile_id` | string | No | - | Tile UUID |

### Response Includes
- Tile metadata (description, settings)
- Generation status (queued, processing, completed, failed)
- PNG image data / download URL

---

## list_isometric_tiles

Paginated listing sorted by creation date (newest first).

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `limit` | int | No | 10 | Results per page |
| `offset` | int | No | 0 | Pagination offset |

---

## delete_isometric_tile

Permanently removes an isometric tile. Irreversible.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `tile_id` | string | No | - | Tile UUID to delete |
