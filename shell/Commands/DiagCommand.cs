// ============================================================
//  ARQYON SHELL — DiagCommand.cs
//  Handles: arqyon diag
//           arqyon diag --export [--html] [--out <path>]
// ============================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using ArqyonShell.Interop;
using ArqyonShell.Output;

namespace ArqyonShell.Commands;

internal static class DiagCommand
{
    public static void Run(string[] args)
    {
        bool exportMode = Array.Exists(args, a => a.Equals("--export", StringComparison.OrdinalIgnoreCase));
        bool htmlMode   = Array.Exists(args, a => a.Equals("--html",   StringComparison.OrdinalIgnoreCase));

        // Parse optional --out <path>
        string? outPath = null;
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i].Equals("--out", StringComparison.OrdinalIgnoreCase))
            { outPath = args[i + 1]; break; }

        Printer.SectionHeader("Full System Diagnostic", "✦");
        Printer.Working("Running diagnostics");

        string json;
        try
        {
            json = ArqyonCore.RunDiagnosticsJson();
        }
        catch (Exception ex)
        {
            Printer.Err($"Diagnostic failed: {ex.Message}");
            return;
        }

        Printer.Done();

        if (exportMode)
        {
            ExportReport(json, htmlMode, outPath);
            return;
        }

        // ── Inline Display ────────────────────────────────────
        try
        {
            var j = JsonDocument.Parse(json).RootElement;

            // Header meta
            Printer.Blank();
            Printer.Row("Timestamp",   j.GetProperty("timestamp").GetString()  ?? "—");
            Printer.Row("Uptime",      FormatUptime(j.GetProperty("uptime_sec").GetString() ?? "0"));
            Printer.Row("Network",     j.GetProperty("network").GetString()     ?? "—",
                j.GetProperty("network").GetString() == "reachable"
                    ? ConsoleColor.Green : ConsoleColor.Yellow);

            // OS
            Printer.SectionHeader("Operating System", "⊞");
            var os = j.GetProperty("os");
            Printer.Row("Name",    os.GetProperty("name").GetString()    ?? "—");
            Printer.Row("Version", os.GetProperty("version").GetString() ?? "—");
            Printer.Row("Build",   os.GetProperty("build").GetString()   ?? "—");
            Printer.Row("Arch",    os.GetProperty("arch").GetString()    ?? "—");

            // CPU
            Printer.SectionHeader("Processor", "⚙");
            var cpu = j.GetProperty("cpu");
            Printer.Row("Name",    cpu.GetProperty("name").GetString()          ?? "—");
            Printer.Row("Cores",   cpu.GetProperty("cores").GetInt32().ToString());
            Printer.Row("Threads", cpu.GetProperty("threads").GetInt32().ToString());
            Printer.Row("Speed",   $"{cpu.GetProperty("speed_ghz").GetDouble():F2} GHz");

            // RAM
            Printer.SectionHeader("Memory", "▣");
            var ram = j.GetProperty("ram");
            double ramPct = ram.GetProperty("percent").GetDouble();
            Printer.Row("Total",     ram.GetProperty("total").GetString()     ?? "—");
            Printer.Row("Used",      ram.GetProperty("used").GetString()      ?? "—");
            Printer.Row("Available", ram.GetProperty("available").GetString() ?? "—");
            Printer.Row("Usage",     $"{ramPct:F1}%",
                ramPct > 85 ? ConsoleColor.Red : ramPct > 65 ? ConsoleColor.Yellow : ConsoleColor.Green);

            // Disks
            Printer.SectionHeader("Storage", "◫");
            var disks = j.GetProperty("disks");
            var diskHeaders = new List<string> { "Drive", "Total", "Used", "Free", "%" };
            var diskRows    = new List<List<string>>();
            foreach (var d in disks.EnumerateArray())
            {
                diskRows.Add(new List<string>
                {
                    d.GetProperty("drive").GetString() ?? "?",
                    d.GetProperty("total").GetString() ?? "?",
                    d.GetProperty("used").GetString()  ?? "?",
                    d.GetProperty("free").GetString()  ?? "?",
                    $"{d.GetProperty("percent").GetDouble():F1}%",
                });
            }
            if (diskRows.Count > 0) Printer.Table(diskHeaders, diskRows, colWidth: 14);

            // Languages
            Printer.SectionHeader("Detected Runtimes", "◈");
            var langs = j.GetProperty("languages");
            var langHeaders = new List<string> { "Runtime", "Version" };
            var langRows    = new List<List<string>>();
            foreach (var l in langs.EnumerateArray())
            {
                langRows.Add(new List<string>
                {
                    l.GetProperty("name").GetString()    ?? "—",
                    l.GetProperty("version").GetString() ?? "—",
                });
            }
            if (langRows.Count > 0) Printer.Table(langHeaders, langRows, colWidth: 28);
            else Printer.Info("No runtimes detected.");

            Printer.Blank();
            Printer.Ok("Diagnostic complete. Use --export to save as JSON or --export --html for a report.");
        }
        catch (Exception ex)
        {
            Printer.Err($"Display error: {ex.Message}");
        }
    }

    // ── Export ───────────────────────────────────────────────

    private static void ExportReport(string json, bool html, string? path)
    {
        try
        {
            string saved = html
                ? JsonExporter.ExportHtml(json, path)
                : JsonExporter.ExportJson(json, path);

            Printer.Blank();
            Printer.Ok($"Report saved → {saved}");
        }
        catch (Exception ex)
        {
            Printer.Err($"Export failed: {ex.Message}");
        }
    }

    // ── Helpers ──────────────────────────────────────────────

    private static string FormatUptime(string seconds)
    {
        if (!long.TryParse(seconds, out long s)) return "—";
        long h = s / 3600, m = (s % 3600) / 60, sec = s % 60;
        return $"{h}h {m}m {sec}s";
    }
}
