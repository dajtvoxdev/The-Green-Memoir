using System;
using UnityEngine;

/// <summary>
/// Represents a stack of items in the inventory.
/// Used for item stacking system where identical items can stack up to a maximum quantity.
/// </summary>
[Serializable]
public class ItemStack
{
    /// <summary>
    /// Unique identifier linking to ItemDefinition.
    /// </summary>
    public string itemId;
    
    /// <summary>
    /// Current quantity of items in this stack.
    /// </summary>
    public int quantity;
    
    /// <summary>
    /// Default constructor for JSON serialization.
    /// </summary>
    public ItemStack()
    {
        itemId = string.Empty;
        quantity = 1;
    }
    
    /// <summary>
    /// Creates a new item stack with the specified item and quantity.
    /// </summary>
    /// <param name="itemId">The unique item identifier</param>
    /// <param name="quantity">Initial quantity (default: 1)</param>
    public ItemStack(string itemId, int quantity = 1)
    {
        this.itemId = itemId;
        this.quantity = quantity;
    }
    
    /// <summary>
    /// Checks if additional quantity can be added to this stack.
    /// </summary>
    /// <param name="additionalQty">Quantity to add</param>
    /// <param name="maxStack">Maximum stack size allowed</param>
    /// <returns>True if the additional quantity fits in the stack</returns>
    public bool CanStack(int additionalQty, int maxStack)
    {
        return quantity + additionalQty <= maxStack;
    }
    
    /// <summary>
    /// Gets the remaining space in this stack.
    /// </summary>
    /// <param name="maxStack">Maximum stack size allowed</param>
    /// <returns>Number of items that can still be added</returns>
    public int GetSpaceRemaining(int maxStack)
    {
        return maxStack - quantity;
    }
    
    /// <summary>
    /// Adds quantity to this stack.
    /// </summary>
    /// <param name="amount">Amount to add (can be negative for removal)</param>
    public void AddQuantity(int amount)
    {
        quantity = Mathf.Max(0, quantity + amount);
    }
    
    /// <summary>
    /// Checks if this stack is empty (quantity is 0).
    /// </summary>
    public bool IsEmpty => quantity <= 0;
    
    /// <summary>
    /// Creates a copy of this item stack.
    /// </summary>
    public ItemStack Clone()
    {
        return new ItemStack(itemId, quantity);
    }
}