using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PugSudoku.Drawing;

public static class PugDrawer
{
    private static readonly string[][] Palettes =
    [
        ["#FFBF69", "#E8963A", "#F0A84A", "#B86B1A"],  // 1 orange
        ["#7B5B3A", "#5C3D1E", "#6B4B2A", "#3D2510"],  // 2 chocolate
        ["#A0D2DB", "#6EB5C0", "#8BC4D0", "#4A8A96"],  // 3 sky blue
        ["#C3A0D8", "#9B6FBF", "#B090C8", "#7A4FA0"],  // 4 lavender
        ["#F7A8B8", "#E07090", "#F090A0", "#C05070"],  // 5 pink
        ["#A8D8A0", "#70B868", "#90C888", "#508848"],  // 6 green
        ["#FFD180", "#E8A830", "#F0C050", "#C08010"],  // 7 gold
        ["#B0B0C8", "#8888A8", "#9898B8", "#686890"],  // 8 slate
        ["#E8C8A0", "#C8A070", "#D8B888", "#A88050"],  // 9 sand
    ];

    private static readonly string[] FaceNames =
        ["Wesoly", "Cool", "Spiacy", "Mrugajacy", "Zdziwiony",
         "Usmiech", "Kapelusz", "Wasy", "Kokarda"];

    public static string GetName(int digit) =>
        digit >= 1 && digit <= 9 ? FaceNames[digit - 1] : "?";

    public static void Draw(Canvas c, int digit, double cx, double cy, double sz)
    {
        if (digit < 1 || digit > 9) return;
        var p = Palettes[digit - 1];
        switch (digit)
        {
            case 1: DrawHappy(c, cx, cy, sz, p); break;
            case 2: DrawCool(c, cx, cy, sz, p); break;
            case 3: DrawSleepy(c, cx, cy, sz, p); break;
            case 4: DrawWink(c, cx, cy, sz, p); break;
            case 5: DrawSurprised(c, cx, cy, sz, p); break;
            case 6: DrawSmile(c, cx, cy, sz, p); break;
            case 7: DrawHat(c, cx, cy, sz, p); break;
            case 8: DrawMustache(c, cx, cy, sz, p); break;
            case 9: DrawBow(c, cx, cy, sz, p); break;
        }
    }

    public static void DrawKey(Canvas c, string key, double cx, double cy, double sz)
    {
        if (key == "pug_happy") DrawHappy(c, cx, cy, sz, Palettes[0]);
        else if (key == "pug_cool") DrawCool(c, cx, cy, sz, Palettes[1]);
        else if (key == "pug_sleepy") DrawSleepy(c, cx, cy, sz, Palettes[2]);
    }

    public static Canvas CreateIcon(int digit, double size)
    {
        var canvas = new Canvas { Width = size, Height = size };
        Draw(canvas, digit, size / 2, size / 2, size);
        return canvas;
    }

    // digit number overlay on top of the pug
    public static void DrawWithNumber(Canvas c, int digit, double cx, double cy, double sz, bool showDigit)
    {
        Draw(c, digit, cx, cy, sz);
        if (showDigit)
        {
            double r = sz * 0.22;
            var circle = new Ellipse
            {
                Width = r * 2, Height = r * 2,
                Fill = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1.2
            };
            Canvas.SetLeft(circle, cx - r);
            Canvas.SetTop(circle, cy - r);
            c.Children.Add(circle);

            var tb = new TextBlock
            {
                Text = digit.ToString(),
                FontSize = sz * 0.36,
                FontWeight = FontWeights.ExtraBold,
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new FontFamily("Segoe UI")
            };
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(tb, cx - tb.DesiredSize.Width / 2);
            Canvas.SetTop(tb, cy - tb.DesiredSize.Height / 2);
            c.Children.Add(tb);
        }
    }

    // -------  1: HAPPY (orange)  -------
    private static void DrawHappy(Canvas c, double cx, double cy, double sz, string[] p)
    {
        double S(double v) => v * sz / 100.0;
        AddEllipse(c, cx - S(42), cy - S(38), S(84), S(76), p[0], p[3], S(2));
        foreach (var sign in new[] { -1, 1 })
            AddEllipse(c, cx + sign * S(34) - S(14), cy - S(42), S(28), S(20), p[1], p[3], S(1.5));
        AddEllipse(c, cx - S(22), cy - S(6), S(44), S(34), p[2], null, 0);
        foreach (var sign in new[] { -1, 1 })
        {
            double ex = cx + sign * S(15), ey = cy - S(10);
            AddEllipse(c, ex - S(8), ey - S(8), S(16), S(16), "White", p[3], S(1));
            AddEllipse(c, ex - S(4), ey - S(4), S(8), S(8), "#1A0A00", null, 0);
            AddEllipse(c, ex - S(6), ey - S(6), S(4), S(4), "White", null, 0);
        }
        AddEllipse(c, cx - S(6), cy + S(2), S(12), S(8), "#1A0A00", null, 0);
        AddEllipse(c, cx - S(5), cy + S(14), S(10), S(10), "#FF6B6B", "#E04545", S(1));
    }

    // -------  2: COOL (sunglasses)  -------
    private static void DrawCool(Canvas c, double cx, double cy, double sz, string[] p)
    {
        double S(double v) => v * sz / 100.0;
        AddEllipse(c, cx - S(42), cy - S(38), S(84), S(76), p[0], p[3], S(2));
        foreach (var sign in new[] { -1, 1 })
            AddEllipse(c, cx + sign * S(34) - S(14), cy - S(42), S(28), S(20), p[1], p[3], S(1.5));
        AddEllipse(c, cx - S(22), cy - S(6), S(44), S(34), p[2], null, 0);
        AddLine(c, cx - S(10), cy - S(10), cx + S(10), cy - S(10), "#0A0A0A", S(2.5));
        foreach (var sign in new[] { -1, 1 })
        {
            double ex = cx + sign * S(15), ey = cy - S(10);
            AddEllipse(c, ex - S(10), ey - S(8), S(20), S(16), "#0A0A0A", "#222", S(1.5));
            AddEllipse(c, ex - S(6), ey - S(5), S(5), S(4), "#333", null, 0);
            AddLine(c, cx + sign * S(25), cy - S(10), cx + sign * S(40), cy - S(16), "#0A0A0A", S(2));
        }
        AddEllipse(c, cx - S(6), cy + S(2), S(12), S(8), "#1A0A00", null, 0);
    }

    // -------  3: SLEEPY (closed eyes, blush)  -------
    private static void DrawSleepy(Canvas c, double cx, double cy, double sz, string[] p)
    {
        double S(double v) => v * sz / 100.0;
        AddEllipse(c, cx - S(42), cy - S(38), S(84), S(76), p[0], p[3], S(2));
        foreach (var sign in new[] { -1, 1 })
            AddEllipse(c, cx + sign * S(34) - S(14), cy - S(42), S(28), S(20), p[1], p[3], S(1.5));
        AddEllipse(c, cx - S(22), cy - S(6), S(44), S(34), p[2], null, 0);
        foreach (var sign in new[] { -1, 1 })
        {
            double ex = cx + sign * S(15), ey = cy - S(10);
            var path = MakeArc(ex, ey, S(8), S(6), S(2), p[3]);
            c.Children.Add(path);
        }
        foreach (var sign in new[] { -1, 1 })
            AddEllipse(c, cx + sign * S(26) - S(6), cy, S(12), S(8), "#CC7788", null, 0);
        AddEllipse(c, cx - S(6), cy + S(2), S(12), S(8), "#1A0A00", null, 0);
        AddLine(c, cx - S(6), cy + S(14), cx + S(6), cy + S(14), p[3], S(1.5));
    }

    // -------  4: WINK (one eye closed)  -------
    private static void DrawWink(Canvas c, double cx, double cy, double sz, string[] p)
    {
        double S(double v) => v * sz / 100.0;
        AddEllipse(c, cx - S(42), cy - S(38), S(84), S(76), p[0], p[3], S(2));
        foreach (var sign in new[] { -1, 1 })
            AddEllipse(c, cx + sign * S(34) - S(14), cy - S(42), S(28), S(20), p[1], p[3], S(1.5));
        AddEllipse(c, cx - S(22), cy - S(6), S(44), S(34), p[2], null, 0);
        double lex = cx - S(15), ley = cy - S(10);
        AddEllipse(c, lex - S(8), ley - S(8), S(16), S(16), "White", p[3], S(1));
        AddEllipse(c, lex - S(4), ley - S(4), S(8), S(8), "#1A0A00", null, 0);
        double rex = cx + S(15), rey = cy - S(10);
        c.Children.Add(MakeArc(rex, rey, S(8), S(6), S(2), "#1A0A00"));
        AddEllipse(c, cx - S(6), cy + S(2), S(12), S(8), "#1A0A00", null, 0);
        AddEllipse(c, cx - S(5), cy + S(14), S(10), S(10), "#FF6B6B", "#E04545", S(1));
    }

    // -------  5: SURPRISED (big O mouth)  -------
    private static void DrawSurprised(Canvas c, double cx, double cy, double sz, string[] p)
    {
        double S(double v) => v * sz / 100.0;
        AddEllipse(c, cx - S(42), cy - S(38), S(84), S(76), p[0], p[3], S(2));
        foreach (var sign in new[] { -1, 1 })
            AddEllipse(c, cx + sign * S(34) - S(14), cy - S(42), S(28), S(20), p[1], p[3], S(1.5));
        AddEllipse(c, cx - S(22), cy - S(6), S(44), S(34), p[2], null, 0);
        foreach (var sign in new[] { -1, 1 })
        {
            double ex = cx + sign * S(15), ey = cy - S(12);
            AddEllipse(c, ex - S(9), ey - S(9), S(18), S(18), "White", p[3], S(1));
            AddEllipse(c, ex - S(5), ey - S(5), S(10), S(10), "#1A0A00", null, 0);
        }
        AddEllipse(c, cx - S(7), cy + S(6), S(14), S(14), "#1A0A00", p[3], S(1));
        AddEllipse(c, cx - S(4), cy + S(9), S(8), S(8), "#FF6B6B", null, 0);
    }

    // -------  6: SMILE (curved mouth)  -------
    private static void DrawSmile(Canvas c, double cx, double cy, double sz, string[] p)
    {
        double S(double v) => v * sz / 100.0;
        AddEllipse(c, cx - S(42), cy - S(38), S(84), S(76), p[0], p[3], S(2));
        foreach (var sign in new[] { -1, 1 })
            AddEllipse(c, cx + sign * S(34) - S(14), cy - S(42), S(28), S(20), p[1], p[3], S(1.5));
        AddEllipse(c, cx - S(22), cy - S(6), S(44), S(34), p[2], null, 0);
        foreach (var sign in new[] { -1, 1 })
        {
            double ex = cx + sign * S(15), ey = cy - S(10);
            AddEllipse(c, ex - S(8), ey - S(8), S(16), S(16), "White", p[3], S(1));
            AddEllipse(c, ex - S(4), ey - S(4), S(8), S(8), "#1A0A00", null, 0);
        }
        AddEllipse(c, cx - S(6), cy + S(2), S(12), S(8), "#1A0A00", null, 0);
        var smile = new Path
        {
            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A0A00")!),
            StrokeThickness = Math.Max(1, S(2)),
            Data = new PathGeometry([
                new PathFigure(new Point(cx - S(12), cy + S(12)), [
                    new ArcSegment(new Point(cx + S(12), cy + S(12)),
                        new Size(S(12), S(8)), 0, false, SweepDirection.Clockwise, true)
                ], false)
            ])
        };
        c.Children.Add(smile);
    }

    // -------  7: HAT (top hat)  -------
    private static void DrawHat(Canvas c, double cx, double cy, double sz, string[] p)
    {
        double S(double v) => v * sz / 100.0;
        AddEllipse(c, cx - S(42), cy - S(28), S(84), S(76), p[0], p[3], S(2));
        foreach (var sign in new[] { -1, 1 })
            AddEllipse(c, cx + sign * S(34) - S(14), cy - S(32), S(28), S(20), p[1], p[3], S(1.5));
        AddEllipse(c, cx - S(22), cy + S(4), S(44), S(34), p[2], null, 0);
        // hat
        var brim = new Rectangle { Width = S(60), Height = S(6), RadiusX = S(3), RadiusY = S(3),
            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C2C2C")!) };
        Canvas.SetLeft(brim, cx - S(30)); Canvas.SetTop(brim, cy - S(42));
        c.Children.Add(brim);
        var crown = new Rectangle { Width = S(36), Height = S(28), RadiusX = S(3), RadiusY = S(3),
            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C2C2C")!) };
        Canvas.SetLeft(crown, cx - S(18)); Canvas.SetTop(crown, cy - S(70));
        c.Children.Add(crown);
        var band = new Rectangle { Width = S(36), Height = S(5),
            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(p[0])!) };
        Canvas.SetLeft(band, cx - S(18)); Canvas.SetTop(band, cy - S(48));
        c.Children.Add(band);
        foreach (var sign in new[] { -1, 1 })
        {
            double ex = cx + sign * S(15), ey = cy;
            AddEllipse(c, ex - S(8), ey - S(8), S(16), S(16), "White", p[3], S(1));
            AddEllipse(c, ex - S(4), ey - S(4), S(8), S(8), "#1A0A00", null, 0);
        }
        AddEllipse(c, cx - S(6), cy + S(12), S(12), S(8), "#1A0A00", null, 0);
    }

    // -------  8: MUSTACHE  -------
    private static void DrawMustache(Canvas c, double cx, double cy, double sz, string[] p)
    {
        double S(double v) => v * sz / 100.0;
        AddEllipse(c, cx - S(42), cy - S(38), S(84), S(76), p[0], p[3], S(2));
        foreach (var sign in new[] { -1, 1 })
            AddEllipse(c, cx + sign * S(34) - S(14), cy - S(42), S(28), S(20), p[1], p[3], S(1.5));
        AddEllipse(c, cx - S(22), cy - S(6), S(44), S(34), p[2], null, 0);
        foreach (var sign in new[] { -1, 1 })
        {
            double ex = cx + sign * S(15), ey = cy - S(10);
            AddEllipse(c, ex - S(8), ey - S(8), S(16), S(16), "White", p[3], S(1));
            AddEllipse(c, ex - S(4), ey - S(4), S(8), S(8), "#1A0A00", null, 0);
        }
        AddEllipse(c, cx - S(6), cy + S(2), S(12), S(8), "#1A0A00", null, 0);
        // mustache
        foreach (var sign in new[] { -1, 1 })
        {
            var stache = new Path
            {
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A2A1A")!),
                Data = new PathGeometry([
                    new PathFigure(new Point(cx, cy + S(10)), [
                        new BezierSegment(
                            new Point(cx + sign * S(8), cy + S(4)),
                            new Point(cx + sign * S(22), cy + S(6)),
                            new Point(cx + sign * S(20), cy + S(14)), true),
                        new BezierSegment(
                            new Point(cx + sign * S(18), cy + S(18)),
                            new Point(cx + sign * S(6), cy + S(16)),
                            new Point(cx, cy + S(10)), true)
                    ], true)
                ])
            };
            c.Children.Add(stache);
        }
    }

    // -------  9: BOW (bow tie)  -------
    private static void DrawBow(Canvas c, double cx, double cy, double sz, string[] p)
    {
        double S(double v) => v * sz / 100.0;
        AddEllipse(c, cx - S(42), cy - S(38), S(84), S(76), p[0], p[3], S(2));
        foreach (var sign in new[] { -1, 1 })
            AddEllipse(c, cx + sign * S(34) - S(14), cy - S(42), S(28), S(20), p[1], p[3], S(1.5));
        AddEllipse(c, cx - S(22), cy - S(6), S(44), S(34), p[2], null, 0);
        foreach (var sign in new[] { -1, 1 })
        {
            double ex = cx + sign * S(15), ey = cy - S(10);
            AddEllipse(c, ex - S(8), ey - S(8), S(16), S(16), "White", p[3], S(1));
            AddEllipse(c, ex - S(4), ey - S(4), S(8), S(8), "#1A0A00", null, 0);
            AddEllipse(c, ex - S(6), ey - S(6), S(4), S(4), "White", null, 0);
        }
        AddEllipse(c, cx - S(6), cy + S(2), S(12), S(8), "#1A0A00", null, 0);
        // bow tie
        foreach (var sign in new[] { -1, 1 })
        {
            var wing = new Path
            {
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E04040")!),
                Data = new PathGeometry([
                    new PathFigure(new Point(cx, cy + S(28)), [
                        new LineSegment(new Point(cx + sign * S(18), cy + S(20)), true),
                        new LineSegment(new Point(cx + sign * S(18), cy + S(36)), true),
                    ], true)
                ])
            };
            c.Children.Add(wing);
        }
        AddEllipse(c, cx - S(4), cy + S(24), S(8), S(8), "#C03030", null, 0);
    }

    // -------  Helpers  -------
    private static Path MakeArc(double cx, double cy, double rx, double ry, double sw, string color)
    {
        return new Path
        {
            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)!),
            StrokeThickness = Math.Max(1, sw),
            Data = new PathGeometry([
                new PathFigure(new Point(cx - rx, cy + ry * 0.3), [
                    new ArcSegment(new Point(cx + rx, cy + ry * 0.3),
                        new Size(rx, ry), 0, false, SweepDirection.Counterclockwise, true)
                ], false)
            ])
        };
    }

    private static void AddEllipse(Canvas c, double x, double y, double w, double h,
        string fill, string? stroke, double strokeW)
    {
        var e = new Ellipse
        {
            Width = Math.Max(1, w), Height = Math.Max(1, h),
            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fill)!)
        };
        if (stroke != null)
        {
            e.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(stroke)!);
            e.StrokeThickness = Math.Max(0.5, strokeW);
        }
        Canvas.SetLeft(e, x); Canvas.SetTop(e, y);
        c.Children.Add(e);
    }

    private static void AddLine(Canvas c, double x1, double y1, double x2, double y2,
        string color, double thickness)
    {
        c.Children.Add(new Line
        {
            X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)!),
            StrokeThickness = Math.Max(0.5, thickness)
        });
    }
}
