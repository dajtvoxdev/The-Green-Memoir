using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// User profile stored in Firebase Realtime Database.
/// Phase 2 Enhancement (#22): Added Version field for optimistic concurrency.
/// </summary>
public class User
{

    public string Name {  get; set; }
    public int Gold { get; set; }
    public int Diamond { get; set; }

    public Map MapInGame {  get; set; }

    /// <summary>
    /// Data version for optimistic concurrency control.
    /// Incremented on every save — server rejects writes with stale version.
    /// </summary>
    public long Version { get; set; }

    public User()
    {
        Version = 0;
    }

    public User(string name, int gold, int diamond, Map mapInGame)
    {
        Name = name;
        Gold = gold;
        Diamond = diamond;
        MapInGame = mapInGame;
        Version = 0;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
