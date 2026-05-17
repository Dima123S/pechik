using System.Windows.Media;

namespace PugSudoku.Drawing;

public record ThemeColors(
    Brush Bg, Brush CardBg, Brush CellBg, Brush CellHover,
    Brush Accent, Brush AccentDark, Brush Text, Brush TextLight,
    Brush Border, Brush WinHighlight, Brush ButtonBg, Brush ButtonHover,
    Brush ButtonText, Brush Pink, Brush ErrorBg, Brush FixedCellBg,
    Brush SelectedCellBg, Brush SameGroupBg
);

public static class Themes
{
    static Brush B(string hex) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

    public static ThemeColors Light { get; } = new(
        Bg: B("#FFF5EE"), CardBg: B("#FFFFFF"), CellBg: B("#FFF8F2"), CellHover: B("#FFE4D6"),
        Accent: B("#E8967A"), AccentDark: B("#D4715A"), Text: B("#5D4037"), TextLight: B("#8B7355"),
        Border: B("#E8D5C4"), WinHighlight: B("#FFD700"), ButtonBg: B("#FFF0E6"), ButtonHover: B("#FFE0CC"),
        ButtonText: B("#5D4037"), Pink: B("#FFB6C1"), ErrorBg: B("#FFCCCC"),
        FixedCellBg: B("#F0E6D8"), SelectedCellBg: B("#FFE0B2"), SameGroupBg: B("#FFF0E0")
    );

    public static ThemeColors Dark { get; } = new(
        Bg: B("#2C2016"), CardBg: B("#3D2E20"), CellBg: B("#4A3828"), CellHover: B("#5A4838"),
        Accent: B("#E8967A"), AccentDark: B("#D4715A"), Text: B("#F5DEB3"), TextLight: B("#C4A47C"),
        Border: B("#5A4838"), WinHighlight: B("#FFD700"), ButtonBg: B("#4A3828"), ButtonHover: B("#5A4838"),
        ButtonText: B("#F5DEB3"), Pink: B("#CC8090"), ErrorBg: B("#6B3030"),
        FixedCellBg: B("#3D3020"), SelectedCellBg: B("#5A4020"), SameGroupBg: B("#4A3525")
    );

    public static ThemeColors Get(string name) => name == "dark" ? Dark : Light;
}
