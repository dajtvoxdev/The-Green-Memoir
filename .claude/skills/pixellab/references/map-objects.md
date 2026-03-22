# Map Objects & Pro Tiles Reference

## Map Objects

### create_map_object

Generates a pixel art object with transparent background for use in game maps (trees, rocks, buildings, items, etc.).

#### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `description` | string | No | - | Object appearance (e.g. "a large oak tree with autumn leaves") |
| `width` | int | No | - | Object width in pixels |
| `height` | int | No | - | Object height in pixels |
| `view` | string | No | - | `"3/4 top-down"`, `"high top-down"`, `"side"` |
| `outline` | string | No | - | `"lineless"` or `"single color"` |
| `shading` | string | No | - | `"flat"`, `"selective outlining"`, `"full shading"` |
| `detail` | string | No | - | `"low"`, `"medium"`, `"high"` |
| `background_image` | image data | No | - | Reference image for context |
| `inpainting` | object | No | - | Modification/editing instructions for existing objects |

#### Tips
- Match `view` with your tileset view for visual consistency
- Use `width` and `height` independently - objects don't need to be square
- For trees/buildings, make `height` larger than `width`
- Use `background_image` to place objects in context (e.g. on your tileset)
- Use `inpainting` to edit/modify parts of an existing generated object

---

### get_map_object

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `object_id` | string | No | - | Object UUID |

Returns object data, processing status, and PNG download link.

---

## Pro Tiles (Advanced)

### create_tiles_pro

Advanced tile creation with customizable perspective, style references, and multi-tile batch generation.

#### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `description` | string | No | - | Tile description (e.g. "stone castle wall", "forest floor") |
| `tile_type` | string | No | - | Tile category (e.g. `"isometric"`) |
| `tile_size` | int | No | - | Base tile dimension in pixels |
| `tile_height` | int | No | - | Vertical dimension (for 3D tiles) |
| `n_tiles` | int | No | 1 | Number of tile variations to generate |
| `tile_view` | string | No | - | Camera perspective type |
| `tile_view_angle` | float | No | - | Rotation angle in degrees |
| `tile_depth_ratio` | float | No | - | Depth perception ratio |
| `seed` | int | No | - | For reproducible generation |
| `style_images` | list | No | - | Reference images for style matching |
| `style_options` | object | No | - | Fine-tuned style parameters |

#### When to Use Pro Tiles vs Standard Tools
- **Standard isometric**: Simple single tiles with basic options
- **Pro tiles**: Custom angles, batch generation, style references, advanced perspective control

#### Tips
- Use `n_tiles > 1` to generate multiple variations in one call
- Use `style_images` to match existing art in your game
- Combine `tile_view_angle` and `tile_depth_ratio` for custom isometric angles
- Pro tiles support more perspective options than standard isometric tiles

---

### get_tiles_pro

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `tile_id` | string | No | - | Pro tiles UUID |

Returns all generated tile variations, status, and download links.

### list_tiles_pro

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `limit` | int | No | 10 | Results per page |
| `offset` | int | No | 0 | Pagination offset |

### delete_tiles_pro

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `tile_id` | string | No | - | Pro tiles UUID to delete |
