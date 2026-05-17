using System.IO;
using System.Text.Json;

namespace PugSudoku.Models;

public class ScoreManager
{
    private static readonly string FilePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "records.json");

    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int BestTimeEasy { get; set; } = int.MaxValue;
    public int BestTimeMedium { get; set; } = int.MaxValue;
    public int BestTimeHard { get; set; } = int.MaxValue;
    public int TotalHints { get; set; }
    public int TotalErrors { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }

    public ScoreManager() => Load();

    public void RecordWin(string difficulty, int seconds, int hints, int errors)
    {
        GamesPlayed++;
        GamesWon++;
        TotalHints += hints;
        TotalErrors += errors;
        CurrentStreak++;
        if (CurrentStreak > BestStreak) BestStreak = CurrentStreak;

        switch (difficulty)
        {
            case "easy":   if (seconds < BestTimeEasy) BestTimeEasy = seconds; break;
            case "medium": if (seconds < BestTimeMedium) BestTimeMedium = seconds; break;
            case "hard":   if (seconds < BestTimeHard) BestTimeHard = seconds; break;
        }
        Save();
    }

    public void RecordLoss()
    {
        GamesPlayed++;
        CurrentStreak = 0;
        Save();
    }

    public string FormatBest(int val) =>
        val == int.MaxValue ? "--:--" : $"{val / 60:D2}:{val % 60:D2}";

    public void ResetAll()
    {
        GamesPlayed = GamesWon = TotalHints = TotalErrors = CurrentStreak = BestStreak = 0;
        BestTimeEasy = BestTimeMedium = BestTimeHard = int.MaxValue;
        Save();
    }

    private void Load()
    {
        if (!File.Exists(FilePath)) return;
        try
        {
            var json = File.ReadAllText(FilePath);
            var d = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
            if (d == null) return;
            if (d.TryGetValue("games_played", out var v)) GamesPlayed = v;
            if (d.TryGetValue("games_won", out v)) GamesWon = v;
            if (d.TryGetValue("best_easy", out v)) BestTimeEasy = v;
            if (d.TryGetValue("best_medium", out v)) BestTimeMedium = v;
            if (d.TryGetValue("best_hard", out v)) BestTimeHard = v;
            if (d.TryGetValue("total_hints", out v)) TotalHints = v;
            if (d.TryGetValue("total_errors", out v)) TotalErrors = v;
            if (d.TryGetValue("streak", out v)) CurrentStreak = v;
            if (d.TryGetValue("best_streak", out v)) BestStreak = v;
        }
        catch { }
    }

    private void Save()
    {
        var d = new Dictionary<string, int>
        {
            ["games_played"] = GamesPlayed, ["games_won"] = GamesWon,
            ["best_easy"] = BestTimeEasy, ["best_medium"] = BestTimeMedium,
            ["best_hard"] = BestTimeHard, ["total_hints"] = TotalHints,
            ["total_errors"] = TotalErrors, ["streak"] = CurrentStreak,
            ["best_streak"] = BestStreak,
        };
        File.WriteAllText(FilePath, JsonSerializer.Serialize(d, new JsonSerializerOptions { WriteIndented = true }));
    }
}
