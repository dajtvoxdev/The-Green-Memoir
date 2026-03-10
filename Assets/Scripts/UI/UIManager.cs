using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton UI manager that controls all UI panels.
/// Uses a stack for panel navigation (open panel → push, close → pop).
///
/// Phase 2 Feature (#13): Centralized UI management.
///
/// Usage:
///   UIManager.Instance.ShowPanel("inventory");
///   UIManager.Instance.HidePanel("inventory");
///   UIManager.Instance.TogglePanel("shop");
///   UIManager.Instance.CloseTopPanel();  // Escape key
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [Tooltip("All UI panels in the game. Drag panel root GameObjects here.")]
    public PanelBase[] panels;

    /// <summary>
    /// Dictionary for O(1) panel lookup by panelId.
    /// </summary>
    private Dictionary<string, PanelBase> panelRegistry;

    /// <summary>
    /// Stack of currently open panels (most recent on top).
    /// </summary>
    private Stack<PanelBase> panelStack;

    /// <summary>
    /// Whether any panel is currently open.
    /// </summary>
    public bool AnyPanelOpen => panelStack != null && panelStack.Count > 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        BuildRegistry();
        panelStack = new Stack<PanelBase>();
    }

    /// <summary>
    /// Builds the panelId → PanelBase dictionary.
    /// </summary>
    private void BuildRegistry()
    {
        panelRegistry = new Dictionary<string, PanelBase>();

        if (panels == null) return;

        foreach (var panel in panels)
        {
            if (panel == null) continue;

            string id = panel.panelId;
            if (string.IsNullOrEmpty(id))
            {
                id = panel.gameObject.name.ToLower();
                panel.panelId = id;
            }

            if (panelRegistry.ContainsKey(id))
            {
                Debug.LogWarning($"UIManager: Duplicate panelId '{id}', skipping '{panel.gameObject.name}'.");
                continue;
            }

            panelRegistry[id] = panel;
        }

        Debug.Log($"UIManager: Registered {panelRegistry.Count} panels.");
    }

    void Update()
    {
        // Global Escape key handling: close the topmost panel
        if (Input.GetKeyDown(KeyCode.Escape) && panelStack.Count > 0)
        {
            CloseTopPanel();
        }
    }

    /// <summary>
    /// Shows a panel by its panelId.
    /// </summary>
    public void ShowPanel(string panelId)
    {
        if (!panelRegistry.TryGetValue(panelId, out PanelBase panel))
        {
            Debug.LogWarning($"UIManager: Panel '{panelId}' not found!");
            return;
        }

        if (panel.IsVisible) return;

        panel.Show();
        panelStack.Push(panel);
    }

    /// <summary>
    /// Hides a panel by its panelId.
    /// </summary>
    public void HidePanel(string panelId)
    {
        if (!panelRegistry.TryGetValue(panelId, out PanelBase panel))
        {
            Debug.LogWarning($"UIManager: Panel '{panelId}' not found!");
            return;
        }

        if (!panel.IsVisible) return;

        panel.Hide();
        RemoveFromStack(panel);
    }

    /// <summary>
    /// Toggles a panel's visibility.
    /// </summary>
    public void TogglePanel(string panelId)
    {
        if (!panelRegistry.TryGetValue(panelId, out PanelBase panel))
        {
            Debug.LogWarning($"UIManager: Panel '{panelId}' not found!");
            return;
        }

        if (panel.IsVisible)
        {
            HidePanel(panelId);
        }
        else
        {
            ShowPanel(panelId);
        }
    }

    /// <summary>
    /// Closes the topmost panel in the stack.
    /// Used by Escape key.
    /// </summary>
    public void CloseTopPanel()
    {
        if (panelStack.Count == 0) return;

        PanelBase top = panelStack.Pop();
        if (top != null && top.IsVisible)
        {
            top.Hide();
        }
    }

    /// <summary>
    /// Closes all open panels.
    /// </summary>
    public void CloseAllPanels()
    {
        while (panelStack.Count > 0)
        {
            PanelBase panel = panelStack.Pop();
            if (panel != null && panel.IsVisible)
            {
                panel.Hide();
            }
        }
    }

    /// <summary>
    /// Gets a panel by its panelId.
    /// </summary>
    public PanelBase GetPanel(string panelId)
    {
        panelRegistry.TryGetValue(panelId, out PanelBase panel);
        return panel;
    }

    /// <summary>
    /// Checks if a specific panel is currently visible.
    /// </summary>
    public bool IsPanelOpen(string panelId)
    {
        if (panelRegistry.TryGetValue(panelId, out PanelBase panel))
        {
            return panel.IsVisible;
        }
        return false;
    }

    /// <summary>
    /// Registers a panel at runtime (for dynamically created panels).
    /// </summary>
    public void RegisterPanel(PanelBase panel)
    {
        if (panel == null || string.IsNullOrEmpty(panel.panelId)) return;

        panelRegistry[panel.panelId] = panel;
    }

    /// <summary>
    /// Removes a panel from the stack without hiding it.
    /// </summary>
    private void RemoveFromStack(PanelBase panel)
    {
        // Rebuild stack without the removed panel
        var temp = new Stack<PanelBase>();
        while (panelStack.Count > 0)
        {
            var p = panelStack.Pop();
            if (p != panel)
            {
                temp.Push(p);
            }
        }
        while (temp.Count > 0)
        {
            panelStack.Push(temp.Pop());
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
