using Spectre.Console;

namespace ArqyonCLI.Core;

/// <summary>
/// Centralized Spectre.Console rendering helpers - keeps all styling in one place.
/// </summary>
internal static class OutputRenderer
{
    // ─── Brand Colors ─────────────────────────────────────────────────────────
    private const string Accent  = "cyan1";
    private const string Label   = "grey70";
    private const string Ok      = "green3";
    private const string Warn    = "yellow3";
    private const string Fail    = "red3";
    private const string Muted   = "grey46";

    // ─── Header ───────────────────────────────────────────────────────────────
    public static void PrintBanner()
    {
        AnsiConsole.Write(
            new FigletText("ARQYON")
                .Centered()
                .Color(Color.Cyan1));

        AnsiConsole.MarkupLine($"[{Muted}]  Developer & System Toolkit  ·  v1.0.0  ·  ARQYON[/]");
        AnsiConsole.WriteLine();
    }

    // ─── Section header ───────────────────────────────────────────────────────
    public static void Section(string title)
    {
        AnsiConsole.Write(new Rule($"[{Accent}] {title} [/]").RuleStyle(Muted).LeftJustified());
    }

    // ─── Key/Value row ────────────────────────────────────────────────────────
    public static void Row(string key, string value, string? color = null)
    {
        var c = color ?? "white";
        AnsiConsole.MarkupLine($"  [{Label}]{key,-22}[/] [{c}]{Markup.Escape(value)}[/]");
    }

    // ─── Status row (✓ / ✗ / ?) ───────────────────────────────────────────────
    public static void StatusRow(string label, bool? ok, string detail = "")
    {
        var (icon, color) = ok switch {
            true  => ("✓", Ok),
            false => ("✗", Fail),
            null  => ("?", Warn),
        };
        var extra = string.IsNullOrEmpty(detail) ? "" : $" [grey46]{Markup.Escape(detail)}[/]";
        AnsiConsole.MarkupLine($"  [{color}]{icon}[/] [{Label}]{label,-30}[/]{extra}");
    }

    // ─── Error / Info ─────────────────────────────────────────────────────────
    public static void Error(string msg)   => AnsiConsole.MarkupLine($"[{Fail}]  error: {Markup.Escape(msg)}[/]");
    public static void Warning(string msg) => AnsiConsole.MarkupLine($"[{Warn}]  warn: {Markup.Escape(msg)}[/]");
    public static void Info(string msg)    => AnsiConsole.MarkupLine($"[{Muted}]  {Markup.Escape(msg)}[/]");

    // ─── Progress spinner ─────────────────────────────────────────────────────
    public static T Spin<T>(string label, Func<T> work)
    {
        T result = default!;
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse(Accent))
            .Start($"[{Label}]{label}[/]", _ => { result = work(); });
        return result;
    }

    public static void Blank() => AnsiConsole.WriteLine();
}
