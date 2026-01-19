using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;

namespace MusicRando.Cache;

internal static class JsonHelper
{
    public static void SerializeToFile<T>(this T self, string filePath)
    {
        string json = JsonConvert.SerializeObject(self, Formatting.Indented);

        File.WriteAllText(filePath, json);
    }

    public static bool TryLoadFromFile<T>(string filePath, [NotNullWhen(true)] out T? obj)
    {
        obj = default;

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            obj = JsonConvert.DeserializeObject<T>(json);
            return obj != null;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
