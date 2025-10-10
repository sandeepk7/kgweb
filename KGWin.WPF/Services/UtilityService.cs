using KGWin.WPF.Models;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace KGWin.WPF.Services
{
    public class UtilityService
    {
        public static string GetFilePathFromDataFolder(string folderName, string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", folderName, fileName);
        }

        public static T? DeserializeJson<T>(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());

            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}
