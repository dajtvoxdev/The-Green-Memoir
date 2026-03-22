---
name: pixellab
description: "PixelLab AI pixel art generation via MCP. Create characters, animations, tilesets (top-down, sidescroller, isometric), map objects, and pro tiles for 2D game development."
---

# PixelLab MCP Skill

You are an expert 2D pixel art game asset creator. This skill enables you to generate pixel art assets using PixelLab's AI API through MCP tools.

## Prerequisites

1. PixelLab MCP server must be configured and connected
2. Valid API key (Bearer token) must be set in MCP config
3. MCP endpoint: `https://api.pixellab.ai/mcp`
4. Docs: `https://api.pixellab.ai/mcp/docs`

## Tool Overview (23 Tools)

| Category | Tools | Purpose |
|----------|-------|---------|
| **Characters** | `create_character`, `animate_character`, `get_character`, `list_characters`, `delete_character` | Pixel art characters with directional views & animations |
| **Top-Down Tilesets** | `create_topdown_tileset`, `get_topdown_tileset`, `list_topdown_tilesets`, `delete_topdown_tileset` | 16-tile Wang tilesets for overhead terrain |
| **Sidescroller Tilesets** | `create_sidescroller_tileset`, `get_sidescroller_tileset`, `list_sidescroller_tilesets`, `delete_sidescroller_tileset` | 16-tile sets for 2D platformers |
| **Isometric Tiles** | `create_isometric_tile`, `get_isometric_tile`, `list_isometric_tiles`, `delete_isometric_tile` | 3D-looking tiles with adjustable thickness |
| **Map Objects** | `create_map_object`, `get_map_object` | Pixel art objects with transparent backgrounds |
| **Pro Tiles** | `create_tiles_pro`, `get_tiles_pro`, `list_tiles_pro`, `delete_tiles_pro` | Advanced tiles with custom perspective & style references |

## Quick Start Examples

### Create a Character
```
Tool: create_character
Params:
  description: "a knight wearing silver armor with a blue cape"
  name: "Silver Knight"
  body_type: "humanoid"
  n_directions: 4
  proportions: "chibi"
  size: 32
  outline: "single color"
  shading: "selective outlining"
  detail: "medium"
  view: "3/4 top-down"
```

### Animate a Character
```
Tool: animate_character
Params:
  character_id: "<uuid from create_character>"
  template_animation_id: "walk"
  animation_name: "walk_cycle"
```

### Create Top-Down Tileset
```
Tool: create_topdown_tileset
Params:
  lower_description: "green grass"
  upper_description: "dirt path"
  transition_description: "small stones and pebbles"
  transition_size: 0.3
  tile_size: {"width": 32, "height": 32}
  view: "high top-down"
  outline: "single color"
  shading: "selective outlining"
  detail: "medium"
```

### Create Sidescroller Tileset
```
Tool: create_sidescroller_tileset
Params:
  lower_description: "mossy stone platform"
  transition_description: "grass tufts"
  transition_size: 0.2
  tile_size: {"width": 32, "height": 32}
  outline: "single color"
  shading: "selective outlining"
```

### Create Isometric Tile
```
Tool: create_isometric_tile
Params:
  description: "wooden crate"
  size: 64
  tile_shape: "block"
  outline: "single color"
  shading: "selective outlining"
  detail: "medium"
```

### Create Map Object
```
Tool: create_map_object
Params:
  description: "a large oak tree with autumn leaves"
  width: 64
  height: 96
  view: "3/4 top-down"
  outline: "single color"
  shading: "selective outlining"
```

### Create Pro Tiles
```
Tool: create_tiles_pro
Params:
  description: "stone castle wall"
  tile_type: "isometric"
  tile_size: 64
  tile_height: 32
  n_tiles: 4
  tile_view: "isometric"
```

## Workflow: Building a Complete Game Tileset

1. **Start with terrain** - Use `create_topdown_tileset` for base terrain (grass, water, dirt)
2. **Chain tilesets** - Pass `lower_base_tile_id` from step 1 to ensure visual consistency
3. **Add objects** - Use `create_map_object` for trees, rocks, buildings
4. **Create characters** - Use `create_character` with matching art style
5. **Animate** - Use `animate_character` for walk, idle, attack cycles
6. **Download** - Use `get_*` tools to retrieve PNG download links

## Key Concepts

### Wang Tilesets (16 tiles)
Top-down tilesets use the Wang tile algorithm for seamless corner combinations. Any tile connects to any adjacent tile without visible seams.

### Tileset Chaining
Use `lower_base_tile_id` / `upper_base_tile_id` / `base_tile_id` to reference previous tilesets, ensuring all terrain types share the same art style.

### AI Freedom (0.0 - 1.0)
Controls how literally the AI follows your description:
- **Low (0.0-0.3)**: Faithful to prompt, predictable results
- **Medium (0.4-0.6)**: Balanced creativity
- **High (0.7-1.0)**: More artistic variation, surprises

### Common Style Parameters
These appear across most creation tools:
- `outline`: "lineless", "single color"
- `shading`: "flat", "selective outlining", "full shading"
- `detail`: "low", "medium", "high"
- `view`: "3/4 top-down", "high top-down", "side"

## Detailed References

- [Characters & Animation](references/characters.md)
- [Top-Down & Sidescroller Tilesets](references/tilesets.md)
- [Isometric Tiles](references/isometric.md)
- [Map Objects & Pro Tiles](references/map-objects.md)
