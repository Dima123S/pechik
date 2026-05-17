using System.IO;
using System.Text.Json;

namespace PugSudoku.Models;

public class AppSettings
{
    private static readonly string FilePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    public string Theme { get; set; } = "light";
    public string Difficulty { get; set; } = "medium";
    public bool ShowDigits { get; set; } = true;
    public bool SoundEnabled { get; set; } = true;
    public bool PencilMode { get; set; }
    public bool HighlightErrors { get; set; } = true;
    public bool HighlightSameDigit { get; set; } = true;

    public AppSettings() => Load();

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    private void Load()
    {
        if (!File.Exists(FilePath)) return;
        try
        {
            var json = File.ReadAllText(FilePath);
            var s = JsonSerializer.Deserialize<AppSettings>(json);
            if (s == null) return;
            Theme = s.Theme;
            Difficulty = s.Difficulty;
            ShowDigits = s.ShowDigits;
            SoundEnabled = s.SoundEnabled;
            PencilMode = s.PencilMode;
            HighlightErrors = s.HighlightErrors;
            HighlightSameDigit = s.HighlightSameDigit;
        }
        catch { }
    }
}
