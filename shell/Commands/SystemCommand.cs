// ============================================================
//  ARQYON SHELL — SystemCommand.cs
//  Handles: arqyon system
//           arqyon system cpu | ram | os | disk | all
// ============================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using ArqyonShell.Interop;
using ArqyonShell.Output;

namespace ArqyonShell.Commands;

internal static class SystemCommand
{
    public static void Run(string[] args)
    {
        string sub = args.Length > 1 ? args[1].ToLower() : "all";

        switch (sub)
        {
            case "cpu":   ShowCpu();  break;
            case "ram":   ShowRam();  break;
            case "os":    ShowOs();   break;
            case "disk":  ShowDisk(); break;
            case "all":
            default:
                ShowOs();
                ShowCpu();
                ShowRam();
                ShowDisk();
                break;
        }

        Printer.Blank();
    }

    // ── CPU ──────────────────────────────────────────────────

    private static void ShowCpu()
    {
        Printer.SectionHeader("Processor", "⚙");
        try
        {
            var j = ArqyonCore.ParseJson(ArqyonCore.GetCpuInfoJson());
            Printer.Row("Name",    j.GetProperty("name").GetString()    ?? "—");
            Printer.Row("Cores",   j.GetProperty("cores").GetInt32().ToString());
            Printer.Row("Threads", j.GetProperty("threads").GetInt32().ToString());
            Printer.Row("Speed",   $"{j.GetProperty("speed_ghz").GetDouble():F2} GHz");
        }
        catch (Exception ex) { Printer.Err($"CPU read failed: {ex.Message}"); }
    }

    // ── RAM ──────────────────────────────────────────────────

    private static void ShowRam()
    {
        Printer.SectionHeader("Memory", "▣");
        try
        {
            var j = ArqyonCore.ParseJson(ArqyonCore.GetRamInfoJson());
            double pct = j.GetProperty("percent").GetDouble();
            ConsoleColor pctColor = pct > 85 ? ConsoleColor.Red
                                 : pct > 65 ? ConsoleColor.Yellow
                                 : ConsoleColor.Green;

            Printer.Row("Total",     j.GetProperty("total").GetString()     ?? "—");
            Printer.Row("Used",      j.GetProperty("used").GetString()      ?? "—");
            Printer.Row("Available", j.GetProperty("available").GetString() ?? "—");
            Printer.Row("Usage",     $"{pct:F1}%", pctColor);
        }
        catch (Exception ex) { Printer.Err($"RAM read failed: {ex.Message}"); }
    }

    // ── OS ───────────────────────────────────────────────────

    private static void ShowOs()
    {
        Printer.SectionHeader("Operating System", "⊞");
        try
        {
            var j = ArqyonCore.ParseJson(ArqyonCore.GetOsInfoJson());
            Printer.Row("Name",    j.GetProperty("name").GetString()    ?? "—");
            Printer.Row("Version", j.GetProperty("version").GetString() ?? "—");
            Printer.Row("Build",   j.GetProperty("build").GetString()   ?? "—");
            Printer.Row("Arch",    j.GetProperty("arch").GetString()    ?? "—");
        }
        catch (Exception ex) { Printer.Err($"OS read failed: {ex.Message}"); }
    }

    // ── Disk ─────────────────────────────────────────────────

    private static void ShowDisk()
    {
        Printer.SectionHeader("Storage", "◫");
        try
        {
            string raw = ArqyonCore.GetDiskInfoJson();
            var arr = JsonDocument.Parse(raw).RootElement;

            var headers = new List<string> { "Drive", "Total", "Used", "Free", "Usage" };
            var rows    = new List<List<string>>();

            foreach (var drive in arr.EnumerateArray())
            {
                double pct = drive.GetProperty("percent").GetDouble();
                rows.Add(new List<string>
                {
                    drive.GetProperty("drive").GetString()   ?? "?",
                    drive.GetProperty("total").GetString()   ?? "?",
                    drive.GetProperty("used").GetString()    ?? "?",
                    drive.GetProperty("free").GetString()    ?? "?",
                    $"{pct:F1}%"
                });
            }

            if (rows.Count == 0) Printer.Info("No fixed drives detected.");
            else Printer.Table(headers, rows, colWidth: 14);
        }
        catch (Exception ex) { Printer.Err($"Disk read failed: {ex.Message}"); }
    }
}
