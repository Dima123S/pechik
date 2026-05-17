using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using PugSudoku.Drawing;
using PugSudoku.Models;

namespace PugSudoku;

public partial class MainWindow : Window
{
    private readonly AppSettings _settings = new();
    private readonly ScoreManager _score = new();
    private SudokuBoard? _board;
    private ThemeColors T => Themes.Get(_settings.Theme);

    private const double CellSize = 52;
    private const double ThinLine = 1;
    private const double ThickLine = 3;
    private const double BoardPad = 6;

    private Canvas? _gameCanvas;
    private TextBlock? _timerText;
    private TextBlock? _errorsText;
    private TextBlock? _hintsText;
    private TextBlock? _emptyText;
    private DispatcherTimer? _clock;
    private int _elapsedSeconds;
    private (int r, int c)? _selected;
    private bool _pencilMode;

    public MainWindow()
    {
        InitializeComponent();
        ShowMenu();
    }

    private void SetScreen(UIElement content)
    {
        _clock?.Stop();
        RootGrid.Children.Clear();
        RootGrid.Background = T.Bg;
        RootGrid.Children.Add(content);
    }

    // ===================================================================
    //  MAIN MENU
    // ===================================================================
    private void ShowMenu()
    {
        _clock?.Stop();
        var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

        var titleCv = new Canvas { Width = 560, Height = 200, Background = Brushes.Transparent };
        DrawPawPrints(titleCv, 560, 200);

        AddText(titleCv, 280, 35, "PUG", 36, FontWeights.Bold,
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B6914")!));
        AddText(titleCv, 280, 78, "SUDOKU", 36, FontWeights.Bold, T.Accent);
        AddText(titleCv, 280, 120, "\u2764 Mopsowe Sudoku \u2764", 13, FontWeights.Normal, T.TextLight);

        PugDrawer.Draw(titleCv, 1, 100, 165, 60);
        PugDrawer.Draw(titleCv, 5, 220, 170, 50);
        PugDrawer.Draw(titleCv, 2, 340, 170, 50);
        PugDrawer.Draw(titleCv, 7, 460, 165, 60);

        sp.Children.Add(titleCv);

        sp.Children.Add(new TextBlock
        {
            Text = "Wypelnij plansze 9x9 mopsami \u2022 Kazdy mops pojawia sie raz w wierszu, kolumnie i bloku!",
            FontSize = 11, Foreground = T.TextLight, TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center, MaxWidth = 460,
            Margin = new Thickness(0, 0, 0, 8)
        });

        var btnData = new (string text, string sub, int icon, Action action)[]
        {
            ("LATWY", "mniej pustych pol", 1, () => StartGame("easy")),
            ("SREDNI", "zrownowazony", 5, () => StartGame("medium")),
            ("TRUDNY", "ekspert mopsolog", 2, () => StartGame("hard")),
            ("REKORDY", "statystyki", 0, ShowRecords),
            ("USTAWIENIA", "dostosuj gre", 0, ShowSettings),
            ("WYJSCIE", "zamknij", 0, Close),
        };

        foreach (var (text, sub, icon, action) in btnData)
        {
            var btn = MakeMenuButton(text, sub, icon, 340, 50);
            btn.MouseLeftButtonDown += (_, _) => action();
            btn.Margin = new Thickness(0, 4, 0, 0);
            sp.Children.Add(btn);
        }

        SetScreen(sp);
    }

    // ===================================================================
    //  GAME SCREEN
    // ===================================================================
    private void StartGame(string difficulty)
    {
        _settings.Difficulty = difficulty;
        var (sol, puz) = SudokuGenerator.Generate(difficulty);
        _board = new SudokuBoard();
        _board.Setup(sol, puz);
        _selected = null;
        _pencilMode = _settings.PencilMode;
        _elapsedSeconds = 0;
        ShowGame();
    }

    private void ShowGame()
    {
        var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

        // Top bar
        var top = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 2) };
        top.Children.Add(MakeTextButton("\u2190", 36, 36, ShowMenu));
        top.Children.Add(new TextBlock
        {
            Text = "\u2764 SUDOKU \u2764", FontSize = 20, FontWeight = FontWeights.Bold,
            Foreground = T.Text, Margin = new Thickness(12, 5, 0, 0)
        });
        var diffLabel = new TextBlock
        {
            Text = _settings.Difficulty switch { "easy" => "Latwy", "hard" => "Trudny", _ => "Sredni" },
            FontSize = 12, Foreground = T.Accent, FontWeight = FontWeights.Bold,
            Margin = new Thickness(12, 8, 0, 0), VerticalAlignment = VerticalAlignment.Center
        };
        top.Children.Add(diffLabel);
        sp.Children.Add(top);

        // Info row
        var infoRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 3),
            HorizontalAlignment = HorizontalAlignment.Center };
        _timerText = new TextBlock { Text = "00:00", FontSize = 14, Foreground = T.Text, FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 20, 0), VerticalAlignment = VerticalAlignment.Center };
        infoRow.Children.Add(_timerText);
        _errorsText = new TextBlock { Text = $"Bledy: {_board!.ErrorCount}", FontSize = 12, Foreground = T.TextLight,
            Margin = new Thickness(0, 0, 20, 0), VerticalAlignment = VerticalAlignment.Center };
        infoRow.Children.Add(_errorsText);
        _hintsText = new TextBlock { Text = $"Podpowiedzi: {_board.HintsUsed}", FontSize = 12, Foreground = T.TextLight,
            Margin = new Thickness(0, 0, 20, 0), VerticalAlignment = VerticalAlignment.Center };
        infoRow.Children.Add(_hintsText);
        _emptyText = new TextBlock { Text = $"Puste: {_board.EmptyCount()}", FontSize = 12, Foreground = T.TextLight,
            VerticalAlignment = VerticalAlignment.Center };
        infoRow.Children.Add(_emptyText);
        sp.Children.Add(infoRow);

        // Board
        double boardSz = 9 * CellSize + 4 * ThickLine + 6 * ThinLine + BoardPad * 2;
        _gameCanvas = new Canvas { Width = boardSz, Height = boardSz, Background = Brushes.Transparent };
        _gameCanvas.MouseLeftButtonDown += OnBoardClick;

        var border = new Border
        {
            Child = _gameCanvas, Background = T.CardBg,
            BorderBrush = T.Border, BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(10), Margin = new Thickness(6, 0, 6, 0)
        };
        sp.Children.Add(border);
        DrawBoard();

        // Digit buttons (9 pugs in one row)
        var digitRow = new StackPanel { Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 4, 0, 0) };
        for (int d = 1; d <= 9; d++)
        {
            int digit = d;
            var digitBtn = MakeDigitButton(digit);
            digitBtn.MouseLeftButtonDown += (_, _) => OnDigitPress(digit);
            digitRow.Children.Add(digitBtn);
        }
        sp.Children.Add(digitRow);

        // Action buttons row
        var actRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 2),
            HorizontalAlignment = HorizontalAlignment.Center };

        var eraseBtn = MakeMenuButton("WYMAZAC", "", 0, 100, 38);
        eraseBtn.MouseLeftButtonDown += (_, _) => OnErase();
        eraseBtn.Margin = new Thickness(0, 0, 6, 0);
        actRow.Children.Add(eraseBtn);

        var pencilBtn = MakeMenuButton(_pencilMode ? "\u270F OLOWEK: WL" : "\u270F OLOWEK: WYL", "", 0, 140, 38);
        pencilBtn.MouseLeftButtonDown += (_, _) => { _pencilMode = !_pencilMode; ShowGame(); };
        pencilBtn.Margin = new Thickness(0, 0, 6, 0);
        actRow.Children.Add(pencilBtn);

        var hintBtn = MakeMenuButton("PODPOWIEDZ", "", 0, 120, 38);
        hintBtn.MouseLeftButtonDown += (_, _) => OnHint();
        hintBtn.Margin = new Thickness(0, 0, 6, 0);
        actRow.Children.Add(hintBtn);

        var newBtn = MakeMenuButton("NOWA GRA", "", 0, 100, 38);
        newBtn.MouseLeftButtonDown += (_, _) => StartGame(_settings.Difficulty);
        actRow.Children.Add(newBtn);

        sp.Children.Add(actRow);

        SetScreen(sp);

        // Timer
        _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clock.Tick += (_, _) =>
        {
            _elapsedSeconds++;
            if (_timerText != null) _timerText.Text = $"{_elapsedSeconds / 60:D2}:{_elapsedSeconds % 60:D2}";
        };
        _clock.Start();

        KeyDown -= OnKeyDown;
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (_board == null || _selected == null) return;
        int digit = e.Key switch
        {
            Key.D1 or Key.NumPad1 => 1, Key.D2 or Key.NumPad2 => 2, Key.D3 or Key.NumPad3 => 3,
            Key.D4 or Key.NumPad4 => 4, Key.D5 or Key.NumPad5 => 5, Key.D6 or Key.NumPad6 => 6,
            Key.D7 or Key.NumPad7 => 7, Key.D8 or Key.NumPad8 => 8, Key.D9 or Key.NumPad9 => 9,
            _ => 0
        };
        if (digit > 0) OnDigitPress(digit);
        if (e.Key == Key.Delete || e.Key == Key.Back) OnErase();
        if (e.Key == Key.P) { _pencilMode = !_pencilMode; ShowGame(); }

        // Arrow navigation
        if (_selected.HasValue)
        {
            var (r, c) = _selected.Value;
            if (e.Key == Key.Up && r > 0) _selected = (r - 1, c);
            else if (e.Key == Key.Down && r < 8) _selected = (r + 1, c);
            else if (e.Key == Key.Left && c > 0) _selected = (r, c - 1);
            else if (e.Key == Key.Right && c < 8) _selected = (r, c + 1);
            DrawBoard();
        }
    }

    private double CellX(int c)
    {
        int thickBefore = c / 3 + 1;
        int thinBefore = c - c / 3;
        return BoardPad + thickBefore * ThickLine + thinBefore * ThinLine + c * CellSize;
    }

    private double CellY(int r)
    {
        int thickBefore = r / 3 + 1;
        int thinBefore = r - r / 3;
        return BoardPad + thickBefore * ThickLine + thinBefore * ThinLine + r * CellSize;
    }

    private void DrawBoard()
    {
        _gameCanvas!.Children.Clear();
        var b = _board!;
        int selVal = _selected.HasValue ? b.Current[_selected.Value.r, _selected.Value.c] : 0;

        for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
        {
            double x = CellX(c), y = CellY(r);

            bool isSelected = _selected.HasValue && _selected.Value.r == r && _selected.Value.c == c;
            bool sameBox = _selected.HasValue &&
                           _selected.Value.r / 3 == r / 3 && _selected.Value.c / 3 == c / 3;
            bool sameLine = _selected.HasValue &&
                            (_selected.Value.r == r || _selected.Value.c == c);
            bool hasConflict = b.Current[r, c] != 0 && b.HasConflict(r, c) && !b.IsFixed[r, c]
                               && _settings.HighlightErrors;
            bool sameDigit = selVal != 0 && b.Current[r, c] == selVal && _settings.HighlightSameDigit;

            Brush bg;
            if (isSelected) bg = T.SelectedCellBg;
            else if (hasConflict) bg = T.ErrorBg;
            else if (sameDigit) bg = T.WinHighlight;
            else if (sameBox || sameLine) bg = T.SameGroupBg;
            else if (b.IsFixed[r, c]) bg = T.FixedCellBg;
            else bg = T.CellBg;

            var rect = new Rectangle
            {
                Width = CellSize, Height = CellSize, Fill = bg,
                Stroke = T.Border, StrokeThickness = 0.3
            };
            Canvas.SetLeft(rect, x); Canvas.SetTop(rect, y);
            _gameCanvas.Children.Add(rect);

            int val = b.Current[r, c];
            if (val != 0)
            {
                double pugSz = CellSize * 0.78;
                PugDrawer.DrawWithNumber(_gameCanvas, val, x + CellSize / 2, y + CellSize / 2 - 2,
                    pugSz, _settings.ShowDigits);
            }
            else if (b.Pencil[r, c].Count > 0)
            {
                foreach (int p in b.Pencil[r, c])
                {
                    int pr = (p - 1) / 3, pc = (p - 1) % 3;
                    double px = x + 4 + pc * (CellSize - 8) / 3.0 + (CellSize - 8) / 6.0;
                    double py = y + 4 + pr * (CellSize - 8) / 3.0 + (CellSize - 8) / 6.0;
                    PugDrawer.Draw(_gameCanvas, p, px, py, 14);
                }
            }
        }

        // thick grid lines (3x3 boxes)
        var lineBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5D4037")!);
        for (int i = 0; i <= 3; i++)
        {
            double pos = BoardPad + i * (3 * CellSize + 2 * ThinLine + ThickLine) + ThickLine / 2;
            double end = CellY(8) + CellSize + 2;
            _gameCanvas.Children.Add(new Line
            {
                X1 = pos, Y1 = BoardPad, X2 = pos, Y2 = end,
                Stroke = lineBrush, StrokeThickness = ThickLine
            });
            _gameCanvas.Children.Add(new Line
            {
                X1 = BoardPad, Y1 = pos, X2 = end, Y2 = pos,
                Stroke = lineBrush, StrokeThickness = ThickLine
            });
        }
    }

    private (int r, int c)? HitTest(double mx, double my)
    {
        for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
        {
            double x = CellX(c), y = CellY(r);
            if (mx >= x && mx < x + CellSize && my >= y && my < y + CellSize)
                return (r, c);
        }
        return null;
    }

    private void OnBoardClick(object sender, MouseButtonEventArgs e)
    {
        if (_board == null) return;
        var pos = e.GetPosition(_gameCanvas);
        _selected = HitTest(pos.X, pos.Y);
        DrawBoard();
    }

    private void OnDigitPress(int digit)
    {
        if (_board == null || _selected == null) return;
        var (r, c) = _selected.Value;
        if (_board.IsFixed[r, c]) return;

        if (_pencilMode)
        {
            _board.TogglePencil(r, c, digit);
        }
        else
        {
            _board.Place(r, c, digit);
            UpdateInfo();
            if (_board.IsSolved())
            {
                _clock?.Stop();
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
                timer.Tick += (_, _) => { timer.Stop(); ShowVictory(); };
                timer.Start();
                return;
            }
        }
        DrawBoard();
    }

    private void OnErase()
    {
        if (_board == null || _selected == null) return;
        var (r, c) = _selected.Value;
        _board.Erase(r, c);
        _board.Pencil[r, c].Clear();
        DrawBoard();
        UpdateInfo();
    }

    private void OnHint()
    {
        if (_board == null) return;
        _board.RevealHint();
        DrawBoard();
        UpdateInfo();
        if (_board.IsSolved())
        {
            _clock?.Stop();
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            timer.Tick += (_, _) => { timer.Stop(); ShowVictory(); };
            timer.Start();
        }
    }

    private void UpdateInfo()
    {
        if (_errorsText != null) _errorsText.Text = $"Bledy: {_board!.ErrorCount}";
        if (_hintsText != null) _hintsText.Text = $"Podpowiedzi: {_board!.HintsUsed}";
        if (_emptyText != null) _emptyText.Text = $"Puste: {_board!.EmptyCount()}";
    }

    // ===================================================================
    //  VICTORY
    // ===================================================================
    private void ShowVictory()
    {
        _score.RecordWin(_settings.Difficulty, _elapsedSeconds, _board!.HintsUsed, _board.ErrorCount);

        var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        var cv = new Canvas { Width = 560, Height = 360, Background = Brushes.Transparent };
        sp.Children.Add(cv);

        var rng = new Random(7);
        string[] cols = ["#FFD700", "#FF69B4", "#87CEEB", "#98FB98", "#DDA0DD", "#FFA07A"];
        for (int i = 0; i < 60; i++)
        {
            double cx = rng.Next(20, 540), cy = rng.Next(10, 200), sz = rng.Next(3, 10);
            var dot = new Ellipse { Width = sz * 2, Height = sz * 2,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cols[rng.Next(cols.Length)])!) };
            Canvas.SetLeft(dot, cx - sz); Canvas.SetTop(dot, cy - sz);
            cv.Children.Add(dot);
        }

        AddText(cv, 280, 50, "BRAWO!", 34, FontWeights.Bold, T.Accent);
        AddText(cv, 280, 90, "Ukonczyles sudoku!", 16, FontWeights.Normal, T.Text);

        for (int d = 1; d <= 9; d++)
            PugDrawer.Draw(cv, d, 40 + (d - 1) * 56, 155, 44);

        AddText(cv, 280, 210, $"Czas: {_elapsedSeconds / 60:D2}:{_elapsedSeconds % 60:D2}", 18, FontWeights.Bold, T.Text);
        AddText(cv, 280, 240, $"Bledy: {_board.ErrorCount}  |  Podpowiedzi: {_board.HintsUsed}", 13, FontWeights.Normal, T.TextLight);

        string diffPl = _settings.Difficulty switch { "easy" => "Latwy", "hard" => "Trudny", _ => "Sredni" };
        AddText(cv, 280, 270, $"Poziom: {diffPl}", 13, FontWeights.Normal, T.TextLight);

        bool isBest = _settings.Difficulty switch
        {
            "easy" => _elapsedSeconds <= _score.BestTimeEasy,
            "hard" => _elapsedSeconds <= _score.BestTimeHard,
            _ => _elapsedSeconds <= _score.BestTimeMedium,
        };
        if (isBest)
            AddText(cv, 280, 300, "\u2B50 NOWY REKORD! \u2B50", 16, FontWeights.Bold,
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700")!));

        var btnRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center };
        var again = MakeMenuButton("JESZCZE RAZ", "", 0, 160, 46);
        again.MouseLeftButtonDown += (_, _) => StartGame(_settings.Difficulty);
        again.Margin = new Thickness(0, 0, 10, 0);
        btnRow.Children.Add(again);
        var menu = MakeMenuButton("DO MENU", "", 0, 160, 46);
        menu.MouseLeftButtonDown += (_, _) => ShowMenu();
        btnRow.Children.Add(menu);
        sp.Children.Add(btnRow);

        SetScreen(sp);
    }

    // ===================================================================
    //  RECORDS
    // ===================================================================
    private void ShowRecords()
    {
        var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

        var top = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 16, 0, 10) };
        top.Children.Add(MakeTextButton("\u2190", 36, 36, ShowMenu));
        top.Children.Add(new TextBlock
        {
            Text = "\u2764 REKORDY \u2764", FontSize = 22, FontWeight = FontWeights.Bold,
            Foreground = T.Text, Margin = new Thickness(20, 4, 0, 0)
        });
        sp.Children.Add(top);

        var card = new Border
        {
            Width = 400, Background = T.CardBg,
            CornerRadius = new CornerRadius(16), BorderBrush = T.Border, BorderThickness = new Thickness(1),
            Padding = new Thickness(20), Margin = new Thickness(0, 10, 0, 10)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var stats = new (string label, string value, bool hl)[]
        {
            ("\U0001f3ae  Rozegrane gry", _score.GamesPlayed.ToString(), false),
            ("\U0001f3c6  Wygrane", _score.GamesWon.ToString(), true),
            ("\U0001f552  Najlepszy (Latwy)", _score.FormatBest(_score.BestTimeEasy), false),
            ("\U0001f552  Najlepszy (Sredni)", _score.FormatBest(_score.BestTimeMedium), true),
            ("\U0001f552  Najlepszy (Trudny)", _score.FormatBest(_score.BestTimeHard), false),
            ("\U0001f4a1  Uzyte podpowiedzi", _score.TotalHints.ToString(), false),
            ("\u274C  Laczne bledy", _score.TotalErrors.ToString(), false),
            ("\U0001f525  Wygrane z rzedu", _score.CurrentStreak.ToString(), true),
            ("\U0001f451  Najlepsza seria", _score.BestStreak.ToString(), true),
        };

        for (int i = 0; i < stats.Length; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            var lbl = new TextBlock { Text = stats[i].label, FontSize = 13, Foreground = T.Text,
                VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(lbl, i); Grid.SetColumn(lbl, 0);
            grid.Children.Add(lbl);

            var val = new TextBlock { Text = stats[i].value, FontSize = 16, FontWeight = FontWeights.Bold,
                Foreground = stats[i].hl ? T.Accent : T.Text,
                VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetRow(val, i); Grid.SetColumn(val, 1);
            grid.Children.Add(val);
        }

        card.Child = grid;
        sp.Children.Add(card);

        var pugCv = new Canvas { Width = 560, Height = 60, Background = Brushes.Transparent };
        PugDrawer.Draw(pugCv, 3, 280, 30, 50);
        sp.Children.Add(pugCv);

        var resetBtn = MakeMenuButton("WYCZYSC REKORDY", "", 0, 220, 42);
        resetBtn.MouseLeftButtonDown += (_, _) => { _score.ResetAll(); ShowRecords(); };
        resetBtn.Margin = new Thickness(0, 6, 0, 0);
        sp.Children.Add(resetBtn);

        SetScreen(sp);
    }

    // ===================================================================
    //  SETTINGS
    // ===================================================================
    private void ShowSettings()
    {
        var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, Width = 420 };

        var top = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 16, 0, 10) };
        top.Children.Add(MakeTextButton("\u2190", 36, 36, ShowMenu));
        top.Children.Add(new TextBlock
        {
            Text = "\u2764 USTAWIENIA \u2764", FontSize = 22, FontWeight = FontWeights.Bold,
            Foreground = T.Text, Margin = new Thickness(10, 4, 0, 0)
        });
        sp.Children.Add(top);

        // Theme
        sp.Children.Add(SectionLabel("MOTYW"));
        var themeRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(30, 0, 0, 0) };
        themeRow.Children.Add(MakeToggle("\u2600 Jasny", _settings.Theme == "light",
            () => { _settings.Theme = "light"; _settings.Save(); ShowSettings(); }));
        themeRow.Children.Add(MakeToggle("\U0001f319 Ciemny", _settings.Theme == "dark",
            () => { _settings.Theme = "dark"; _settings.Save(); ShowSettings(); }));
        sp.Children.Add(themeRow);

        // Display
        sp.Children.Add(SectionLabel("WYSWIETLANIE"));
        sp.Children.Add(MakeSwitch("Pokazuj cyfry", _settings.ShowDigits,
            () => { _settings.ShowDigits = !_settings.ShowDigits; _settings.Save(); ShowSettings(); }));
        sp.Children.Add(MakeSwitch("Podswietlaj bledy", _settings.HighlightErrors,
            () => { _settings.HighlightErrors = !_settings.HighlightErrors; _settings.Save(); ShowSettings(); }));
        sp.Children.Add(MakeSwitch("Podswietlaj te same", _settings.HighlightSameDigit,
            () => { _settings.HighlightSameDigit = !_settings.HighlightSameDigit; _settings.Save(); ShowSettings(); }));

        // Pug legend
        sp.Children.Add(SectionLabel("MOPSY - LEGENDA"));
        var legendGrid = new WrapPanel { Margin = new Thickness(30, 4, 0, 0), MaxWidth = 360 };
        for (int d = 1; d <= 9; d++)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Width = 115, Margin = new Thickness(0, 4, 0, 0) };
            var icon = PugDrawer.CreateIcon(d, 32);
            icon.Margin = new Thickness(0, 0, 6, 0);
            row.Children.Add(icon);
            row.Children.Add(new TextBlock
            {
                Text = $"{d} = {PugDrawer.GetName(d)}", FontSize = 11, Foreground = T.Text,
                VerticalAlignment = VerticalAlignment.Center
            });
            legendGrid.Children.Add(row);
        }
        sp.Children.Add(legendGrid);

        SetScreen(sp);
    }

    // ===================================================================
    //  UI HELPERS
    // ===================================================================
    private TextBlock SectionLabel(string text) => new()
    {
        Text = text, FontSize = 14, FontWeight = FontWeights.Bold,
        Foreground = T.Text, Margin = new Thickness(30, 18, 0, 6)
    };

    private Border MakeToggle(string text, bool active, Action onClick)
    {
        var b = new Border
        {
            Width = 120, Height = 36, CornerRadius = new CornerRadius(14),
            Background = active ? T.Accent : T.ButtonBg,
            BorderBrush = T.Border, BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 10, 0), Cursor = Cursors.Hand,
            Child = new TextBlock
            {
                Text = text, FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = active ? Brushes.White : T.ButtonText
            }
        };
        b.MouseLeftButtonDown += (_, _) => onClick();
        return b;
    }

    private StackPanel MakeSwitch(string label, bool active, Action onClick)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(30, 4, 0, 0) };
        row.Children.Add(new TextBlock { Text = label, FontSize = 13, Foreground = T.Text, Width = 160,
            VerticalAlignment = VerticalAlignment.Center });
        var track = new Border { Width = 48, Height = 24, CornerRadius = new CornerRadius(12),
            Background = active ? T.Accent : T.Border, Cursor = Cursors.Hand };
        var knob = new Ellipse { Width = 18, Height = 18, Fill = Brushes.White,
            HorizontalAlignment = active ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Margin = new Thickness(3) };
        track.Child = knob;
        track.MouseLeftButtonDown += (_, _) => onClick();
        row.Children.Add(track);
        return row;
    }

    private Canvas MakeDigitButton(int digit)
    {
        double w = 52, h = 52;
        var cv = new Canvas { Width = w, Height = h, Background = Brushes.Transparent, Cursor = Cursors.Hand,
            Margin = new Thickness(1) };
        var rect = new Rectangle { Width = w, Height = h, RadiusX = 10, RadiusY = 10,
            Fill = T.ButtonBg, Stroke = T.Border, StrokeThickness = 1 };
        cv.Children.Add(rect);
        PugDrawer.DrawWithNumber(cv, digit, w / 2, h / 2 - 2, 34, true);

        cv.MouseEnter += (_, _) => rect.Fill = T.ButtonHover;
        cv.MouseLeave += (_, _) => rect.Fill = T.ButtonBg;
        return cv;
    }

    private Canvas MakeMenuButton(string text, string sub, int icon, double w, double h)
    {
        var cv = new Canvas { Width = w, Height = h, Background = Brushes.Transparent, Cursor = Cursors.Hand };
        var rect = new Rectangle { Width = w, Height = h, RadiusX = 16, RadiusY = 16,
            Fill = T.ButtonBg, Stroke = T.Border, StrokeThickness = 1 };
        cv.Children.Add(rect);

        double tx = icon > 0 ? 50 : 16;
        double ty = string.IsNullOrEmpty(sub) ? h / 2 : h / 2 - 7;

        AddText(cv, tx, ty, text, 14, FontWeights.Bold, T.ButtonText, HorizontalAlignment.Left);
        if (!string.IsNullOrEmpty(sub))
            AddText(cv, tx, h / 2 + 9, sub, 10, FontWeights.Normal, T.TextLight, HorizontalAlignment.Left);

        if (icon > 0) PugDrawer.Draw(cv, icon, 28, h / 2, 32);

        cv.MouseEnter += (_, _) => rect.Fill = T.ButtonHover;
        cv.MouseLeave += (_, _) => rect.Fill = T.ButtonBg;
        return cv;
    }

    private Border MakeTextButton(string text, double w, double h, Action onClick)
    {
        var b = new Border
        {
            Width = w, Height = h, Cursor = Cursors.Hand,
            Child = new TextBlock { Text = text, FontSize = 20, FontWeight = FontWeights.Bold,
                Foreground = T.Text, HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center }
        };
        b.MouseLeftButtonDown += (_, _) => onClick();
        return b;
    }

    private static void AddText(Canvas cv, double x, double y, string text,
        double size, FontWeight weight, Brush fg, HorizontalAlignment align = HorizontalAlignment.Center)
    {
        var tb = new TextBlock { Text = text, FontSize = size, FontWeight = weight, Foreground = fg,
            FontFamily = new FontFamily("Segoe UI") };
        tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double left = align == HorizontalAlignment.Left ? x : x - tb.DesiredSize.Width / 2;
        Canvas.SetLeft(tb, left);
        Canvas.SetTop(tb, y - tb.DesiredSize.Height / 2);
        cv.Children.Add(tb);
    }

    private static void DrawPawPrints(Canvas cv, double w, double h)
    {
        var rng = new Random(42);
        for (int i = 0; i < 14; i++)
        {
            double x = rng.Next(20, (int)w - 20), y = rng.Next(20, (int)h - 20);
            DrawPaw(cv, x, y, rng.Next(8, 15), "#E8D5C4");
        }
    }

    private static void DrawPaw(Canvas cv, double x, double y, double sz, string color)
    {
        double s = sz / 10.0;
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)!);
        var pad = new Ellipse { Width = 8 * s, Height = 6 * s, Fill = brush };
        Canvas.SetLeft(pad, x - 4 * s); Canvas.SetTop(pad, y - 2 * s);
        cv.Children.Add(pad);
        foreach (var (dx, dy) in new[] { (-3, -4), (0, -6), (3, -4) })
        {
            var toe = new Ellipse { Width = 4 * s, Height = 4 * s, Fill = brush };
            Canvas.SetLeft(toe, x + dx * s - 2 * s); Canvas.SetTop(toe, y + dy * s - 2 * s);
            cv.Children.Add(toe);
        }
    }
}
