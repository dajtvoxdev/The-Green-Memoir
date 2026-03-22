using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Helpers for converting Firebase values to and from JSON safely.
/// Keeps backward compatibility with legacy data that was double-encoded as strings.
/// </summary>
public static class FirebaseJsonUtility
{
    /// <summary>
    /// Normalizes a value read from Firebase.
    /// If the payload was stored as a JSON string containing JSON, unwrap it.
    /// </summary>
    public static string NormalizeReadValue(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return rawJson;
        }

        string current = rawJson.Trim();

        // Legacy saves could double-encode payloads (for example "\"{...}\"").
        for (int i = 0; i < 4; i++)
        {
            if (!(current.StartsWith("\"") && current.EndsWith("\"")))
            {
                break;
            }

            try
            {
                string unwrapped = JsonConvert.DeserializeObject<string>(current);
                if (string.IsNullOrEmpty(unwrapped) || unwrapped == current)
                {
                    break;
                }

                current = unwrapped.Trim();
            }
            catch
            {
                break;
            }
        }

        return current;
    }

    /// <summary>
    /// Ensures a payload is valid JSON before sending it to Firebase.
    /// Plain strings are serialized into JSON strings automatically.
    /// </summary>
    public static string PrepareForWrite(string data)
    {
        if (data == null)
        {
            return "null";
        }

        string trimmed = data.Trim();
        if (trimmed.Length == 0)
        {
            return JsonConvert.SerializeObject(string.Empty);
        }

        if (IsValidJson(trimmed))
        {
            return trimmed;
        }

        return JsonConvert.SerializeObject(data);
    }

    /// <summary>
    /// Converts an arbitrary Firebase value back into a JSON string.
    /// </summary>
    public static string ConvertValueToJson(object value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is string stringValue)
        {
            return PrepareForWrite(NormalizeReadValue(stringValue));
        }

        return JsonConvert.SerializeObject(value);
    }

    private static bool IsValidJson(string value)
    {
        try
        {
            JToken.Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
