using System.Collections.Generic;

public class CacheUtil {
    
    private static Dictionary<string, string> cacheData = new Dictionary<string, string>();

    public static void SetData(string key, string value)
    {
        if (!string.IsNullOrEmpty(key))
        {
            cacheData[key] = value;
        }
    }

    public static string GetData(string key)
    {
        string data;

        if (cacheData.TryGetValue(key, out data))
        {
            return data;
        }

        return null;
    }
}
