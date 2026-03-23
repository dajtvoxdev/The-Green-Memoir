/// <summary>
/// Centralized Firebase path builder for user-related data.
/// Separates gameplay profile data from sibling nodes like inventory and flags.
/// </summary>
public static class FirebaseUserPaths
{
    public static string GetUserRootPath(string userId) => $"Users/{userId}";

    public static string GetUserProfilePath(string userId) => $"{GetUserRootPath(userId)}/profile";

    public static string GetInventoryPath(string userId) => $"{GetUserRootPath(userId)}/inventory";

    public static string GetStarterPackFlagPath(string userId) => $"{GetUserRootPath(userId)}/starterPackGiven";

    public static string GetStarterPackRecoveryFlagPath(string userId) => $"{GetUserRootPath(userId)}/starterPackRecovered";

    public static string GetProfileMapPath(string userId) => $"{GetUserProfilePath(userId)}/MapInGame";

    public static string GetProfileTilesPath(string userId) => $"{GetProfileMapPath(userId)}/lstTilemapDetail";

    public static string GetProfileTilePath(string userId, int tileIndex) => $"{GetProfileTilesPath(userId)}/{tileIndex}";
}
