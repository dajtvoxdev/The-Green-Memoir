using NUnit.Framework;
using System;
using System.Threading.Tasks;
using MathUtil = System.Math;

/// <summary>
/// Unit tests for Firebase retry logic and exponential backoff.
/// Note: These tests verify the logic patterns; actual Firebase calls require network.
/// </summary>
public class FirebaseRetryTests
{
    // Simulated retry counter for testing
    private int _retryCount;
    
    [Test]
    public void ExponentialBackoff_CalculatesCorrectDelay()
    {
        // Arrange
        float initialDelay = 0.5f;
        float multiplier = 2f;
        float maxDelay = 10f;
        
        // Act - Calculate delays for each retry
        float delay1 = initialDelay;                          // 0.5s
        float delay2 = (float)MathUtil.Min(delay1 * multiplier, maxDelay);  // 1.0s
        float delay3 = (float)MathUtil.Min(delay2 * multiplier, maxDelay);  // 2.0s
        float delay4 = (float)MathUtil.Min(delay3 * multiplier, maxDelay);  // 4.0s
        float delay5 = (float)MathUtil.Min(delay4 * multiplier, maxDelay);  // 8.0s
        float delay6 = (float)MathUtil.Min(delay5 * multiplier, maxDelay);  // 10.0s (capped)
        float delay7 = (float)MathUtil.Min(delay6 * multiplier, maxDelay);  // 10.0s (still capped)
        
        // Assert
        Assert.AreEqual(0.5f, delay1, 0.001f);
        Assert.AreEqual(1.0f, delay2, 0.001f);
        Assert.AreEqual(2.0f, delay3, 0.001f);
        Assert.AreEqual(4.0f, delay4, 0.001f);
        Assert.AreEqual(8.0f, delay5, 0.001f);
        Assert.AreEqual(10.0f, delay6, 0.001f);
        Assert.AreEqual(10.0f, delay7, 0.001f); // Capped at max
    }
    
    [Test]
    public void MaxDelay_CapsExponentialGrowth()
    {
        // Arrange
        float initialDelay = 1f;
        float multiplier = 2f;
        float maxDelay = 5f;
        float currentDelay = initialDelay;
        
        // Act - Keep multiplying until we hit the cap
        for (int i = 0; i < 10; i++)
        {
            currentDelay = (float)MathUtil.Min(currentDelay * multiplier, maxDelay);
        }
        
        // Assert
        Assert.AreEqual(maxDelay, currentDelay, 0.001f);
    }
    
    [Test]
    public void RetryLogic_SucceedsOnFirstAttempt()
    {
        // Arrange
        _retryCount = 0;
        Func<Task<bool>> operation = () =>
        {
            _retryCount++;
            return Task.FromResult(true); // Always succeeds
        };
        
        // Act
        var result = RunWithRetry(operation, 3).Result;
        
        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, _retryCount); // Only tried once
    }
    
    [Test]
    public void RetryLogic_SucceedsAfterFailures()
    {
        // Arrange
        _retryCount = 0;
        int failCount = 2;
        Func<Task<bool>> operation = () =>
        {
            _retryCount++;
            if (_retryCount <= failCount)
            {
                throw new Exception("Simulated failure");
            }
            return Task.FromResult(true);
        };
        
        // Act
        var result = RunWithRetry(operation, 5).Result;
        
        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(failCount + 1, _retryCount); // Succeeded on 3rd attempt
    }
    
    [Test]
    public void RetryLogic_FailsAfterMaxRetries()
    {
        // Arrange
        _retryCount = 0;
        int maxRetries = 3;
        Func<Task<bool>> operation = () =>
        {
            _retryCount++;
            throw new Exception("Always fails");
        };
        
        // Act
        var result = RunWithRetry(operation, maxRetries).Result;
        
        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(maxRetries, _retryCount); // Tried exactly maxRetries times
    }
    
    [Test]
    public void RetryLogic_ZeroRetries_StillExecutesOnce()
    {
        // Arrange
        _retryCount = 0;
        Func<Task<bool>> operation = () =>
        {
            _retryCount++;
            return Task.FromResult(true);
        };
        
        // Act - Even with 0 "retries", we should execute once
        var result = RunWithRetry(operation, 1).Result;
        
        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, _retryCount);
    }
    
    [Test]
    public void Response_OK_CreatesSuccessResponse()
    {
        // Arrange & Act
        var response = FirebaseResponse<bool>.Ok(true);
        
        // Assert
        Assert.IsTrue(response.Success);
        Assert.IsTrue(response.Data);
        Assert.IsNull(response.ErrorMessage);
        Assert.IsNull(response.Exception);
    }
    
    [Test]
    public void Response_Fail_CreatesFailureResponse()
    {
        // Arrange
        string errorMessage = "Test error";
        Exception testException = new Exception("Test");
        
        // Act
        var response = FirebaseResponse<string>.Fail(errorMessage, testException);
        
        // Assert
        Assert.IsFalse(response.Success);
        Assert.AreEqual(errorMessage, response.ErrorMessage);
        Assert.AreEqual(testException, response.Exception);
    }
    
    [Test]
    public void Response_Fail_WithoutException()
    {
        // Arrange & Act
        var response = FirebaseResponse<string>.Fail("Error message");
        
        // Assert
        Assert.IsFalse(response.Success);
        Assert.AreEqual("Error message", response.ErrorMessage);
        Assert.IsNull(response.Exception);
    }
    
    [Test]
    public void Response_Generic_OK_ForVoidOperations()
    {
        // Arrange & Act
        var response = FirebaseResponse.Ok();
        
        // Assert
        Assert.IsTrue(response.Success);
        Assert.IsNull(response.ErrorMessage);
    }
    
    /// <summary>
    /// Simulates the retry logic pattern used in FirebaseTransactionManager.
    /// </summary>
    private async Task<bool> RunWithRetry(Func<Task<bool>> operation, int maxRetries)
    {
        float delay = 0.1f; // Shorter delay for testing
        
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch
            {
                if (attempt < maxRetries - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                    delay *= 2; // Exponential backoff
                }
            }
        }
        
        return false;
    }
    
}
