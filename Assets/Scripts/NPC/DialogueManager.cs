using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages NPC dialogue display: typewriter effect, line progression, and panel lifecycle.
/// Pauses gameplay (Time.timeScale = 0) while a conversation is active.
///
/// Phase 3 Feature (#31): NPC System + Dialogue.
///
/// Usage:
///   DialogueManager.Instance.StartDialogue(definition);
///   // Player presses E → AdvanceLine() auto-called via update
///
/// Wire in Inspector: panelRoot, speakerNameText, dialogueText, portraitImage, continuePrompt.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Root panel GameObject — shown/hidden per conversation.")]
    public GameObject panelRoot;

    [Tooltip("Displays the NPC's name.")]
    public TMP_Text speakerNameText;

    [Tooltip("Displays the dialogue body with typewriter effect.")]
    public TMP_Text dialogueText;

    [Tooltip("Optional: portrait image beside text. Hidden if no Sprite.")]
    public Image portraitImage;

    [Tooltip("'Press E to continue' prompt — hidden while typing.")]
    public GameObject continuePrompt;

    [Header("Typewriter")]
    [Tooltip("Characters revealed per second.")]
    public float typewriterSpeed = 40f;

    [Header("Keys")]
    public KeyCode advanceKey = KeyCode.E;

    /// <summary>Fired when a dialogue sequence starts.</summary>
    public event Action OnDialogueStarted;

    /// <summary>Fired when a dialogue sequence ends.</summary>
    public event Action OnDialogueEnded;

    public bool IsActive { get; private set; }

    private DialogueLine[] _lines;
    private int            _currentLine;
    private Coroutine      _typewriterCoroutine;
    private bool           _lineComplete;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        SetPanelVisible(false);
    }

    void Update()
    {
        if (!IsActive) return;

        if (Input.GetKeyDown(advanceKey))
        {
            AdvanceLine();
        }
    }

    // ==================== PUBLIC API ====================

    /// <summary>Starts a dialogue sequence. Pauses game time.</summary>
    public void StartDialogue(DialogueDefinition definition)
    {
        if (definition == null || definition.lines == null || definition.lines.Length == 0)
        {
            Debug.LogWarning("DialogueManager: Definition is empty.");
            return;
        }

        if (IsActive) EndDialogue();

        _lines       = definition.lines;
        _currentLine = 0;
        IsActive     = true;

        Time.timeScale = 0f;
        SetPanelVisible(true);
        OnDialogueStarted?.Invoke();

        ShowLine(_currentLine);
    }

    /// <summary>Advances to next line or ends the conversation.</summary>
    public void AdvanceLine()
    {
        // If still typing — skip to full line
        if (!_lineComplete)
        {
            SkipTypewriter();
            return;
        }

        _currentLine++;
        if (_currentLine >= _lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine(_currentLine);
        }
    }

    // ==================== PRIVATE ====================

    private void ShowLine(int index)
    {
        DialogueLine line = _lines[index];

        if (speakerNameText != null)
            speakerNameText.text = line.speakerName;

        if (portraitImage != null)
        {
            bool hasPortrait = line.portrait != null;
            portraitImage.gameObject.SetActive(hasPortrait);
            if (hasPortrait) portraitImage.sprite = line.portrait;
        }

        SetContinuePromptVisible(false);
        _lineComplete = false;

        if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterCoroutine(line.text));
    }

    private IEnumerator TypewriterCoroutine(string fullText)
    {
        if (dialogueText != null) dialogueText.text = "";

        int charIndex = 0;
        float interval = typewriterSpeed > 0 ? 1f / typewriterSpeed : 0f;

        while (charIndex < fullText.Length)
        {
            charIndex++;
            if (dialogueText != null)
                dialogueText.text = fullText.Substring(0, charIndex);

            yield return new WaitForSecondsRealtime(interval);
        }

        _lineComplete = true;
        SetContinuePromptVisible(true);
    }

    private void SkipTypewriter()
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }

        if (dialogueText != null && _lines != null)
            dialogueText.text = _lines[_currentLine].text;

        _lineComplete = true;
        SetContinuePromptVisible(true);
    }

    private void EndDialogue()
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }

        IsActive = false;
        Time.timeScale = 1f;
        SetPanelVisible(false);
        OnDialogueEnded?.Invoke();
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelRoot != null) panelRoot.SetActive(visible);
    }

    private void SetContinuePromptVisible(bool visible)
    {
        if (continuePrompt != null) continuePrompt.SetActive(visible);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            // Always restore time scale on destroy
            Time.timeScale = 1f;
        }
    }
}
