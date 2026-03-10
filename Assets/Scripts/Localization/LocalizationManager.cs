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
