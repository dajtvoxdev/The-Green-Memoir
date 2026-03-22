using UnityEngine;

/// <summary>
/// NPC component that triggers a dialogue sequence when the player
/// presses E within the BoxCollider2D trigger area.
///
/// Phase 3 Feature (#31): NPC System + Dialogue.
///
/// Setup:
///   1. Create a GameObject with SpriteRenderer (NPC sprite)
///   2. Add BoxCollider2D → set isTrigger = true, size ~(2, 2)
///   3. Attach this script, assign dialogueDefinition in Inspector
///   4. Player must have tag "Player" and a Collider2D
/// </summary>
public class DialogueNPC : MonoBehaviour
{
    [Header("Dialogue")]
    [Tooltip("The conversation to display when interacting.")]
    public DialogueDefinition dialogueDefinition;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("World-space prompt text shown when player is in range.")]
    public string interactPrompt = "Nhấn [E] để nói chuyện";

    private bool       _playerInRange;
    private GameObject _promptUI;

    void Update()
    {
        if (!_playerInRange) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive) return;

        if (Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (dialogueDefinition == null)
        {
            NotificationManager.Instance?.ShowNotification("...", 1f);
            return;
        }

        AudioManager.Instance?.PlaySFX("ui_click");
        DialogueManager.Instance?.StartDialogue(dialogueDefinition);
        HidePrompt();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        HidePrompt();
    }

    private void ShowPrompt()
    {
        if (_promptUI == null) _promptUI = CreateWorldPrompt();
        _promptUI.SetActive(true);
    }

    private void HidePrompt()
    {
        if (_promptUI != null) _promptUI.SetActive(false);
    }

    private GameObject CreateWorldPrompt()
    {
        var go = new GameObject("DialoguePrompt");
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(0, 1.5f, 0);

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = UnityEngine.RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        var rt = go.GetComponent<UnityEngine.RectTransform>();
        rt.sizeDelta = new Vector2(3f, 0.5f);
        rt.localScale = Vector3.one * 0.01f;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);

        var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = interactPrompt;
        tmp.fontSize  = 24;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color     = Color.yellow;

        var textRT = textGO.GetComponent<UnityEngine.RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return go;
    }

    void OnDestroy()
    {
        if (_promptUI != null) Destroy(_promptUI);
    }
}
