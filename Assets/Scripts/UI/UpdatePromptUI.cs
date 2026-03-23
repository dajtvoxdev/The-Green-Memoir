using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// In-game auto-update UI. Shows when the game version is outdated.
/// Downloads the latest installer from the release manifest URL,
/// displays a progress bar, then launches the installer and quits.
///
/// Flow:
///   1. FirebaseLoginManager detects version mismatch
///   2. Calls UpdatePromptUI.Show(currentVersion, latestVersion, downloadUrl)
///   3. User clicks "Update" → downloads installer with progress bar
///   4. Download complete → launches installer with /SILENT flag
///   5. Game quits so installer can replace files
/// </summary>
public class UpdatePromptUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Root panel — shown/hidden via CanvasGroup")]
    public GameObject panelRoot;

    [Tooltip("Title text (e.g., 'Cập nhật có sẵn!')")]
    public TMP_Text titleText;

    [Tooltip("Message text showing version info")]
    public TMP_Text messageText;

    [Tooltip("Progress bar slider (0-1)")]
    public Slider progressBar;

    [Tooltip("Progress percentage text")]
    public TMP_Text progressText;

    [Tooltip("Update button — starts download")]
    public Button updateButton;

    [Tooltip("Text on the update button")]
    public TMP_Text updateButtonText;

    [Tooltip("Cancel/close button — exits prompt")]
    public Button cancelButton;

    [Tooltip("Status text (downloading, installing, error)")]
    public TMP_Text statusText;

    private string _downloadUrl;
    private string _latestVersion;
    private bool _isDownloading;
    private Coroutine _downloadCoroutine;

    void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    /// <summary>
    /// Shows the update prompt with version info and download URL.
    /// Called by FirebaseLoginManager when version mismatch is detected.
    /// </summary>
    public void Show(string currentVersion, string latestVersion, string downloadUrl)
    {
        _downloadUrl = downloadUrl;
        _latestVersion = latestVersion;
        _isDownloading = false;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (titleText != null)
            titleText.text = LocalizationManager.LocalizeText("Cập nhật có sẵn!");

        if (messageText != null)
            messageText.text = LocalizationManager.LocalizeText(
                $"Phiên bản hiện tại: {currentVersion}\nPhiên bản mới nhất: {latestVersion}\n\nBạn cần cập nhật để tiếp tục chơi.");

        if (progressBar != null)
        {
            progressBar.value = 0f;
            progressBar.gameObject.SetActive(false);
        }

        if (progressText != null)
            progressText.gameObject.SetActive(false);

        if (statusText != null)
            statusText.text = "";

        if (updateButton != null)
        {
            updateButton.onClick.RemoveAllListeners();
            updateButton.onClick.AddListener(OnUpdateClicked);
            updateButton.interactable = !string.IsNullOrEmpty(downloadUrl);
        }

        if (updateButtonText != null)
            updateButtonText.text = LocalizationManager.LocalizeText("Cập nhật ngay");

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        if (string.IsNullOrEmpty(downloadUrl))
        {
            SetStatus("Không có link tải. Vui lòng cập nhật từ website.", Color.yellow);
        }
    }

    /// <summary>
    /// Hides the update prompt.
    /// </summary>
    public void Hide()
    {
        if (_downloadCoroutine != null)
        {
            StopCoroutine(_downloadCoroutine);
            _downloadCoroutine = null;
        }

        _isDownloading = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnUpdateClicked()
    {
        if (_isDownloading) return;

        if (string.IsNullOrEmpty(_downloadUrl))
        {
            SetStatus("Không có link tải.", Color.red);
            return;
        }

        _downloadCoroutine = StartCoroutine(DownloadAndInstall());
    }

    private void OnCancelClicked()
    {
        if (_isDownloading)
        {
            // Cancel download
            if (_downloadCoroutine != null)
            {
                StopCoroutine(_downloadCoroutine);
                _downloadCoroutine = null;
            }

            _isDownloading = false;
            SetStatus("Đã hủy tải xuống.", Color.yellow);

            if (updateButton != null) updateButton.interactable = true;
            if (updateButtonText != null)
                updateButtonText.text = LocalizationManager.LocalizeText("Thử lại");
            if (progressBar != null) progressBar.gameObject.SetActive(false);
            if (progressText != null) progressText.gameObject.SetActive(false);
            return;
        }

        Hide();
    }

    /// <summary>
    /// Downloads the installer and launches it when complete.
    /// </summary>
    private IEnumerator DownloadAndInstall()
    {
        _isDownloading = true;

        if (updateButton != null) updateButton.interactable = false;
        if (cancelButton != null)
        {
            var cancelText = cancelButton.GetComponentInChildren<TMP_Text>();
            if (cancelText != null)
                cancelText.text = LocalizationManager.LocalizeText("Hủy");
        }

        // Show progress UI
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = 0f;
        }
        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
            progressText.text = "0%";
        }

        SetStatus("Đang tải bản cập nhật...", Color.white);

        // Determine save path
        string normalizedVersion = NormalizeVersionLabel(_latestVersion);
        string fileName = $"Setup_MoonlitGarden_{normalizedVersion}.exe";
        string savePath = Path.Combine(Application.temporaryCachePath, fileName);

        // Clean up old download if exists
        if (File.Exists(savePath))
        {
            try { File.Delete(savePath); } catch { /* ignore */ }
        }

        // Download
        using (UnityWebRequest request = UnityWebRequest.Get(_downloadUrl))
        {
            request.downloadHandler = new DownloadHandlerFile(savePath) { removeFileOnAbort = true };
            request.timeout = 300; // 5 min timeout

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                float progress = request.downloadProgress;
                if (progressBar != null) progressBar.value = progress;
                if (progressText != null) progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";

                // Show downloaded size
                ulong downloaded = request.downloadedBytes;
                string sizeStr = FormatBytes(downloaded);
                SetStatus($"Đang tải... {sizeStr}", Color.white);

                yield return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorDetail = request.error;
                UnityEngine.Debug.LogError($"UpdatePromptUI: Download failed: {errorDetail}");
                SetStatus($"Tải thất bại: {errorDetail}", Color.red);

                _isDownloading = false;
                if (updateButton != null) updateButton.interactable = true;
                if (updateButtonText != null)
                    updateButtonText.text = LocalizationManager.LocalizeText("Thử lại");
                yield break;
            }
        }

        // Download complete
        if (progressBar != null) progressBar.value = 1f;
        if (progressText != null) progressText.text = "100%";

        if (!File.Exists(savePath))
        {
            SetStatus("Lỗi: File tải về không tìm thấy.", Color.red);
            _isDownloading = false;
            yield break;
        }

        FileInfo fi = new FileInfo(savePath);
        SetStatus($"Tải hoàn tất! ({FormatBytes((ulong)fi.Length)}) Đang khởi chạy cài đặt...", new Color(0.3f, 1f, 0.3f));

        yield return new WaitForSeconds(1f);

        // Launch installer and quit
        LaunchInstallerAndQuit(savePath);
    }

    /// <summary>
    /// Launches the Inno Setup installer with /SILENT flag and quits the game.
    /// /SILENT shows progress but no prompts. /VERYSILENT hides everything.
    /// Using /SILENT so user can see installation progress.
    /// </summary>
    private void LaunchInstallerAndQuit(string installerPath)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = "/SILENT /CLOSEAPPLICATIONS /RESTARTAPPLICATIONS",
                UseShellExecute = true
            };

            Process.Start(psi);
            UnityEngine.Debug.Log($"UpdatePromptUI: Launched installer at {installerPath}");

            // Quit game so installer can replace files
            Application.Quit();

#if UNITY_EDITOR
            UnityEngine.Debug.Log("UpdatePromptUI: In editor — would quit here in a real build.");
#endif
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"UpdatePromptUI: Failed to launch installer: {ex.Message}");
            SetStatus($"Không thể chạy bộ cài đặt: {ex.Message}", Color.red);
            _isDownloading = false;

            // Fallback: open folder containing installer
            try
            {
                string folder = Path.GetDirectoryName(installerPath);
                if (folder != null)
                    Process.Start("explorer.exe", $"/select,\"{installerPath}\"");
            }
            catch { /* ignore fallback error */ }
        }
    }

    private void SetStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = LocalizationManager.LocalizeText(message);
            statusText.color = color;
        }
    }

    private static string FormatBytes(ulong bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1048576) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / 1048576.0:F1} MB";
    }

    private static string NormalizeVersionLabel(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return "latest";
        }

        string trimmed = version.Trim();
        return trimmed.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : $"v{trimmed}";
    }
}
