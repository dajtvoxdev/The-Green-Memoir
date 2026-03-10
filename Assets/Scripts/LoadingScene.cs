using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Màn hình Loading: tự động tải dữ liệu Firebase rồi chuyển sang PlayScene.
/// </summary>
public class LoadingScene : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Thanh tiến trình hiển thị phần trăm tải dữ liệu")]
    public Slider progressBar;

    [Tooltip("Text hiển thị trạng thái tải")]
    public Text statusText;

    [Tooltip("Nút bấm dự phòng nếu tải quá lâu (tùy chọn)")]
    public Button buttonLoadDone;

    [Header("Settings")]
    [Tooltip("Thời gian tối thiểu hiển thị màn hình loading (tránh nhấp nháy)")]
    public float minLoadingTime = 2f;

    [Tooltip("Thời gian chờ tối đa trước khi báo lỗi timeout")]
    public float timeoutDuration = 30f;

    private float loadingStartTime;
    private bool dataLoaded = false;
    private bool loadingFailed = false;

    void Start()
    {
        loadingStartTime = Time.time;

        // Ẩn nút bấm dự phòng ban đầu (chỉ hiện khi lỗi hoặc timeout)
        if (buttonLoadDone != null)
        {
            buttonLoadDone.gameObject.SetActive(false);
            buttonLoadDone.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("PlayScene");
            });
        }

        // Cập nhật UI ban đầu
        UpdateUI(0f, "Đang kết nối máy chủ...");

        // Đăng ký lắng nghe sự kiện tải xong dữ liệu
        LoadDataManager.OnUserLoaded += OnUserDataLoaded;

        // Bắt đầu coroutine theo dõi tiến trình
        StartCoroutine(LoadingRoutine());
    }

    /// <summary>
    /// Callback khi LoadDataManager tải xong dữ liệu (thành công hoặc thất bại).
    /// </summary>
    private void OnUserDataLoaded(bool success)
    {
        if (success)
        {
            dataLoaded = true;
            UpdateUI(0.7f, "Dữ liệu đã sẵn sàng!");
        }
        else
        {
            loadingFailed = true;
            string errorMsg = LoadDataManager.LastErrorMessage ?? "Không xác định";
            UpdateUI(0f, $"Lỗi tải dữ liệu: {errorMsg}");
        }
    }

    /// <summary>
    /// Coroutine chính: theo dõi tiến trình tải dữ liệu và chuyển scene khi sẵn sàng.
    /// </summary>
    private IEnumerator LoadingRoutine()
    {
        float fakeProgress = 0f;

        // Giai đoạn 1: Chờ dữ liệu Firebase (0% -> 70%)
        while (!dataLoaded && !loadingFailed)
        {
            float elapsed = Time.time - loadingStartTime;

            // Kiểm tra timeout
            if (elapsed > timeoutDuration)
            {
                loadingFailed = true;
                UpdateUI(fakeProgress, "Hết thời gian chờ. Vui lòng thử lại.");
                ShowFallbackButton();
                yield break;
            }

            // Fake progress tăng dần để người chơi thấy đang tải (tối đa 60%)
            fakeProgress = Mathf.Min(0.6f, elapsed / timeoutDuration * 2f);
            UpdateUI(fakeProgress, "Đang tải dữ liệu người chơi...");

            yield return new WaitForSeconds(0.1f);
        }

        // Nếu tải thất bại, hiện nút bấm dự phòng
        if (loadingFailed)
        {
            ShowFallbackButton();
            yield break;
        }

        // Giai đoạn 2: Dữ liệu đã tải xong, đảm bảo thời gian loading tối thiểu
        float remainingMinTime = minLoadingTime - (Time.time - loadingStartTime);
        if (remainingMinTime > 0)
        {
            float startProgress = 0.7f;
            float timer = 0f;
            while (timer < remainingMinTime)
            {
                timer += Time.deltaTime;
                float p = startProgress + (timer / remainingMinTime) * 0.15f;
                UpdateUI(p, "Đang chuẩn bị thế giới...");
                yield return null;
            }
        }

        // Giai đoạn 3: Async load PlayScene (85% -> 100%)
        UpdateUI(0.85f, "Đang tải cảnh chơi...");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("PlayScene");
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            // Scene load progress: 0 -> 0.9 thì chờ allowSceneActivation
            float sceneProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            float mappedProgress = 0.85f + sceneProgress * 0.14f; // 85% -> 99%
            UpdateUI(mappedProgress, "Đang tải cảnh chơi...");

            if (asyncLoad.progress >= 0.9f)
            {
                UpdateUI(1f, "Hoàn tất!");
                yield return new WaitForSeconds(0.3f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Cập nhật giao diện progress bar và status text.
    /// </summary>
    private void UpdateUI(float progress, string status)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (statusText != null)
            statusText.text = status;
    }

    /// <summary>
    /// Hiển thị nút bấm dự phòng khi tải bị lỗi hoặc timeout.
    /// </summary>
    private void ShowFallbackButton()
    {
        if (buttonLoadDone != null)
        {
            buttonLoadDone.gameObject.SetActive(true);
        }
    }

    void OnDestroy()
    {
        // Hủy đăng ký event để tránh memory leak
        LoadDataManager.OnUserLoaded -= OnUserDataLoaded;
    }
}
