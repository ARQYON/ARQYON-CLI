// ============================================================
//  ARQYON SHELL — Printer.cs
//  All console output: banners, tables, labels, colors.
//  No command logic lives here — pure presentation only.
// ============================================================

using System;
using System.Collections.Generic;

namespace ArqyonShell.Output;

internal static class Printer
{
    // ── Theme ─────────────────────────────────────────────────
    private static readonly ConsoleColor Accent   = ConsoleColor.Cyan;
    private static readonly ConsoleColor Dim      = ConsoleColor.DarkGray;
    private static readonly ConsoleColor Success  = ConsoleColor.Green;
    private static readonly ConsoleColor Warning  = ConsoleColor.Yellow;
    private static readonly ConsoleColor Error    = ConsoleColor.Red;
    private static readonly ConsoleColor Label    = ConsoleColor.White;
    private static readonly ConsoleColor Value    = ConsoleColor.Gray;

    // ── Core Write Helpers ────────────────────────────────────

    public static void Write(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    public static void WriteLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void Blank() => Console.WriteLine();

    // ── Banner ────────────────────────────────────────────────

    public static void Banner()
    {
        Blank();
        WriteLine("  █████╗ ██████╗  ██████╗ ██╗   ██╗ ██████╗ ███╗   ██╗", Accent);
        WriteLine(" ██╔══██╗██╔══██╗██╔═══██╗╚██╗ ██╔╝██╔═══██╗████╗  ██║", Accent);
        WriteLine(" ███████║██████╔╝██║   ██║ ╚████╔╝ ██║   ██║██╔██╗ ██║", Accent);
        WriteLine(" ██╔══██║██╔══██╗██║▄▄ ██║  ╚██╔╝  ██║   ██║██║╚██╗██║", Accent);
        WriteLine(" ██║  ██║██║  ██║╚██████╔╝   ██║   ╚██████╔╝██║ ╚████║", Accent);
        WriteLine(" ╚═╝  ╚═╝╚═╝  ╚═╝ ╚══▀▀═╝   ╚═╝    ╚═════╝ ╚═╝  ╚═══╝", Accent);
        Blank();
        WriteLine("  Think Beyond. Build Without Limits.", Dim);
        WriteLine("  v0.1.0  •  C++ Core + C# Shell + Python Automation", Dim);
        Blank();
    }

    // ── Section Header ────────────────────────────────────────

    public static void SectionHeader(string title, string icon = "")
    {
        Blank();
        string line = new string('─', 50);
        WriteLine($"  {icon}  {title.ToUpper()}", Accent);
        WriteLine($"  {line}", Dim);
    }

    // ── Key-Value Row ─────────────────────────────────────────

    public static void Row(string key, string value, ConsoleColor valueColor = default)
    {
        if (valueColor == default) valueColor = Value;
        string paddedKey = key.PadRight(20);
        Write($"  {paddedKey}", Label);
        WriteLine(value, valueColor);
    }

    // ── Table ─────────────────────────────────────────────────

    public static void Table(List<string> headers, List<List<string>> rows,
                             int colWidth = 24)
    {
        Blank();

        // Header row
        Write("  ", Dim);
        foreach (var h in headers)
            Write(h.PadRight(colWidth), Accent);
        Blank();

        // Divider
        Write("  ", Dim);
        foreach (var _ in headers)
            Write(new string('─', colWidth - 1) + " ", Dim);
        Blank();

        // Data rows
        foreach (var row in rows)
        {
            Write("  ", Dim);
            for (int i = 0; i < row.Count; i++)
            {
                string cell = row[i];
                if (cell.Length > colWidth - 2)
                    cell = cell[..(colWidth - 5)] + "...";
                Write(cell.PadRight(colWidth), i == 0 ? Label : Value);
            }
            Blank();
        }
    }

    // ── Status Indicators ─────────────────────────────────────

    public static void Ok(string message)
        => WriteLine($"  ✔  {message}", Success);

    public static void Warn(string message)
        => WriteLine($"  ⚠  {message}", Warning);

    public static void Err(string message)
        => WriteLine($"  ✘  {message}", Error);

    public static void Info(string message)
        => WriteLine($"  ·  {message}", Dim);

    // ── Progress / Spinner (simple inline) ────────────────────

    public static void Working(string message)
    {
        Write($"  ◌  {message}...", Dim);
    }

    public static void Done()
    {
        Console.CursorLeft = 0;
        Write("  ✔  ", Success);
        Blank();
    }

    // ── Error Box ─────────────────────────────────────────────

    public static void ErrorBox(string title, string detail)
    {
        Blank();
        WriteLine($"  ╔══ {title} ══", Error);
        WriteLine($"  ║  {detail}", Error);
        WriteLine($"  ╚{'═', 1}", Error);
        Blank();
    }

    // ── Help Entry ────────────────────────────────────────────

    public static void HelpEntry(string command, string description)
    {
        Write($"  {command,-30}", Accent);
        WriteLine(description, Dim);
    }

    // ── Divider ───────────────────────────────────────────────

    public static void Divider()
        => WriteLine("  " + new string('─', 50), Dim);
}
