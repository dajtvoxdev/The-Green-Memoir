using System;
using UnityEngine;

/// <summary>
/// Represents a coordinate on the tilemap grid.
/// Used for efficient tile lookup and serialization.
/// </summary>
[Serializable]
public struct TileCoordinate
{
    /// <summary>
    /// X coordinate (column) on the tilemap.
    /// </summary>
    public int x;
    
    /// <summary>
    /// Y coordinate (row) on the tilemap.
    /// </summary>
    public int y;
    
    /// <summary>
    /// Creates a new tile coordinate.
    /// </summary>
    /// <param name="x">X coordinate (column)</param>
    /// <param name="y">Y coordinate (row)</param>
    public TileCoordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    
    /// <summary>
    /// Returns a string representation in format "x_y".
    /// Used for Firebase path keys and dictionary lookups.
    /// </summary>
    public override string ToString() => $"{x}_{y}";
    
    /// <summary>
    /// Gets a hash code for this coordinate.
    /// </summary>
    public override int GetHashCode()
    {
        return x.GetHashCode() ^ (y.GetHashCode() << 2);
    }
    
    /// <summary>
    /// Checks equality with another object.
    /// </summary>
    public override bool Equals(object other)
    {
        if (other is TileCoordinate tc)
        {
            return x == tc.x && y == tc.y;
        }
        return false;
    }
    
    /// <summary>
    /// Implicit conversion from TileCoordinate to Vector3Int.
    /// </summary>
    public static implicit operator Vector3Int(TileCoordinate tc) 
        => new Vector3Int(tc.x, tc.y, 0);
    
    /// <summary>
    /// Implicit conversion from Vector3Int to TileCoordinate.
    /// </summary>
    public static implicit operator TileCoordinate(Vector3Int v) 
        => new TileCoordinate(v.x, v.y);
    
    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(TileCoordinate a, TileCoordinate b)
    {
        return a.x == b.x && a.y == b.y;
    }
    
    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(TileCoordinate a, TileCoordinate b)
    {
        return !(a == b);
    }
}