using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// User profile stored in Firebase Realtime Database.
/// Phase 2 Enhancement (#22): Added Version field for optimistic concurrency.
/// Web Integration: Added HasPurchased field for payment verification.
/// </summary>
public class User
{

    public string Name {  get; set; }
    public int Gold { get; set; }
    public int Diamond { get; set; }
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    [System.ComponentModel.DefaultValue(50)]
    public int Stamina { get; set; }

    public Map MapInGame {  get; set; }

    /// <summary>
    /// Data version for optimistic concurrency control.
    /// Incremented on every save — server rejects writes with stale version.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// Whether this user has purchased the game via the website.
    /// Set by the web server (Admin SDK) after successful Sepay payment.
    /// Game reads this field on login to verify access.
    /// Default false for backward compatibility with existing users.
    /// </summary>
    [JsonProperty("hasPurchased", DefaultValueHandling = DefaultValueHandling.Populate)]
    [System.ComponentModel.DefaultValue(false)]
    public bool HasPurchased { get; set; }

    public User()
    {
        Stamina = 50;
        Version = 0;
        HasPurchased = false;
    }

    public User(string name, int gold, int diamond, Map mapInGame)
    {
        Name = name;
        Gold = gold;
        Diamond = diamond;
        Stamina = 50;
        MapInGame = mapInGame;
        Version = 0;
        HasPurchased = false;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
