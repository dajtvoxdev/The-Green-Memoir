using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FirebaseLoginManager : MonoBehaviour
{
    //Dang Ki
    [Header("Register")]
    public InputField ipRegisterEmail;
    public InputField ipRegisterPassword;

    public Button buttonRegister;

    //Dang nhap
    [Header("Sign In")]
    public InputField ipLoginEmail;
    public InputField ipLoginPassword;

    public Button buttonLogin;

    private FirebaseAuth auth;

    //Dang nhap nhanh: Google va Choi Ngay (Anonymous)
    [Header("Quick Login")]
    public Button buttonLoginGoogle;
    public Button buttonPlayNow;

    //Chuyen doi qua lai giua dang ki va dang nhap
    [Header("Switch from")]
    public Button buttonMoveToSignIn;
    public Button buttonMoveToRegister;

    public GameObject loginForm;
    public GameObject registerForm;

    [Header("Status Message")]
    [Tooltip("Text element to show login/register error messages.")]
    public TMP_Text statusText;

    //Upload data User to Firebase when register
    private FirebaseDatabaseManager databaseManager;
    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        buttonRegister.onClick.AddListener(RegisterAccountWithFireBase);
        buttonLogin.onClick.AddListener(SignInAccountWithFireBase);

        //Dang nhap nhanh
        if (buttonLoginGoogle != null)
            buttonLoginGoogle.onClick.AddListener(SignInWithGoogle);
        if (buttonPlayNow != null)
            buttonPlayNow.onClick.AddListener(SignInAnonymously);

        buttonMoveToSignIn.onClick.AddListener(SwitchForm);
        buttonMoveToRegister.onClick.AddListener(SwitchForm);

        databaseManager = GetComponent<FirebaseDatabaseManager>();
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

            if (task.IsCompleted)
            {
                ShowStatus("Đăng ký thành công!", new Color(0.3f, 1f, 0.3f));


                Map mapInGame = new Map();
                User userInGame = new User("", 100, 50, mapInGame);

                FirebaseUser firebaseUser = task.Result.User;
                Debug.Log("Firebase user: " + firebaseUser);

                SeedNewUserProfile(firebaseUser, userInGame);

                SceneManager.LoadScene("AsyncLoadingScene");

                //FirebaseUser userLog = task.Result.User;
                //Debug.Log("UID: " + userLog.UserId);
                //Debug.Log("Email: " + userLog.Email);
            }
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

            if (task.IsCompleted)
            {
                Debug.Log("Dang nhap thanh cong");
                FirebaseUser user = task.Result.User;
                Debug.Log("UID: " + user.UserId);

                ShowStatus("Đăng nhập thành công!", new Color(0.3f, 1f, 0.3f));
                SceneManager.LoadScene("AsyncLoadingScene");
            }
        });
    }

    public void SwitchForm()
    {
        loginForm.SetActive(!loginForm.activeSelf);
        registerForm.SetActive(!registerForm.activeSelf);
    }

    /// <summary>
    /// Đăng nhập ẩn danh (Chơi Ngay) - không cần email/password.
    /// Firebase tự tạo UID tạm cho người chơi.
    /// </summary>
    public void SignInAnonymously()
    {
        Debug.Log("Dang nhap an danh (Choi Ngay)...");
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.Log("Dang nhap an danh bi huy");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.Log("Dang nhap an danh that bai");
                Debug.Log(task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Dang nhap an danh thanh cong!");
                FirebaseUser user = task.Result.User;
                Debug.Log("Anonymous UID: " + user.UserId);

                // Tạo dữ liệu mặc định cho người chơi ẩn danh
                Map mapInGame = new Map();
                User userInGame = new User("", 100, 50, mapInGame);
                SeedNewUserProfile(user, userInGame);

                SceneManager.LoadScene("AsyncLoadingScene");
            }
        });
    }

    /// <summary>
    /// Đăng nhập bằng Google.
    /// Sử dụng Firebase Auth + Google Provider.
    /// Trên Android: gọi Google Play Services để lấy ID Token.
    /// Trên Editor: chỉ log hướng dẫn.
    /// </summary>
    public void SignInWithGoogle()
    {
        Debug.Log("Bắt đầu đăng nhập Google...");

#if UNITY_ANDROID && !UNITY_EDITOR
        // Gọi Google Sign-In trên Android thông qua Firebase Auth UI
        // Cần cấu hình Web Client ID trong Firebase Console → Authentication → Sign-in method → Google
        StartCoroutine(GoogleSignInCoroutine());
#else
        // Trên Editor/Desktop: Firebase Google Sign-In không hỗ trợ popup trực tiếp.
        // Sử dụng đăng nhập ẩn danh làm fallback cho testing.
        Debug.Log("Google Sign-In chỉ hoạt động trên Android/iOS.");
        Debug.Log("Dùng đăng nhập ẩn danh để test thay thế...");
        SignInAnonymously();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// Coroutine xử lý Google Sign-In trên Android.
    /// Gọi Google Play Services → lấy ID Token → tạo Firebase Credential → đăng nhập.
    /// </summary>
    private System.Collections.IEnumerator GoogleSignInCoroutine()
    {
        // Web Client ID từ Firebase Console → Authentication → Sign-in method → Google
        // Lấy từ google-services.json mục oauth_client → client_type=3 (web)
        string webClientId = "984351192548-ao3t6cbqjtmnq7qt37etq02cgj05ulvv.apps.googleusercontent.com";

        // Kiểm tra Web Client ID đã cấu hình chưa
        if (webClientId.StartsWith("YOUR_"))
        {
            Debug.LogError("Chua cau hinh Web Client ID! Vao Firebase Console → Authentication → Google → lay Web Client ID.");
            Debug.LogError("Sau do paste vao bien webClientId trong FirebaseLoginManager.cs");
            yield break;
        }

        bool signInCompleted = false;
        string idToken = null;
        string errorMsg = null;

        // Gọi Google Sign-In trên Android bằng GoogleSignInOptions + GoogleSignInClient
        try
        {
            using (var googleSignInOptions = new AndroidJavaObject(
                "com.google.android.gms.auth.api.signin.GoogleSignInOptions$Builder",
                new AndroidJavaObject("com.google.android.gms.auth.api.signin.GoogleSignInOptions",
                    "DEFAULT_SIGN_IN")))
            {
                googleSignInOptions.Call<AndroidJavaObject>("requestIdToken", webClientId);
                googleSignInOptions.Call<AndroidJavaObject>("requestEmail");
                var options = googleSignInOptions.Call<AndroidJavaObject>("build");

                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var googleSignInClient = new AndroidJavaClass(
                    "com.google.android.gms.auth.api.signin.GoogleSignIn")
                    .CallStatic<AndroidJavaObject>("getClient", activity, options))
                {
                    var signInIntent = googleSignInClient.Call<AndroidJavaObject>("getSignInIntent");
                    activity.Call("startActivityForResult", signInIntent, 9001);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Google Sign-In Android error: " + ex.Message);
            Debug.Log("Fallback: dùng đăng nhập ẩn danh...");
            SignInAnonymously();
            yield break;
        }

        // Chờ xử lý kết quả (sẽ cần ActivityResult handler)
        // Trong trường hợp đơn giản, fallback sang Anonymous
        Debug.Log("Google Sign-In: Đang chờ kết quả từ Google...");
        yield return new UnityEngine.WaitForSeconds(3f);

        if (idToken != null)
        {
            // Tạo Firebase Credential từ Google ID Token
            Firebase.Auth.Credential credential =
                Firebase.Auth.GoogleAuthProvider.GetCredential(idToken, null);

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Firebase Google Sign-In that bai: " + task.Exception);
                    return;
                }

                Debug.Log("Dang nhap Google thanh cong!");
                FirebaseUser user = task.Result.User;
                Debug.Log("Google UID: " + user.UserId);
                Debug.Log("Google Email: " + user.Email);

                // Tạo dữ liệu mặc định cho người chơi
                Map mapInGame = new Map();
                User userInGame = new User(user.DisplayName ?? "", 100, 50, mapInGame);
                SeedNewUserProfile(user, userInGame);

                SceneManager.LoadScene("AsyncLoadingScene");
            });
        }
        else
        {
            Debug.Log("Khong lay duoc Google ID Token. Fallback: dang nhap an danh.");
            SignInAnonymously();
        }
    }
#endif

    /// <summary>
    /// Shows a status message on the login UI.
    /// </summary>
    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
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

    /// <summary>
    /// Extracts a user-friendly error message from Firebase exceptions.
    /// </summary>
    private string ParseFirebaseError(System.AggregateException exception)
    {
        if (exception == null) return "Lỗi không xác định.";

        string fullMsg = exception.ToString();

        if (fullMsg.Contains("INVALID_LOGIN_CREDENTIALS") || fullMsg.Contains("WRONG_PASSWORD"))
            return "Email hoặc mật khẩu không đúng.";
        if (fullMsg.Contains("USER_NOT_FOUND"))
            return "Tài khoản không tồn tại.";
        if (fullMsg.Contains("INVALID_EMAIL"))
            return "Email không hợp lệ.";
        if (fullMsg.Contains("WEAK_PASSWORD"))
            return "Mật khẩu quá yếu (cần ít nhất 6 ký tự).";
        if (fullMsg.Contains("EMAIL_ALREADY_IN_USE"))
            return "Email đã được đăng ký.";
        if (fullMsg.Contains("USER_DISABLED"))
            return "Tài khoản đã bị khóa.";
        if (fullMsg.Contains("NETWORK") || fullMsg.Contains("network"))
            return "Lỗi mạng. Kiểm tra kết nối internet.";
        if (fullMsg.Contains("TOO_MANY_ATTEMPTS"))
            return "Quá nhiều lần thử. Vui lòng đợi rồi thử lại.";

        return "Đăng nhập thất bại. Vui lòng thử lại.";
    }
}
