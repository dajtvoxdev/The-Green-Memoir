using UnityEngine;

/// <summary>
/// A single line of NPC dialogue with speaker name, text body, and optional portrait.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("NPC name shown above the text box.")]
    public string speakerName;

    [Tooltip("The dialogue text content.")]
    [TextArea(2, 5)]
    public string text;

    [Tooltip("Optional portrait sprite shown beside the text.")]
    public Sprite portrait;
}

/// <summary>
/// ScriptableObject containing a sequence of dialogue lines for an NPC.
///
/// Phase 3 Feature (#31): NPC System + Dialogue.
///
/// Create: Right-click > MoonlitGarden > Dialogue
/// Assign to a DialogueNPC component in the scene.
/// </summary>
[CreateAssetMenu(fileName = "Dialogue_", menuName = "MoonlitGarden/Dialogue")]
public class DialogueDefinition : ScriptableObject
{
    [Tooltip("All lines in this conversation, shown sequentially.")]
    public DialogueLine[] lines;
}
