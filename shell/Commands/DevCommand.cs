// ============================================================
//  ARQYON SHELL — DevCommand.cs
//  Handles: arqyon dev
//           arqyon dev langs
//           arqyon dev env
//           arqyon dev doctor
// ============================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using ArqyonShell.Interop;
using ArqyonShell.Output;

namespace ArqyonShell.Commands;

internal static class DevCommand
{
    public static void Run(string[] args)
    {
        string sub = args.Length > 1 ? args[1].ToLower() : "langs";

        switch (sub)
        {
            case "langs":    ShowLanguages(); break;
            case "env":      ShowEnvPath();   break;
            case "doctor":   RunDoctor();     break;
            default:
                Printer.Warn($"Unknown sub-command: {sub}");
                Printer.Info("Available: langs | env | doctor");
                break;
        }

        Printer.Blank();
    }

    // ── Languages ────────────────────────────────────────────

    private static void ShowLanguages()
    {
        Printer.SectionHeader("Detected Languages & Runtimes", "◈");
        Printer.Working("Scanning PATH");

        try
        {
            string raw = ArqyonCore.DetectLanguagesJson();
            var arr = JsonDocument.Parse(raw).RootElement;

            var headers = new List<string> { "Runtime", "Version", "Path" };
            var rows    = new List<List<string>>();

            foreach (var lang in arr.EnumerateArray())
            {
                rows.Add(new List<string>
                {
                    lang.GetProperty("name").GetString()    ?? "—",
                    lang.GetProperty("version").GetString() ?? "—",
                    lang.GetProperty("path").GetString()    ?? "—",
                });
            }

            Printer.Done();

            if (rows.Count == 0)
                Printer.Warn("No known runtimes found on PATH.");
            else
            {
                Printer.Table(headers, rows, colWidth: 26);
                Printer.Blank();
                Printer.Ok($"{rows.Count} runtime(s) detected.");
            }
        }
        catch (Exception ex) { Printer.Err($"Detection failed: {ex.Message}"); }
    }

    // ── ENV PATH ─────────────────────────────────────────────

    private static void ShowEnvPath()
    {
        Printer.SectionHeader("PATH Entries", "⊞");
        try
        {
            string raw = ArqyonCore.ScanEnvPathJson();
            var arr = JsonDocument.Parse(raw).RootElement;

            int i = 1;
            foreach (var entry in arr.EnumerateArray())
            {
                string path = entry.GetString() ?? "";
                Printer.Row($"[{i++}]", path);
            }
        }
        catch (Exception ex) { Printer.Err($"PATH scan failed: {ex.Message}"); }
    }

    // ── Doctor ───────────────────────────────────────────────
    // Checks for essential developer tools and reports status.

    private static readonly (string Name, string Exe)[] EssentialTools =
    {
        ("Git",         "git"),
        ("Node.js",     "node"),
        ("npm",         "npm"),
        ("Python",      "python"),
        ("C# / .NET",   "dotnet"),
        ("GCC",         "gcc"),
        ("Clang",       "clang"),
        ("Docker",      "docker"),
        ("Rust",        "rustc"),
        ("Go",          "go"),
    };

    private static void RunDoctor()
    {
        Printer.SectionHeader("Developer Environment Doctor", "✦");
        Printer.Working("Probing tools");

        try
        {
            string raw = ArqyonCore.DetectLanguagesJson();
            var arr = JsonDocument.Parse(raw).RootElement;

            // Build a quick lookup of detected exe names
            var detected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var lang in arr.EnumerateArray())
            {
                string? path = lang.GetProperty("path").GetString();
                if (!string.IsNullOrEmpty(path))
                {
                    // Extract the filename without extension
                    string exe = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                    detected.Add(exe);
                }
            }

            Printer.Done();
            Printer.Blank();

            int found = 0, missing = 0;
            foreach (var (name, exe) in EssentialTools)
            {
                if (detected.Contains(exe))
                {
                    Printer.Ok($"{name,-20} found");
                    found++;
                }
                else
                {
                    Printer.Warn($"{name,-20} not found on PATH");
                    missing++;
                }
            }

            Printer.Blank();
            Printer.Divider();

            if (missing == 0)
                Printer.Ok("All essential tools detected. Environment looks healthy.");
            else
            {
                Printer.Ok($"{found} tool(s) found.");
                Printer.Warn($"{missing} tool(s) missing — install them and re-run doctor.");
            }
        }
        catch (Exception ex) { Printer.Err($"Doctor failed: {ex.Message}"); }
    }
}
