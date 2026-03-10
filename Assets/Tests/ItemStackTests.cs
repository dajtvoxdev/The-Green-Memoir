using NUnit.Framework;
using UnityEngine.TestTools;

/// <summary>
/// Unit tests for the ItemStack class.
/// Tests stacking behavior, quantity limits, and edge cases.
/// </summary>
public class ItemStackTests
{
    [Test]
    public void Constructor_SetsItemIdAndQuantity()
    {
        // Arrange
        string itemId = "test_item_001";
        int quantity = 5;
        
        // Act
        ItemStack stack = new ItemStack(itemId, quantity);
        
        // Assert
        Assert.AreEqual(itemId, stack.itemId);
        Assert.AreEqual(quantity, stack.quantity);
    }
    
    [Test]
    public void DefaultConstructor_SetsEmptyItemIdAndQuantityOne()
    {
        // Act
        ItemStack stack = new ItemStack();
        
        // Assert
        Assert.AreEqual(string.Empty, stack.itemId);
        Assert.AreEqual(1, stack.quantity);
    }
    
    [Test]
    public void CanStack_ReturnsTrue_WhenQuantityFits()
    {
        // Arrange
        ItemStack stack = new ItemStack("item1", 50);
        int maxStack = 99;
        int additionalQty = 49;
        
        // Act
        bool canStack = stack.CanStack(additionalQty, maxStack);
        
        // Assert
        Assert.IsTrue(canStack);
    }
    
    [Test]
    public void CanStack_ReturnsFalse_WhenQuantityExceeds()
    {
        // Arrange
        ItemStack stack = new ItemStack("item1", 50);
        int maxStack = 99;
        int additionalQty = 50;
        
        // Act
        bool canStack = stack.CanStack(additionalQty, maxStack);
        
        // Assert
        Assert.IsFalse(canStack);
    }
    
    [Test]
    public void GetSpaceRemaining_ReturnsCorrectSpace()
    {
        // Arrange
        ItemStack stack = new ItemStack("item1", 30);
        int maxStack = 99;
        
        // Act
        int space = stack.GetSpaceRemaining(maxStack);
        
        // Assert
        Assert.AreEqual(69, space);
    }
    
    [Test]
    public void AddQuantity_IncreasesQuantity()
    {
        // Arrange
        ItemStack stack = new ItemStack("item1", 10);
        
        // Act
        stack.AddQuantity(5);
        
        // Assert
        Assert.AreEqual(15, stack.quantity);
    }
    
    [Test]
    public void AddQuantity_NegativeValue_DecreasesQuantity()
    {
        // Arrange
        ItemStack stack = new ItemStack("item1", 10);
        
        // Act
        stack.AddQuantity(-5);
        
        // Assert
        Assert.AreEqual(5, stack.quantity);
    }
    
    [Test]
    public void AddQuantity_DoesNotGoBelowZero()
    {
        // Arrange
        ItemStack stack = new ItemStack("item1", 10);
        
        // Act
        stack.AddQuantity(-20);
        
        // Assert
        Assert.AreEqual(0, stack.quantity);
    }
    
    [Test]
    public void IsEmpty_ReturnsTrue_WhenQuantityIsZero()
    {
        // Arrange
        ItemStack stack = new ItemStack("item1", 0);
        
        // Assert
        Assert.IsTrue(stack.IsEmpty);
    }
    
    [Test]
    public void IsEmpty_ReturnsFalse_WhenQuantityIsPositive()
    {
        // Arrange
        ItemStack stack = new ItemStack("item1", 1);
        
        // Assert
        Assert.IsFalse(stack.IsEmpty);
    }
    
    [Test]
    public void Clone_CreatesIndependentCopy()
    {
        // Arrange
        ItemStack original = new ItemStack("item1", 10);
        
        // Act
        ItemStack clone = original.Clone();
        clone.quantity = 20;
        
        // Assert
        Assert.AreEqual(10, original.quantity);
        Assert.AreEqual(20, clone.quantity);
        Assert.AreEqual(original.itemId, clone.itemId);
    }
    
    [Test]
    public void StackMerge_Simulated_StacksCorrectly()
    {
        // Arrange
        ItemStack stack1 = new ItemStack("item1", 50);
        ItemStack stack2 = new ItemStack("item1", 30);
        int maxStack = 99;
        
        // Act & Assert - Simulate stacking logic
        int spaceRemaining = stack1.GetSpaceRemaining(maxStack);
        Assert.AreEqual(49, spaceRemaining);
        
        bool canStack = stack1.CanStack(stack2.quantity, maxStack);
        Assert.IsTrue(canStack); // 50 + 30 = 80 <= 99
    }
    
    [Test]
    public void StackSplit_PartialTransfer()
    {
        // Arrange
        ItemStack stack = new ItemStack("item1", 100);
        int splitAmount = 25;
        
        // Act - Simulate splitting
        stack.AddQuantity(-splitAmount);
        ItemStack newStack = new ItemStack("item1", splitAmount);
        
        // Assert
        Assert.AreEqual(75, stack.quantity);
        Assert.AreEqual(25, newStack.quantity);
        Assert.AreEqual(stack.itemId, newStack.itemId);
    }
    
    [Test]
    public void QuantityLimits_ExactMaxStack()
    {
        // Arrange
        ItemStack stack = new ItemStack("item1", 98);
        int maxStack = 99;
        
        // Act & Assert
        Assert.IsTrue(stack.CanStack(1, maxStack));
        Assert.IsFalse(stack.CanStack(2, maxStack));
    }
}