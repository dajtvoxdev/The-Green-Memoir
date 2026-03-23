using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FirebaseLoginManager : MonoBehaviour
{
    [Serializable]
    private class GoogleOAuthRuntimeConfig
    {
        public string clientId;
        public string clientSecret;
    }

    [Header("Register")]
    public InputField ipRegisterEmail;
    public InputField ipRegisterPassword;
    public Button buttonRegister;

    [Header("Sign In")]
    public InputField ipLoginEmail;
    public InputField ipLoginPassword;
    public Button buttonLogin;

    [Header("Quick Login")]
    public Button buttonLoginGoogle;

    [Header("Google OAuth (Desktop)")]
    [Tooltip("Desktop App OAuth Client ID from Google Cloud Console")]
    public string googleClientId = "";

    [Tooltip("Used only during token exchange for Google OAuth in the current project setup.")]
    public string googleClientSecret = "";

    [Header("Switch form")]
    public Button buttonMoveToSignIn;
    public Button buttonMoveToRegister;

    public GameObject loginForm;
    public GameObject registerForm;

    [Header("Status Message")]
    [Tooltip("Text element to show login/register error messages.")]
    public TMP_Text statusText;

    [Header("Release Sync")]
    [Tooltip("Endpoint on the web repo that returns the current release manifest.")]
    public string releaseManifestUrl = "https://the-green-memoir-web.vercel.app/api/game/release";

    [Tooltip("When enabled, the login scene fetches the latest release info from the website.")]
    public bool checkLatestReleaseOnStart = true;

    [Header("Auto-Update")]
    [Tooltip("Reference to the UpdatePromptUI panel in the scene.")]
    public UpdatePromptUI updatePromptUI;

    private FirebaseAuth auth;
    private FirebaseDatabaseManager databaseManager;
    private DatabaseReference databaseReference;
    private GameReleaseManifest latestRelease;

    private void Start()
    {
        LoadGoogleOAuthRuntimeConfig();
        Debug.Log($"FirebaseLoginManager: Google OAuth config ready. clientIdSet={!string.IsNullOrWhiteSpace(googleClientId)}, clientSecretSet={!string.IsNullOrWhiteSpace(googleClientSecret)}");

        auth = FirebaseAuth.DefaultInstance;
        buttonRegister.onClick.AddListener(RegisterAccountWithFireBase);
        buttonLogin.onClick.AddListener(SignInAccountWithFireBase);

        if (buttonLoginGoogle != null)
        {
            buttonLoginGoogle.onClick.AddListener(SignInWithGoogle);
        }

        buttonMoveToSignIn.onClick.AddListener(SwitchForm);
        buttonMoveToRegister.onClick.AddListener(SwitchForm);

        databaseManager = GetComponent<FirebaseDatabaseManager>();
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        if (checkLatestReleaseOnStart)
        {
            StartCoroutine(FetchLatestReleaseManifest());
        }
    }

    private void LoadGoogleOAuthRuntimeConfig()
    {
        try
        {
            string configPath = Path.Combine(Application.streamingAssetsPath, "google-oauth-config.json");
            if (!File.Exists(configPath))
            {
                return;
            }

            string json = File.ReadAllText(configPath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            GoogleOAuthRuntimeConfig config = JsonUtility.FromJson<GoogleOAuthRuntimeConfig>(json);
            if (config == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(config.clientId))
            {
                googleClientId = config.clientId.Trim();
            }

            if (!string.IsNullOrWhiteSpace(config.clientSecret))
            {
                googleClientSecret = config.clientSecret.Trim();
            }

            Debug.Log($"FirebaseLoginManager: Loaded Google OAuth config from {configPath}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"FirebaseLoginManager: Failed to load Google OAuth runtime config: {ex.Message}");
        }
    }

    public void RegisterAccountWithFireBase()
    {
        string email = ipRegisterEmail.text;
        string password = ipRegisterPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Vui lòng nhập email và mật khẩu.", Color.red);
            return;
        }

        if (password.Length < 6)
        {
            ShowStatus("Mật khẩu phải có ít nhất 6 ký tự.", Color.red);
            return;
        }

        ShowStatus("Đang đăng ký...", Color.white);

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                ShowStatus("Đăng ký bị hủy.", Color.yellow);
                return;
            }

            if (task.IsFaulted)
            {
                string errorMsg = ParseFirebaseError(task.Exception);
                ShowStatus(errorMsg, Color.red);
                Debug.LogWarning($"Register failed: {task.Exception}");
                return;
            }

            if (!task.IsCompleted)
            {
                return;
            }

            ShowStatus("Đăng ký thành công! Đang kiểm tra quyền truy cập...", new Color(0.3f, 1f, 0.3f));

            Map mapInGame = new Map();
            User userInGame = new User("", 100, 50, mapInGame);

            FirebaseUser firebaseUser = task.Result.User;
            Debug.Log("Firebase user: " + firebaseUser);

            SeedNewUserProfile(firebaseUser, userInGame);
            StartCoroutine(ValidateLatestVersionAndEnter(firebaseUser));
        });
    }

    public void SignInAccountWithFireBase()
    {
        string email = ipLoginEmail.text;
        string password = ipLoginPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Vui lòng nhập email và mật khẩu.", Color.red);
            return;
        }

        ShowStatus("Đang đăng nhập...", Color.white);

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                ShowStatus("Đăng nhập bị hủy.", Color.yellow);
                return;
            }

            if (task.IsFaulted)
            {
                string errorMsg = ParseFirebaseError(task.Exception);
                ShowStatus(errorMsg, Color.red);
                Debug.LogWarning($"Login failed: {task.Exception}");
                return;
            }

            if (!task.IsCompleted)
            {
                return;
            }

            Debug.Log("Đăng nhập thành công");
            FirebaseUser user = task.Result.User;
            Debug.Log("UID: " + user.UserId);

            ShowStatus("Đăng nhập thành công! Đang kiểm tra phiên bản...", new Color(0.3f, 1f, 0.3f));
            StartCoroutine(ValidateLatestVersionAndEnter(user));
        });
    }

    public void SwitchForm()
    {
        loginForm.SetActive(!loginForm.activeSelf);
        registerForm.SetActive(!registerForm.activeSelf);
    }

    public void SignInWithGoogle()
    {
        if (string.IsNullOrEmpty(googleClientId))
        {
            ShowStatus("Chưa cấu hình Google Desktop Client ID.", Color.red);
            return;
        }

        if (string.IsNullOrWhiteSpace(googleClientSecret))
        {
            Debug.LogWarning("FirebaseLoginManager: Google client secret is empty before token exchange.");
        }

        ShowStatus("Đang mở trình duyệt để đăng nhập Google...", Color.white);
        StartCoroutine(GoogleOAuthDesktopFlow());
    }

    private IEnumerator GoogleOAuthDesktopFlow()
    {
        int port = FindAvailablePort();
        string redirectUri = $"http://127.0.0.1:{port}/";
        string state = Guid.NewGuid().ToString("N");
        string codeVerifier = GenerateCodeVerifier();
        string codeChallenge = GenerateCodeChallenge(codeVerifier);

        string authUrl = "https://accounts.google.com/o/oauth2/v2/auth"
            + $"?client_id={Uri.EscapeDataString(googleClientId)}"
            + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
            + "&response_type=code"
            + "&scope=openid%20email%20profile"
            + $"&state={state}"
            + "&code_challenge_method=S256"
            + $"&code_challenge={Uri.EscapeDataString(codeChallenge)}"
            + "&access_type=offline"
            + "&prompt=select_account";

        string authCode = null;
        string receivedState = null;
        bool listenerDone = false;
        bool listenerError = false;

        Thread listenerThread = new Thread(() =>
        {
            HttpListener listener = null;
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(redirectUri);
                listener.Start();

                IAsyncResult result = listener.BeginGetContext(null, null);
                if (result.AsyncWaitHandle.WaitOne(120000))
                {
                    HttpListenerContext context = listener.EndGetContext(result);
                    authCode = context.Request.QueryString["code"];
                    receivedState = context.Request.QueryString["state"];
                    string error = context.Request.QueryString["error"];

                    string responseHtml;
                    if (!string.IsNullOrEmpty(authCode))
                    {
                        responseHtml =
                            "<html><body style='font-family:sans-serif;text-align:center;padding:50px;background:#1a1a2e;color:#e0e0e0;'>" +
                            "<h2 style='color:#4ecca3;'>Đăng nhập thành công!</h2>" +
                            "<p>Bạn có thể đóng tab này và quay lại game.</p></body></html>";
                    }
                    else
                    {
                        responseHtml =
                            "<html><body style='font-family:sans-serif;text-align:center;padding:50px;background:#1a1a2e;color:#e0e0e0;'>" +
                            $"<h2 style='color:#e74c3c;'>Đăng nhập thất bại</h2><p>{error ?? "Lỗi không xác định"}</p></body></html>";
                        listenerError = true;
                    }

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseHtml);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.ContentType = "text/html; charset=utf-8";
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.Close();
                }
                else
                {
                    listenerError = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Google OAuth listener error: {ex.Message}");
                listenerError = true;
            }
            finally
            {
                try
                {
                    listener?.Stop();
                }
                catch
                {
                }

                listenerDone = true;
            }
        });

        listenerThread.IsBackground = true;
        listenerThread.Start();

        Application.OpenURL(authUrl);

        ShowStatus("Đang chờ đăng nhập từ trình duyệt...", Color.white);
        float timeout = 120f;
        while (!listenerDone && timeout > 0)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (listenerError || string.IsNullOrEmpty(authCode))
        {
            ShowStatus("Đăng nhập Google bị hủy hoặc hết thời gian.", Color.yellow);
            yield break;
        }

        if (receivedState != state)
        {
            ShowStatus("Lỗi bảo mật: state không khớp.", Color.red);
            yield break;
        }

        ShowStatus("Đang xác thực với Google...", Color.white);
        yield return StartCoroutine(ExchangeCodeForToken(authCode, redirectUri, codeVerifier));
    }

    private IEnumerator ExchangeCodeForToken(string code, string redirectUri, string codeVerifier)
    {
        string body = BuildUrlEncodedForm(new[]
        {
            ("code", code),
            ("client_id", googleClientId),
            ("client_secret", googleClientSecret),
            ("redirect_uri", redirectUri),
            ("code_verifier", codeVerifier),
            ("grant_type", "authorization_code"),
        });

        using (UnityWebRequest request = new UnityWebRequest("https://oauth2.googleapis.com/token", UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            Debug.Log($"FirebaseLoginManager: Exchanging Google auth code. redirectUri={redirectUri}, clientIdSet={!string.IsNullOrWhiteSpace(googleClientId)}, clientSecretSet={!string.IsNullOrWhiteSpace(googleClientSecret)}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                ShowStatus("Lỗi xác thực Google. Thử lại sau.", Color.red);
                Debug.LogError($"Token exchange failed: {request.error}\n{request.downloadHandler.text}");
                yield break;
            }

            string json = request.downloadHandler.text;
            string idToken = ExtractJsonValue(json, "id_token");
            string accessToken = ExtractJsonValue(json, "access_token");

            if (string.IsNullOrEmpty(idToken) && string.IsNullOrEmpty(accessToken))
            {
                ShowStatus("Không nhận được token hợp lệ từ Google.", Color.red);
                Debug.LogError($"No usable token in response: {json}");
                yield break;
            }

            ShowStatus("Đang đăng nhập vào game...", Color.white);
            Credential credential = GoogleAuthProvider.GetCredential(
                string.IsNullOrEmpty(idToken) ? null : idToken,
                string.IsNullOrEmpty(accessToken) ? null : accessToken);

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    string errorMsg = task.Exception != null ? ParseFirebaseError(task.Exception) : "Đăng nhập thất bại.";
                    ShowStatus(errorMsg, Color.red);
                    Debug.LogError($"Firebase Google Sign-In failed: {task.Exception}");
                    return;
                }

                FirebaseUser user = auth.CurrentUser;
                Debug.Log($"Google Sign-In success! UID: {user.UserId}, Email: {user.Email}");

                ShowStatus("Đăng nhập Google thành công! Đang kiểm tra quyền truy cập...", new Color(0.3f, 1f, 0.3f));

                Map mapInGame = new Map();
                User userInGame = new User(user.DisplayName ?? "", 100, 50, mapInGame);
                SeedNewUserProfile(user, userInGame);

                StartCoroutine(ValidateLatestVersionAndEnter(user));
            });
        }
    }

    private static string BuildUrlEncodedForm((string key, string value)[] pairs)
    {
        StringBuilder builder = new StringBuilder();

        foreach (var pair in pairs)
        {
            if (string.IsNullOrWhiteSpace(pair.key) || string.IsNullOrWhiteSpace(pair.value))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append(UnityWebRequest.EscapeURL(pair.key));
            builder.Append('=');
            builder.Append(UnityWebRequest.EscapeURL(pair.value));
        }

        return builder.ToString();
    }

    private int FindAvailablePort()
    {
        System.Net.Sockets.TcpListener listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private string ExtractJsonValue(string json, string key)
    {
        string pattern = $"\"{key}\"";
        int keyIndex = json.IndexOf(pattern, StringComparison.Ordinal);
        if (keyIndex < 0)
        {
            return null;
        }

        int colonIndex = json.IndexOf(':', keyIndex + pattern.Length);
        if (colonIndex < 0)
        {
            return null;
        }

        int startQuote = json.IndexOf('"', colonIndex + 1);
        if (startQuote < 0)
        {
            return null;
        }

        int endQuote = json.IndexOf('"', startQuote + 1);
        if (endQuote < 0)
        {
            return null;
        }

        return json.Substring(startQuote + 1, endQuote - startQuote - 1);
    }

    private string GenerateCodeVerifier()
    {
        const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";
        const int verifierLength = 64;
        StringBuilder builder = new StringBuilder(verifierLength);
        byte[] randomBytes = new byte[verifierLength];
        RandomNumberGenerator.Fill(randomBytes);

        for (int i = 0; i < verifierLength; i++)
        {
            builder.Append(allowedChars[randomBytes[i] % allowedChars.Length]);
        }

        return builder.ToString();
    }

    private string GenerateCodeChallenge(string codeVerifier)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
            return Base64UrlEncode(hash);
        }
    }

    private string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FetchLatestReleaseManifest()
    {
        if (string.IsNullOrWhiteSpace(releaseManifestUrl))
        {
            latestRelease = null;
            yield break;
        }

        latestRelease = null;

        using (UnityWebRequest request = UnityWebRequest.Get(releaseManifestUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Game release manifest fetch failed: {request.error}");
                yield break;
            }

            string json = request.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(json))
            {
                yield break;
            }

            try
            {
                latestRelease = JsonUtility.FromJson<GameReleaseManifest>(json);
                if (latestRelease == null || string.IsNullOrWhiteSpace(latestRelease.versionNumber))
                {
                    Debug.LogWarning("Game release manifest payload is empty or invalid.");
                    yield break;
                }

                string currentVersion = GetCurrentBuildVersion();
                Debug.Log($"Game release sync: local={currentVersion}, latest={latestRelease.versionNumber}, download={latestRelease.downloadUrl}");

                if (!VersionMatches(currentVersion, latestRelease.versionNumber))
                {
                    Debug.LogWarning($"Game build is out of sync with web release. Local={currentVersion}, Latest={latestRelease.versionNumber}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse game release manifest: {ex.Message}");
            }
        }
    }

    private IEnumerator ValidateLatestVersionAndEnter(FirebaseUser firebaseUser)
    {
        if (firebaseUser == null)
        {
            ShowStatus("Không tìm thấy tài khoản đăng nhập.", Color.red);
            yield break;
        }

        ShowStatus("Đang kiểm tra phiên bản mới nhất...", Color.white);
        yield return StartCoroutine(FetchLatestReleaseManifest());

        if (latestRelease == null || string.IsNullOrWhiteSpace(latestRelease.versionNumber))
        {
            Debug.LogWarning("Login blocked because the latest release manifest could not be verified.");
            auth.SignOut();
            ShowStatus("Không thể xác minh phiên bản mới nhất. Vui lòng kiểm tra mạng và thử lại.", Color.red);
            yield break;
        }

        string currentVersion = GetCurrentBuildVersion();
        if (!VersionMatches(currentVersion, latestRelease.versionNumber))
        {
            Debug.LogWarning($"Login blocked because game version is outdated. Local={currentVersion}, Latest={latestRelease.versionNumber}");
            auth.SignOut();

            // Show auto-update prompt if available, otherwise just show status text
            if (updatePromptUI != null)
            {
                ShowStatus("", Color.clear);
                updatePromptUI.Show(currentVersion, latestRelease.versionNumber, latestRelease.downloadUrl);
            }
            else
            {
                ShowStatus($"Phiên bản hiện tại ({currentVersion}) đã cũ. Vui lòng cập nhật lên {latestRelease.versionNumber} trên website.", Color.yellow);
            }
            yield break;
        }

        ValidatePurchaseAccessAndEnter(firebaseUser);
    }

    private void ValidatePurchaseAccessAndEnter(FirebaseUser firebaseUser)
    {
        if (databaseReference == null)
        {
            Debug.LogWarning("Firebase database reference is null. Blocking login because purchase validation cannot run.");
            auth.SignOut();
            ShowStatus("Không thể kiểm tra trạng thái mua game. Vui lòng thử lại.", Color.red);
            return;
        }

        string userPath = FirebaseUserPaths.GetUserRootPath(firebaseUser.UserId);
        databaseReference.Child(userPath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string errorMessage = task.Exception?.GetBaseException().Message ?? "Không thể kiểm tra trạng thái mua game.";
                Debug.LogError($"Purchase access check failed: {errorMessage}");
                ShowStatus("Không thể xác minh quyền truy cập. Vui lòng thử lại.", Color.red);
                auth.SignOut();
                return;
            }

            bool hasPurchased = ResolvePurchaseAccess(task.Result);
            if (!hasPurchased)
            {
                Debug.LogWarning($"Login blocked for unpaid account: {firebaseUser.UserId}");
                auth.SignOut();
                ShowStatus("Tài khoản này chưa mua game. Hãy thanh toán trên website trước khi chơi.", Color.yellow);
                return;
            }

            ShowStatus("Xác minh thành công! Đang vào game...", new Color(0.3f, 1f, 0.3f));
            StartCoroutine(LoadSceneAfterDelay("AsyncLoadingScene", 0.5f));
        });
    }

    private bool ResolvePurchaseAccess(DataSnapshot snapshot)
    {
        if (snapshot?.Value == null)
        {
            return false;
        }

        string rawJson = FirebaseJsonUtility.NormalizeReadValue(snapshot.GetRawJsonValue());
        if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null")
        {
            return false;
        }

        try
        {
            JToken root = JToken.Parse(rawJson);
            return TryReadBoolean(root["hasPurchased"])
                || TryReadBoolean(root["profile"]?["hasPurchased"])
                || TryReadBoolean(root["profile"]?["HasPurchased"]);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to parse purchase access payload: {ex.Message}");
            return false;
        }
    }

    private bool TryReadBoolean(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return false;
        }

        try
        {
            if (token.Type == JTokenType.Boolean)
            {
                return token.Value<bool>();
            }

            if (token.Type == JTokenType.String)
            {
                string normalized = FirebaseJsonUtility.NormalizeReadValue(token.ToString());
                return bool.TryParse(normalized, out bool result) && result;
            }

            return token.Value<bool>();
        }
        catch
        {
            return false;
        }
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = LocalizationManager.LocalizeText(message);
            statusText.color = color;
        }

        Debug.Log($"[Login] {message}");
    }

    private void SeedNewUserProfile(FirebaseUser firebaseUser, User userInGame)
    {
        if (databaseManager == null || firebaseUser == null || userInGame == null)
        {
            return;
        }

        databaseManager.WriteDatabase(FirebaseUserPaths.GetUserProfilePath(firebaseUser.UserId), userInGame.ToString());
    }

    private string ParseFirebaseError(AggregateException exception)
    {
        if (exception == null)
        {
            return "Lỗi không xác định.";
        }

        string fullMsg = exception.ToString();

        if (fullMsg.Contains("INVALID_LOGIN_CREDENTIALS") || fullMsg.Contains("WRONG_PASSWORD"))
        {
            return "Email hoặc mật khẩu không đúng.";
        }

        if (fullMsg.Contains("USER_NOT_FOUND"))
        {
            return "Tài khoản không tồn tại.";
        }

        if (fullMsg.Contains("INVALID_EMAIL"))
        {
            return "Email không hợp lệ.";
        }

        if (fullMsg.Contains("WEAK_PASSWORD"))
        {
            return "Mật khẩu quá yếu (cần ít nhất 6 ký tự).";
        }

        if (fullMsg.Contains("EMAIL_ALREADY_IN_USE"))
        {
            return "Email đã được đăng ký.";
        }

        if (fullMsg.Contains("USER_DISABLED"))
        {
            return "Tài khoản đã bị khóa.";
        }

        if (fullMsg.Contains("NETWORK") || fullMsg.Contains("network"))
        {
            return "Lỗi mạng. Kiểm tra kết nối internet.";
        }

        if (fullMsg.Contains("TOO_MANY_ATTEMPTS"))
        {
            return "Quá nhiều lần thử. Vui lòng đợi rồi thử lại.";
        }

        if (fullMsg.Contains("restricted to administrators"))
        {
            return "Phương thức đăng nhập này chưa được kích hoạt.";
        }

        return "Đăng nhập thất bại. Vui lòng thử lại.";
    }

    private string GetCurrentBuildVersion()
    {
        return string.IsNullOrWhiteSpace(Application.version) ? "unknown" : Application.version.Trim();
    }

    private bool VersionMatches(string currentVersion, string latestVersion)
    {
        return NormalizeVersionLabel(currentVersion) == NormalizeVersionLabel(latestVersion);
    }

    private string NormalizeVersionLabel(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return string.Empty;
        }

        string normalized = version.Trim().ToLowerInvariant().Replace(" ", string.Empty);

        // Strip leading "v" prefix so "v0.03" and "0.03" are treated as equal
        if (normalized.StartsWith("v"))
        {
            normalized = normalized.Substring(1);
        }

        return normalized;
    }
}
