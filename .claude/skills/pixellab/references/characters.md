# Characters & Animation Reference

## create_character

Generates a pixel art character with directional views (4 or 8 directions).

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `description` | string | No | - | Character appearance details (e.g. "a wizard with a purple robe and staff") |
| `name` | string | No | - | Character identifier/label |
| `body_type` | string | No | "humanoid" | `"humanoid"` or `"quadruped"` |
| `template` | string | No | - | Required for quadrupeds: `"bear"`, `"cat"`, `"dog"`, `"horse"`, `"lion"` |
| `n_directions` | int | No | 4 | Number of rotational views: `4` or `8` |
| `proportions` | string | No | - | Body shape preset (e.g. `"chibi"`, `"normal"`, `"tall"`) |
| `size` | int | No | 32 | Canvas pixel dimensions (e.g. 16, 32, 48, 64) |
| `outline` | string | No | - | `"lineless"` or `"single color"` |
| `shading` | string | No | - | `"flat"`, `"selective outlining"`, `"full shading"` |
| `detail` | string | No | - | `"low"`, `"medium"`, `"high"` |
| `ai_freedom` | float | No | - | 0.0-1.0, how creative the AI can be |
| `view` | string | No | - | `"3/4 top-down"`, `"high top-down"`, `"side"` |

### Tips
- For top-down RPG games, use `view: "3/4 top-down"` with `n_directions: 4`
- For action games, use `n_directions: 8` for smoother rotation
- `proportions: "chibi"` works great for small sprites (16-32px)
- Keep `ai_freedom` low (0.1-0.3) when you need predictable results

---

## animate_character

Queues an animation sequence for an existing character.

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `character_id` | string | No | - | UUID of the target character |
| `template_animation_id` | string | No | - | Animation type (e.g. `"walk"`, `"idle"`, `"attack"`, `"run"`, `"death"`) |
| `action_description` | string | No | - | Custom variation description (e.g. "swinging a sword overhead") |
| `animation_name` | string | No | - | Custom label for this animation |

### Tips
- Always create the character first and wait for it to complete before animating
- Use `template_animation_id` for standard animations (walk, idle, run, attack, death)
- Use `action_description` to customize the animation style
- Each animation is queued and processed asynchronously - use `get_character` to check status

---

## get_character

Retrieves complete character data including all rotations, animations, and download links.

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `character_id` | string | No | - | Character UUID to fetch |
| `include_preview` | bool | No | false | Include base64 preview images |

### Response Includes
- Character metadata (name, description, settings)
- All directional sprite views
- All animation sequences with frame data
- PNG download URLs for each asset

---

## list_characters

Paginated listing of all user-created characters.

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `limit` | int | No | 10 | Results per page |
| `offset` | int | No | 0 | Pagination starting point |
| `tags` | string | No | - | Filter by labels |

---

## delete_character

Permanently removes a character and all associated animations.

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `character_id` | string | No | - | Character UUID to remove |
| `confirm` | bool | No | - | Safety confirmation flag |

**Warning:** This action is irreversible. All animations linked to this character will also be deleted.
