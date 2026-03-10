using System;

/// <summary>
/// Wrapper class for Firebase operation results.
/// Provides a type-safe way to handle success/failure states with data or error information.
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class FirebaseResponse<T>
{
    /// <summary>
    /// True if the operation succeeded, false otherwise.
    /// </summary>
    public bool Success { get; private set; }
    
    /// <summary>
    /// The data returned from the operation (null if failed).
    /// </summary>
    public T Data { get; private set; }
    
    /// <summary>
    /// Error message describing what went wrong (null if succeeded).
    /// </summary>
    public string ErrorMessage { get; private set; }
    
    /// <summary>
    /// The exception that caused the failure (null if succeeded).
    /// </summary>
    public Exception Exception { get; private set; }
    
    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    /// <param name="data">The data to return</param>
    /// <returns>A successful FirebaseResponse</returns>
    public static FirebaseResponse<T> Ok(T data) 
        => new FirebaseResponse<T> { Success = true, Data = data };
    
    /// <summary>
    /// Creates a failed response with an error message.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="ex">Optional exception that caused the failure</param>
    /// <returns>A failed FirebaseResponse</returns>
    public static FirebaseResponse<T> Fail(string message, Exception ex = null) 
        => new FirebaseResponse<T> { Success = false, ErrorMessage = message, Exception = ex };
}

/// <summary>
/// Non-generic FirebaseResponse for operations that don't return data.
/// </summary>
public class FirebaseResponse
{
    /// <summary>
    /// True if the operation succeeded, false otherwise.
    /// </summary>
    public bool Success { get; private set; }
    
    /// <summary>
    /// Error message describing what went wrong (null if succeeded).
    /// </summary>
    public string ErrorMessage { get; private set; }
    
    /// <summary>
    /// The exception that caused the failure (null if succeeded).
    /// </summary>
    public Exception Exception { get; private set; }
    
    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <returns>A successful FirebaseResponse</returns>
    public static FirebaseResponse Ok() 
        => new FirebaseResponse { Success = true };
    
    /// <summary>
    /// Creates a failed response with an error message.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="ex">Optional exception that caused the failure</param>
    /// <returns>A failed FirebaseResponse</returns>
    public static FirebaseResponse Fail(string message, Exception ex = null) 
        => new FirebaseResponse { Success = false, ErrorMessage = message, Exception = ex };
}