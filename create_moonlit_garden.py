#!/usr/bin/env python3
"""
Create Moonlit Garden Scene for the Field project.

Prerequisites:
1. Open Unity project in Unity Editor
2. Install UnitySkills plugin: Window > Package Manager > + > Add package from git URL:
   https://github.com/Besty0728/Unity-Skills.git?path=/SkillsForUnity
3. Start the server: Window > UnitySkills > Start Server
4. Run: python create_moonlit_garden.py
"""

import sys
import os

# Add the skill's scripts directory to path
SKILL_SCRIPTS = os.path.join(
    os.path.dirname(__file__),
    ".claude", "skills", "besty0728-unity-skills-unity-skills", "scripts"
)
sys.path.insert(0, SKILL_SCRIPTS)

import unity_skills
import time

SCENE_PATH = "Assets/Scenes/MoonlitGarden.unity"

def wait(msg, seconds=1.5):
    print(f"  ⏳ {msg}...")
    time.sleep(seconds)


def check_connection():
    print("🔌 Connecting to Unity Editor...")
    try:
        client = unity_skills.UnitySkills()
        result = client.call("editor_get_state")
        if result.get("success"):
            print(f"  ✅ Connected! isPlaying={result.get('isPlaying')}, isCompiling={result.get('isCompiling')}")
            return True
    except ConnectionError as e:
        print(f"  ❌ {e}")
        print("\n  Make sure:")
        print("  1. Unity Editor has the project open")
        print("  2. UnitySkills plugin is installed")
        print("  3. Server is started: Window > UnitySkills > Start Server")
        return False


def create_scene():
    print(f"\n📁 Creating scene: {SCENE_PATH}")
    result = unity_skills.call_skill("scene_create", scenePath=SCENE_PATH)
    if not result.get("success"):
        print(f"  ❌ {result.get('error')}")
        return False
    print(f"  ✅ Scene created")
    return True


def setup_camera():
    print("\n📷 Setting up Main Camera (moonlit 2D view)...")
    # The scene_create gives us a default camera — configure it
    result = unity_skills.call_skill("component_get_properties", name="Main Camera", componentType="Camera")
    if result.get("success"):
        # Set orthographic size for 2D
        unity_skills.call_skill("component_set_property",
            name="Main Camera", componentType="Camera",
            propertyName="orthographicSize", value=5.0)
        # Set background to deep night blue (not black)
        unity_skills.call_skill("component_set_property",
            name="Main Camera", componentType="Camera",
            propertyName="backgroundColor.r", value=0.05)
        unity_skills.call_skill("component_set_property",
            name="Main Camera", componentType="Camera",
            propertyName="backgroundColor.g", value=0.05)
        unity_skills.call_skill("component_set_property",
            name="Main Camera", componentType="Camera",
            propertyName="backgroundColor.b", value=0.15)
        # Position camera above scene center
        unity_skills.call_skill("gameobject_set_transform",
            name="Main Camera", posX=0, posY=0, posZ=-10)
        print("  ✅ Camera configured")
    else:
        print("  ⚠️  Camera not found by default name, skipping detailed config")


def setup_lighting():
    print("\n💡 Setting up moonlit lighting...")
    # Create directional light (moonlight — cool blue, soft)
    result = unity_skills.call_skill("light_create",
        name="Moonlight",
        lightType="Directional",
        x=0, y=5, z=-3,
        r=0.6, g=0.7, b=1.0,   # cool blue-white moonlight
        intensity=0.35,
        shadows="soft")
    if result.get("success"):
        print("  ✅ Moonlight (directional, cool blue) created")
    else:
        print(f"  ⚠️  {result.get('error', 'Could not create moonlight')}")

    # Ambient fill light — very dim warm, simulates distant lanterns
    result = unity_skills.call_skill("light_create",
        name="AmbientFill",
        lightType="Point",
        x=0, y=0, z=0,
        r=1.0, g=0.85, b=0.5,   # warm amber
        intensity=0.12,
        range=20.0,
        shadows="none")
    if result.get("success"):
        print("  ✅ Ambient fill light (warm) created")


def setup_tilemap_grid():
    print("\n🗺️  Setting up Tilemap Grid (Ground / Grass / Forest layers)...")

    # Create the Grid parent object
    result = unity_skills.call_skill("gameobject_create",
        name="Grid", primitiveType="Empty")
    if not result.get("success"):
        print(f"  ⚠️  Grid: {result.get('error')}")

    # Add Grid component
    unity_skills.call_skill("component_add", name="Grid", componentType="Grid")
    print("  ✅ Grid object created")

    # Create three tilemap layers under Grid
    layers = [
        ("tm_Ground", "Ground layer — the base terrain"),
        ("tm_Grass",  "Grass layer — farmable grass tiles"),
        ("tm_Forest", "Forest layer — planted/grown items"),
    ]
    for obj_name, description in layers:
        result = unity_skills.call_skill("gameobject_create",
            name=obj_name, primitiveType="Empty", parentName="Grid")
        if result.get("success"):
            unity_skills.call_skill("component_add",
                name=obj_name, componentType="Tilemap")
            unity_skills.call_skill("component_add",
                name=obj_name, componentType="TilemapRenderer")
            print(f"  ✅ {obj_name} ({description})")
        else:
            print(f"  ⚠️  {obj_name}: {result.get('error')}")


def create_player():
    print("\n🧑 Creating Player GameObject...")

    # Player root (Empty)
    result = unity_skills.call_skill("gameobject_create",
        name="Player", primitiveType="Empty",
        x=0, y=0, z=0)
    if not result.get("success"):
        print(f"  ⚠️  {result.get('error')}")
        return

    # Add SpriteRenderer
    unity_skills.call_skill("component_add",
        name="Player", componentType="SpriteRenderer")

    # Add Rigidbody2D (kinematic for top-down movement)
    unity_skills.call_skill("component_add",
        name="Player", componentType="Rigidbody2D")
    unity_skills.call_skill("component_set_property",
        name="Player", componentType="Rigidbody2D",
        propertyName="bodyType", value=1)  # 1 = Kinematic

    # Add BoxCollider2D
    unity_skills.call_skill("component_add",
        name="Player", componentType="BoxCollider2D")

    # Add Animator — assign the existing Player controller
    unity_skills.call_skill("component_add",
        name="Player", componentType="Animator")

    ctrl_path = "Assets/Animations/Player .controller"
    r = unity_skills.call_skill("component_set_property",
        name="Player", componentType="Animator",
        propertyName="runtimeAnimatorController", value=ctrl_path)
    if r.get("success"):
        print("  ✅ Player animator controller assigned")

    # Add the movement script
    unity_skills.call_skill("component_add",
        name="Player", componentType="PlayerMovement")

    # Add farming controller
    unity_skills.call_skill("component_add",
        name="Player", componentType="PlayerFarmController")

    print("  ✅ Player created with movement + farming components")


def create_managers():
    print("\n⚙️  Creating scene managers...")

    managers = [
        ("DatabaseManager", "FirebaseDatabaseManager"),
        ("InventoryManager", "RecyclableInventoryManager"),
        ("TileMapManager",  "TileMapManager"),
    ]
    for go_name, component in managers:
        r = unity_skills.call_skill("gameobject_create",
            name=go_name, primitiveType="Empty")
        if r.get("success"):
            unity_skills.call_skill("component_add",
                name=go_name, componentType=component)
            print(f"  ✅ {go_name} ({component})")
        else:
            print(f"  ⚠️  {go_name}: {r.get('error')}")


def create_ui():
    print("\n🖼️  Creating UI Canvas...")

    # Screen-space overlay canvas
    r = unity_skills.call_skill("ui_create_canvas",
        name="UICanvas", renderMode="ScreenSpaceOverlay")
    if r.get("success"):
        print("  ✅ Canvas created")

    # HUD panel (top bar for game info)
    unity_skills.call_skill("ui_create_panel",
        name="HUDPanel", parent="UICanvas",
        r=0.0, g=0.0, b=0.0, a=0.6,
        width=800, height=60)

    # Garden label
    unity_skills.call_skill("ui_create_text",
        name="GardenTitle", parent="HUDPanel",
        text="🌙 Moonlit Garden",
        fontSize=24,
        r=0.9, g=0.9, b=1.0, a=1.0,
        width=400, height=50)

    # Controls hint text
    unity_skills.call_skill("ui_create_text",
        name="ControlsHint", parent="UICanvas",
        text="[C] Clear  [V] Plant  [M] Harvest",
        fontSize=14,
        r=0.7, g=0.8, b=1.0, a=0.8,
        width=400, height=30,
        x=0, y=-250)

    print("  ✅ UI created (HUD panel, title, controls hint)")


def create_decorative_lights():
    print("\n✨ Adding decorative lantern point lights...")

    lantern_positions = [
        ("Lantern_NW", -5, 3),
        ("Lantern_NE",  5, 3),
        ("Lantern_SW", -5, -3),
        ("Lantern_SE",  5, -3),
    ]
    items = [
        {"name": name, "lightType": "Point",
         "x": x, "y": y, "z": 0,
         "r": 1.0, "g": 0.8, "b": 0.3,
         "intensity": 0.8, "range": 4.0, "shadows": "none"}
        for name, x, y in lantern_positions
    ]
    result = unity_skills.call_skill("light_create",
        name="Lantern_NW", lightType="Point",
        x=-5, y=3, z=0,
        r=1.0, g=0.8, b=0.3,
        intensity=0.8, range=4.0, shadows="none")

    # Use batch for remaining lanterns
    batch_items = [
        {"name": "Lantern_NE", "lightType": "Point",
         "x": 5, "y": 3, "z": 0,
         "r": 1.0, "g": 0.8, "b": 0.3,
         "intensity": 0.8, "range": 4.0, "shadows": "none"},
        {"name": "Lantern_SW", "lightType": "Point",
         "x": -5, "y": -3, "z": 0,
         "r": 1.0, "g": 0.8, "b": 0.3,
         "intensity": 0.8, "range": 4.0, "shadows": "none"},
        {"name": "Lantern_SE", "lightType": "Point",
         "x": 5, "y": -3, "z": 0,
         "r": 1.0, "g": 0.8, "b": 0.3,
         "intensity": 0.8, "range": 4.0, "shadows": "none"},
    ]
    for item in batch_items:
        unity_skills.call_skill("light_create", **item)

    print("  ✅ 4 lantern lights placed at garden corners")


def verify_and_save():
    print("\n🔍 Verifying scene...")
    info = unity_skills.call_skill("scene_get_info")
    if info.get("success"):
        print(f"  Scene: {info.get('name')}")
        print(f"  Root objects: {info.get('rootObjectCount')}")
        objs = info.get("rootObjects", [])
        for o in objs:
            print(f"    - {o}")

    print("\n💾 Saving scene...")
    result = unity_skills.call_skill("scene_save")
    if result.get("success"):
        print(f"  ✅ Saved to {SCENE_PATH}")
    else:
        print(f"  ⚠️  {result.get('error')}")

    print("\n📸 Taking screenshot...")
    result = unity_skills.call_skill("scene_screenshot",
        filename="moonlit_garden_preview.png",
        width=1920, height=1080)
    if result.get("success"):
        print(f"  ✅ Screenshot saved")


def main():
    print("=" * 60)
    print("  🌙 Moonlit Garden Scene Builder")
    print("  Field (Moonlit Garden) — Unity 2D RPG Farming Game")
    print("=" * 60)

    if not check_connection():
        sys.exit(1)

    with unity_skills.WorkflowContext("CreateMoonlitGarden",
                                      "Build the Moonlit Garden scene from scratch"):
        if not create_scene():
            sys.exit(1)

        setup_camera()
        wait("Camera configured")

        setup_lighting()
        wait("Lighting configured")

        setup_tilemap_grid()
        wait("Tilemap grid ready")

        create_managers()
        wait("Managers ready")

        create_player()
        wait("Player ready", seconds=5)   # wait for script recompile if needed

        create_ui()
        wait("UI ready")

        create_decorative_lights()
        wait("Lights placed")

        verify_and_save()

    print("\n" + "=" * 60)
    print("  ✅ Moonlit Garden scene created successfully!")
    print(f"  📂 {SCENE_PATH}")
    print()
    print("  Next steps:")
    print("  1. Open the scene in Unity Editor")
    print("  2. Assign tiles to tm_Ground, tm_Grass, tm_Forest Tilemaps")
    print("     (use the DemoTilemap prefab or Tiny RPG Forest tiles)")
    print("  3. Link PlayerFarmController references in Inspector")
    print("     (tm_Ground, tm_Grass, tm_Forest, TileMapManager)")
    print("  4. Add the scene to Build Settings")
    print("=" * 60)


if __name__ == "__main__":
    main()
