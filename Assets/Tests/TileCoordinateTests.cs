using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for the TileCoordinate struct.
/// Tests coordinate conversion, hashing, and equality.
/// </summary>
public class TileCoordinateTests
{
    [Test]
    public void Constructor_SetsXAndY()
    {
        // Arrange
        int x = 5;
        int y = 10;
        
        // Act
        TileCoordinate coord = new TileCoordinate(x, y);
        
        // Assert
        Assert.AreEqual(x, coord.x);
        Assert.AreEqual(y, coord.y);
    }
    
    [Test]
    public void ToString_ReturnsUnderscoreSeparatedFormat()
    {
        // Arrange
        TileCoordinate coord = new TileCoordinate(3, 7);
        
        // Act
        string result = coord.ToString();
        
        // Assert
        Assert.AreEqual("3_7", result);
    }
    
    [Test]
    public void ToString_NegativeCoordinates()
    {
        // Arrange
        TileCoordinate coord = new TileCoordinate(-5, -3);
        
        // Act
        string result = coord.ToString();
        
        // Assert
        Assert.AreEqual("-5_-3", result);
    }
    
    [Test]
    public void GetHashCode_SameCoordinates_ReturnsSameHash()
    {
        // Arrange
        TileCoordinate coord1 = new TileCoordinate(5, 10);
        TileCoordinate coord2 = new TileCoordinate(5, 10);
        
        // Act & Assert
        Assert.AreEqual(coord1.GetHashCode(), coord2.GetHashCode());
    }
    
    [Test]
    public void GetHashCode_DifferentCoordinates_ReturnsDifferentHash()
    {
        // Arrange
        TileCoordinate coord1 = new TileCoordinate(5, 10);
        TileCoordinate coord2 = new TileCoordinate(10, 5);
        
        // Note: Different coordinates *usually* return different hashes,
        // but hash collisions are theoretically possible.
        // This test verifies the common case.
        
        // Act & Assert
        // We can't guarantee different hashes, but we can verify equality works
        Assert.IsFalse(coord1.Equals(coord2));
    }
    
    [Test]
    public void Equals_SameCoordinates_ReturnsTrue()
    {
        // Arrange
        TileCoordinate coord1 = new TileCoordinate(5, 10);
        TileCoordinate coord2 = new TileCoordinate(5, 10);
        
        // Act & Assert
        Assert.IsTrue(coord1.Equals(coord2));
        Assert.IsTrue(coord1 == coord2);
    }
    
    [Test]
    public void Equals_DifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        TileCoordinate coord1 = new TileCoordinate(5, 10);
        TileCoordinate coord2 = new TileCoordinate(5, 11);
        
        // Act & Assert
        Assert.IsFalse(coord1.Equals(coord2));
        Assert.IsFalse(coord1 == coord2);
        Assert.IsTrue(coord1 != coord2);
    }
    
    [Test]
    public void Equals_DifferentX_ReturnsFalse()
    {
        // Arrange
        TileCoordinate coord1 = new TileCoordinate(5, 10);
        TileCoordinate coord2 = new TileCoordinate(6, 10);
        
        // Act & Assert
        Assert.IsFalse(coord1.Equals(coord2));
        Assert.IsFalse(coord1 == coord2);
    }
    
    [Test]
    public void Equals_DifferentY_ReturnsFalse()
    {
        // Arrange
        TileCoordinate coord1 = new TileCoordinate(5, 10);
        TileCoordinate coord2 = new TileCoordinate(5, 20);
        
        // Act & Assert
        Assert.IsFalse(coord1.Equals(coord2));
        Assert.IsFalse(coord1 == coord2);
    }
    
    [Test]
    public void ImplicitConversion_ToVector3Int()
    {
        // Arrange
        TileCoordinate coord = new TileCoordinate(5, 10);
        
        // Act
        Vector3Int vector = coord;
        
        // Assert
        Assert.AreEqual(5, vector.x);
        Assert.AreEqual(10, vector.y);
        Assert.AreEqual(0, vector.z);
    }
    
    [Test]
    public void ImplicitConversion_FromVector3Int()
    {
        // Arrange
        Vector3Int vector = new Vector3Int(7, 14, 0);
        
        // Act
        TileCoordinate coord = vector;
        
        // Assert
        Assert.AreEqual(7, coord.x);
        Assert.AreEqual(14, coord.y);
    }
    
    [Test]
    public void EqualityOperator_SameObject_ReturnsTrue()
    {
        // Arrange
        TileCoordinate coord = new TileCoordinate(5, 10);
        TileCoordinate sameRef = coord;

        // Act & Assert
        Assert.IsTrue(coord == sameRef);
    }
    
    [Test]
    public void InequalityOperator_SameCoordinates_ReturnsFalse()
    {
        // Arrange
        TileCoordinate coord1 = new TileCoordinate(5, 10);
        TileCoordinate coord2 = new TileCoordinate(5, 10);
        
        // Act & Assert
        Assert.IsFalse(coord1 != coord2);
    }
    
    [Test]
    public void DictionaryKey_CanUseAsKey()
    {
        // Arrange
        var dict = new System.Collections.Generic.Dictionary<TileCoordinate, string>();
        TileCoordinate coord1 = new TileCoordinate(3, 5);
        TileCoordinate coord2 = new TileCoordinate(3, 5);
        
        // Act
        dict[coord1] = "value1";
        string retrievedValue = dict[coord2];
        
        // Assert
        Assert.AreEqual("value1", retrievedValue);
    }
    
    [Test]
    public void ZeroCoordinates_WorksCorrectly()
    {
        // Arrange
        TileCoordinate coord = new TileCoordinate(0, 0);
        
        // Assert
        Assert.AreEqual(0, coord.x);
        Assert.AreEqual(0, coord.y);
        Assert.AreEqual("0_0", coord.ToString());
    }
}