using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Central localization service for EN/VI UI text.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public event Action<GameLanguage> OnLanguageChanged;

    public GameLanguage CurrentLanguage { get; private set; }

    private const string LanguagePrefKey = "game_language";

    private static readonly Dictionary<string, string> EnglishToVietnamese = new Dictionary<string, string>
    {
        { "LOGIN", "ĐĂNG NHẬP" },
        { "Login", "Đăng nhập" },
        { "REGISTER", "ĐĂNG KÝ" },
        { "Register", "Đăng ký" },
        { "Or login with", "Hoặc đăng nhập bằng" },
        { "Forgot Password", "Quên mật khẩu" },
        { "Don't have an account? Tap register now", "Chưa có tài khoản? Nhấn đăng ký ngay" },
        { "Already have an account? Tap login now", "Đã có tài khoản? Nhấn đăng nhập ngay" },
        { "Password...", "Mật khẩu..." },
        { "Confirm password...", "Xác nhận mật khẩu..." },
        { "PLAY NOW", "CHƠI NGAY" },
        { "Loading...", "Đang tải..." },
        { "Choose your name", "Chọn tên của bạn" },
        { "User Name...", "Tên người chơi..." },
        { "Username:", "Tên:" },
        { "Gold:", "Vàng:" },
        { "Diamond:", "Kim cương:" },
        { "Load Done", "Đã tải xong" },
        { "Dawn", "Rạng đông" },
        { "Morning", "Buổi sáng" },
        { "Afternoon", "Buổi chiều" },
        { "Evening", "Buổi tối" },
        { "Night", "Đêm" },
        { "[No Tool]", "[Không có công cụ]" },
        { "Plant", "Trồng" },
        { "Use", "Dùng" },
        { "Buy", "Mua" },
        { "Sell", "Bán" },
        { "Assign Quickbar", "Gắn Quickbar" },
        { "Remove Quickbar", "Bỏ Quickbar" },
        { "Quickbar is full (max 9 slots).", "Quickbar đã đầy (tối đa 9 ô)." },
        { "Quickbar not found.", "Không tìm thấy Quickbar." },
        { "Item", "Vật phẩm" },
        { "Name", "Tên" },
        { "Gender", "Giới tính" },
        { "Des", "Mô tả" },
        { "No description.", "Không có mô tả." },
        { "Initializing...", "Đang khởi tạo..." },
        { "Connecting to server...", "Đang kết nối máy chủ..." },
        { "Data loaded. Preparing scene...", "Đã tải dữ liệu. Đang chuẩn bị màn chơi..." },
        { "Loading scene...", "Đang tải màn chơi..." },
        { "Loading complete!", "Tải xong!" },
        { "Ready", "Sẵn sàng" },
        { "Fetching user data from server...", "Đang tải dữ liệu người chơi từ máy chủ..." },

        // Login & Register
        { "Please enter email and password.", "Vui lòng nhập email và mật khẩu." },
        { "Password must be at least 6 characters.", "Mật khẩu phải có ít nhất 6 ký tự." },
        { "Registering...", "Đang đăng ký..." },
        { "Registration cancelled.", "Đăng ký bị hủy." },
        { "Registration successful! Checking access...", "Đăng ký thành công! Đang kiểm tra quyền truy cập..." },
        { "Logging in...", "Đang đăng nhập..." },
        { "Login cancelled.", "Đăng nhập bị hủy." },
        { "Login successful! Checking access...", "Đăng nhập thành công! Đang kiểm tra quyền truy cập..." },
        { "Google Client ID not configured.", "Chưa cấu hình Google Client ID." },
        { "Google Client Secret not configured.", "Chưa cấu hình Google Client Secret." },
        { "Opening browser for Google login...", "Đang mở trình duyệt để đăng nhập Google..." },
        { "Waiting for browser login...", "Đang chờ đăng nhập từ trình duyệt..." },
        { "Google login cancelled or timed out.", "Đăng nhập Google bị hủy hoặc hết thời gian." },
        { "Security error: state mismatch.", "Lỗi bảo mật: state không khớp." },
        { "Authenticating with Google...", "Đang xác thực với Google..." },
        { "Google authentication error. Try again later.", "Lỗi xác thực Google. Thử lại sau." },
        { "No token received from Google.", "Không nhận được token từ Google." },
        { "Logging into game...", "Đang đăng nhập vào game..." },
        { "Login failed.", "Đăng nhập thất bại." },
        { "Google login successful! Checking access...", "Đăng nhập Google thành công! Đang kiểm tra quyền truy cập..." },
        { "Login account not found.", "Không tìm thấy tài khoản đăng nhập." },
        { "Cannot verify access. Please try again.", "Không thể xác minh quyền truy cập. Vui lòng thử lại." },
        { "This account has not purchased the game. Please pay on the website before playing.", "Tài khoản này chưa mua game. Hãy thanh toán trên website trước khi chơi." },
        { "Verification successful! Entering game...", "Xác minh thành công! Đang vào game..." },

        // Firebase errors
        { "Unknown error.", "Lỗi không xác định." },
        { "Incorrect email or password.", "Email hoặc mật khẩu không đúng." },
        { "Account does not exist.", "Tài khoản không tồn tại." },
        { "Invalid email.", "Email không hợp lệ." },
        { "Password too weak (at least 6 characters required).", "Mật khẩu quá yếu (cần ít nhất 6 ký tự)." },
        { "Email already registered.", "Email đã được đăng ký." },
        { "Account has been locked.", "Tài khoản đã bị khóa." },
        { "Network error. Check internet connection.", "Lỗi mạng. Kiểm tra kết nối internet." },
        { "Too many attempts. Please wait and try again.", "Quá nhiều lần thử. Vui lòng đợi rồi thử lại." },
        { "This login method has not been activated.", "Phương thức đăng nhập này chưa được kích hoạt." },
        { "Login failed. Please try again.", "Đăng nhập thất bại. Vui lòng thử lại." },

        // Notifications
        { "Login successful!", "Đăng nhập thành công!" },
        { "You can close this tab and return to the game.", "Bạn có thể đóng tab này và quay lại game." },
        { "Data conflict! Please reload.", "Xung đột dữ liệu! Vui lòng tải lại." },
        { "Data synced from server.", "Dữ liệu đã đồng bộ từ máy chủ." },

        // Auto-Update
        { "Update available!", "Cập nhật có sẵn!" },
        { "Update now", "Cập nhật ngay" },
        { "No download link. Please update from the website.", "Không có link tải. Vui lòng cập nhật từ website." },
        { "Download cancelled.", "Đã hủy tải xuống." },
        { "Retry", "Thử lại" },
        { "Cancel", "Hủy" },
        { "Downloading update...", "Đang tải bản cập nhật..." },
        { "Download complete! Launching installer...", "Tải hoàn tất! Đang khởi chạy cài đặt..." },
        { "Download failed.", "Tải thất bại." },
        { "Error: Downloaded file not found.", "Lỗi: File tải về không tìm thấy." },
        { "Cannot launch installer.", "Không thể chạy bộ cài đặt." },
        { "No download link.", "Không có link tải." },
        { "You need to update to continue playing.", "Bạn cần cập nhật để tiếp tục chơi." },

        // Gameplay
        { "Out of energy! Rest until tomorrow to recover.", "Hết năng lượng! Nghỉ ngơi đến ngày mai để phục hồi." },
        { "Select a seed first! Press 1-9 to choose.", "Chọn hạt giống trước! Nhấn 1-9 để chọn." },
        { "Out of seeds! Buy more at the shop.", "Hết hạt giống! Mua thêm ở cửa hàng." },
        { "Inventory is full!", "Túi đồ đã đầy!" },
        { "Shop is not ready.", "Cửa hàng chưa sẵn sàng." },
        { "Shop is under construction...", "Cửa hàng đang được xây dựng..." },
        { "This item cannot be bought.", "Vật phẩm này không thể mua." },
        { "Shop does not buy this item.", "Cửa hàng không thu mua vật phẩm này." },
        { "Not enough gold!", "Không đủ vàng!" },
        { "Equipped:", "Đã trang bị:" },
        { "It's raining today! Crops will be watered automatically.", "Hôm nay trời mưa! Cây trồng sẽ được tưới tự động." },

        // Growth stages
        { "Seed", "Hạt giống" },
        { "Sprout", "Nảy mầm" },
        { "Growing", "Đang lớn" },
        { "Mature", "Trưởng thành" },
        { "Ready to harvest!", "Sẵn sàng thu hoạch!" },
        { "Developing", "Đang phát triển" },

        // Tutorial
        { "Press [B] to open inventory and select seeds.", "Nhấn [B] để mở túi đồ và chọn hạt giống." },
        { "Right-click on grass to till the soil.", "Chuột phải vào đất cỏ để cuốc đất." },
        { "Select seed (keys 1-9), then right-click on tilled soil to plant.", "Chọn hạt (phím 1-9), rồi chuột phải vào đất đã cuốc để gieo." },
        { "Right-click on crops to water or harvest.", "Chuột phải vào cây để tưới nước hoặc thu hoạch." },
        { "Visit the shopkeeper and press [E] to buy more seeds!", "Đến gặp người bán hàng và nhấn [E] để mua thêm hạt giống!" },
    };

    private static readonly Dictionary<string, string> VietnameseToEnglish = BuildReverseMap(EnglishToVietnamese);

    private static readonly Dictionary<string, string> ItemTypeEnToVi = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "seed", "Hạt giống" },
        { "consumable", "Tiêu hao" },
        { "tool", "Công cụ" },
        { "resource", "Tài nguyên" },
        { "material", "Vật liệu" },
        { "crop", "Nông sản" }
    };

    private static readonly Dictionary<string, string> ItemTypeViToEn = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "hạt giống", "seed" },
        { "tiêu hao", "consumable" },
        { "công cụ", "tool" },
        { "tài nguyên", "resource" },
        { "vật liệu", "material" },
        { "nông sản", "crop" }
    };

    private static readonly Regex EnGoldRegex = new Regex(@"^Gold:\s*(\d+)$", RegexOptions.Compiled);
    private static readonly Regex EnDiamondRegex = new Regex(@"^Diamond:\s*(\d+)$", RegexOptions.Compiled);
    private static readonly Regex EnDayRegex = new Regex(@"^Day\s*(\d+)$", RegexOptions.Compiled);
    private static readonly Regex EnQtyTypeRegex = new Regex(@"^Qty:\s*(\d+)\s*\|\s*Type:\s*(.+)$", RegexOptions.Compiled);
    private static readonly Regex EnReadyToPlantRegex = new Regex(@"^Ready to plant\s+(.+)!$", RegexOptions.Compiled);
    private static readonly Regex EnUsedRegex = new Regex(@"^Used\s+(.+)\.$", RegexOptions.Compiled);
    private static readonly Regex EnDroppedRegex = new Regex(@"^Dropped\s+1x\s+(.+)\.$", RegexOptions.Compiled);
    private static readonly Regex EnSplitRegex = new Regex(@"^Split\s+(.+)\s+\((\d+)\)$", RegexOptions.Compiled);
    private static readonly Regex EnErrorRegex = new Regex(@"^Error:\s*(.+)$", RegexOptions.Compiled);
    private static readonly Regex EnLoadDoneRegex = new Regex(@"^Load Done\s*$", RegexOptions.Compiled);
    private static readonly Regex EnAssignedQuickbarRegex = new Regex(@"^Assigned (.+) to Quickbar!$", RegexOptions.Compiled);
    private static readonly Regex EnRemovedQuickbarRegex = new Regex(@"^Removed (.+) from Quickbar\.$", RegexOptions.Compiled);

    private static readonly Regex ViGoldRegex = new Regex(@"^Vàng:\s*(\d+)$", RegexOptions.Compiled);
    private static readonly Regex ViDiamondRegex = new Regex(@"^Kim cương:\s*(\d+)$", RegexOptions.Compiled);
    private static readonly Regex ViDayRegex = new Regex(@"^Ngày\s*(\d+)$", RegexOptions.Compiled);
    private static readonly Regex ViQtyTypeRegex = new Regex(@"^SL:\s*(\d+)\s*\|\s*Loại:\s*(.+)$", RegexOptions.Compiled);
    private static readonly Regex ViReadyToPlantRegex = new Regex(@"^Sẵn sàng trồng\s+(.+)!$", RegexOptions.Compiled);
    private static readonly Regex ViUsedRegex = new Regex(@"^Đã dùng\s+(.+)\.$", RegexOptions.Compiled);
    private static readonly Regex ViDroppedRegex = new Regex(@"^Đã vứt\s+1x\s+(.+)\.$", RegexOptions.Compiled);
    private static readonly Regex ViSplitRegex = new Regex(@"^Tách\s+(.+)\s+\((\d+)\)$", RegexOptions.Compiled);
    private static readonly Regex ViErrorRegex = new Regex(@"^Lỗi:\s*(.+)$", RegexOptions.Compiled);
    private static readonly Regex ViLoadDoneRegex = new Regex(@"^Đã tải xong\s*$", RegexOptions.Compiled);
    private static readonly Regex ViAssignedQuickbarRegex = new Regex(@"^Đã gắn (.+) vào Quickbar!$", RegexOptions.Compiled);
    private static readonly Regex ViRemovedQuickbarRegex = new Regex(@"^Đã bỏ (.+) khỏi Quickbar\.$", RegexOptions.Compiled);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadInitialLanguage();
    }

    public static string LocalizeText(string input)
    {
        return Instance != null ? Instance.Localize(input) : input;
    }

    public void ToggleLanguage()
    {
        SetLanguage(CurrentLanguage == GameLanguage.English ? GameLanguage.Vietnamese : GameLanguage.English);
    }

    public void SetLanguage(GameLanguage language, bool saveToPrefs = true)
    {
        if (CurrentLanguage == language)
        {
            return;
        }

        CurrentLanguage = language;

        if (saveToPrefs)
        {
            PlayerPrefs.SetInt(LanguagePrefKey, (int)language);
            PlayerPrefs.Save();
        }

        OnLanguageChanged?.Invoke(CurrentLanguage);
    }

    public string Localize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return CurrentLanguage == GameLanguage.Vietnamese
            ? TranslateToVietnamese(input)
            : TranslateToEnglish(input);
    }

    private void LoadInitialLanguage()
    {
        GameLanguage defaultLanguage = GameLanguage.Vietnamese;

        if (PlayerPrefs.HasKey(LanguagePrefKey))
        {
            int saved = PlayerPrefs.GetInt(LanguagePrefKey, (int)defaultLanguage);
            if (Enum.IsDefined(typeof(GameLanguage), saved))
            {
                CurrentLanguage = (GameLanguage)saved;
                return;
            }
        }

        CurrentLanguage = defaultLanguage;
        PlayerPrefs.SetInt(LanguagePrefKey, (int)defaultLanguage);
        PlayerPrefs.Save();
    }

    private static Dictionary<string, string> BuildReverseMap(Dictionary<string, string> input)
    {
        Dictionary<string, string> reverse = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> pair in input)
        {
            if (!reverse.ContainsKey(pair.Value))
            {
                reverse.Add(pair.Value, pair.Key);
            }
        }
        return reverse;
    }

    private string TranslateToVietnamese(string input)
    {
        if (EnglishToVietnamese.TryGetValue(input, out string exact))
        {
            return exact;
        }

        Match match = EnGoldRegex.Match(input);
        if (match.Success) return $"Vàng: {match.Groups[1].Value}";

        match = EnDiamondRegex.Match(input);
        if (match.Success) return $"Kim cương: {match.Groups[1].Value}";

        match = EnDayRegex.Match(input);
        if (match.Success) return $"Ngày {match.Groups[1].Value}";

        match = EnQtyTypeRegex.Match(input);
        if (match.Success)
        {
            string type = TranslateItemType(match.Groups[2].Value, GameLanguage.Vietnamese);
            return $"SL: {match.Groups[1].Value}  |  Loại: {type}";
        }

        match = EnReadyToPlantRegex.Match(input);
        if (match.Success) return $"Sẵn sàng trồng {match.Groups[1].Value}!";

        match = EnUsedRegex.Match(input);
        if (match.Success) return $"Đã dùng {match.Groups[1].Value}.";

        match = EnDroppedRegex.Match(input);
        if (match.Success) return $"Đã vứt 1x {match.Groups[1].Value}.";

        match = EnSplitRegex.Match(input);
        if (match.Success) return $"Tách {match.Groups[1].Value} ({match.Groups[2].Value})";

        match = EnErrorRegex.Match(input);
        if (match.Success) return $"Lỗi: {match.Groups[1].Value}";

        match = EnLoadDoneRegex.Match(input);
        if (match.Success) return "Đã tải xong";

        match = EnAssignedQuickbarRegex.Match(input);
        if (match.Success) return $"Đã gắn {match.Groups[1].Value} vào Quickbar!";

        match = EnRemovedQuickbarRegex.Match(input);
        if (match.Success) return $"Đã bỏ {match.Groups[1].Value} khỏi Quickbar.";

        return input;
    }

    private string TranslateToEnglish(string input)
    {
        if (VietnameseToEnglish.TryGetValue(input, out string exact))
        {
            return exact;
        }

        Match match = ViGoldRegex.Match(input);
        if (match.Success) return $"Gold: {match.Groups[1].Value}";

        match = ViDiamondRegex.Match(input);
        if (match.Success) return $"Diamond: {match.Groups[1].Value}";

        match = ViDayRegex.Match(input);
        if (match.Success) return $"Day {match.Groups[1].Value}";

        match = ViQtyTypeRegex.Match(input);
        if (match.Success)
        {
            string type = TranslateItemType(match.Groups[2].Value, GameLanguage.English);
            return $"Qty: {match.Groups[1].Value}  |  Type: {type}";
        }

        match = ViReadyToPlantRegex.Match(input);
        if (match.Success) return $"Ready to plant {match.Groups[1].Value}!";

        match = ViUsedRegex.Match(input);
        if (match.Success) return $"Used {match.Groups[1].Value}.";

        match = ViDroppedRegex.Match(input);
        if (match.Success) return $"Dropped 1x {match.Groups[1].Value}.";

        match = ViSplitRegex.Match(input);
        if (match.Success) return $"Split {match.Groups[1].Value} ({match.Groups[2].Value})";

        match = ViErrorRegex.Match(input);
        if (match.Success) return $"Error: {match.Groups[1].Value}";

        match = ViLoadDoneRegex.Match(input);
        if (match.Success) return "Load Done";

        match = ViAssignedQuickbarRegex.Match(input);
        if (match.Success) return $"Assigned {match.Groups[1].Value} to Quickbar!";

        match = ViRemovedQuickbarRegex.Match(input);
        if (match.Success) return $"Removed {match.Groups[1].Value} from Quickbar.";

        return input;
    }

    private static string TranslateItemType(string itemType, GameLanguage targetLanguage)
    {
        if (string.IsNullOrWhiteSpace(itemType))
        {
            return itemType;
        }

        if (targetLanguage == GameLanguage.Vietnamese)
        {
            return ItemTypeEnToVi.TryGetValue(itemType, out string viType) ? viType : itemType;
        }

        return ItemTypeViToEn.TryGetValue(itemType, out string enType) ? enType : itemType;
    }
}
