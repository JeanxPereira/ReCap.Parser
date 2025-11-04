using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace ReCap.Parser.Editor.Services
{
    public sealed class EditorSettings
    {
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public bool IsMaximized { get; set; }
        public string LastOpenDirectory { get; set; } = "";

        public IStorageFolder? GetStartFolder(IStorageProvider provider)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(LastOpenDirectory) && Directory.Exists(LastOpenDirectory))
                    return provider.TryGetFolderFromPath(LastOpenDirectory);
            }
            catch {}
            return null;
        }
    }

    public static class SettingsService
    {
        static readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ReCap.Parser", "Editor");
        static readonly string FilePath = Path.Combine(Dir, "settings.json");

        public static EditorSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    var s = JsonSerializer.Deserialize<EditorSettings>(json);
                    return s ?? new EditorSettings();
                }
            }
            catch {}
            return new EditorSettings();
        }

        public static void Save(EditorSettings s)
        {
            try
            {
                Directory.CreateDirectory(Dir);
                var json = JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch {}
        }

        public static IStorageFolder? TryGetFolderFromPath(this IStorageProvider provider, string path)
        {
            try
            {
                return provider.TryGetFolderFromPathAsync(path).GetAwaiter().GetResult();
            }
            catch { return null; }
        }

        static async Task<IStorageFolder?> TryGetFolderFromPathAsync(this IStorageProvider provider, string path)
        {
            try
            {
                var folder = await provider.TryGetFolderFromPathAsync(new Uri(new DirectoryInfo(path).FullName));
                return folder;
            }
            catch { return null; }
        }
    }
}
