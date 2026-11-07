// ============================================================
//  ARQYON SHELL — FileCommand.cs
//  Handles: arqyon file hash <path> [--md5]
//           arqyon file info <path>
// ============================================================

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using ArqyonShell.Interop;
using ArqyonShell.Output;

namespace ArqyonShell.Commands;

internal static class FileCommand
{
    public static void Run(string[] args)
    {
        if (args.Length < 2)
        {
            Printer.Warn("Usage: arqyon file <hash|info> <path> [--md5]");
            return;
        }

        string sub = args[1].ToLower();

        switch (sub)
        {
            case "hash" when args.Length >= 3:
                bool useMd5 = Array.Exists(args, a => a.Equals("--md5", StringComparison.OrdinalIgnoreCase));
                ShowHash(args[2], useMd5);
                break;

            case "info" when args.Length >= 3:
                ShowInfo(args[2]);
                break;

            case "hash":
            case "info":
                Printer.Warn($"Usage: arqyon file {sub} <path>");
                break;

            default:
                Printer.Warn($"Unknown sub-command: {sub}");
                Printer.Info("Available: hash | info");
                break;
        }

        Printer.Blank();
    }

    // ── Hash ─────────────────────────────────────────────────

    private static void ShowHash(string path, bool useMd5)
    {
        string algo = useMd5 ? "MD5" : "SHA-256";
        Printer.SectionHeader($"File Hash ({algo})", "⊛");

        if (!File.Exists(path))
        {
            Printer.Err($"File not found: {path}");
            return;
        }

        Printer.Working($"Hashing {Path.GetFileName(path)}");

        try
        {
            string hash = useMd5
                ? ArqyonCore.HashFileMD5Str(path)
                : ArqyonCore.HashFileSHA256Str(path);

            Printer.Done();
            Printer.Blank();
            Printer.Row("File",      Path.GetFullPath(path));
            Printer.Row("Algorithm", algo);
            Printer.Row("Hash",      hash, ConsoleColor.Cyan);
        }
        catch (Exception ex) { Printer.Err($"Hash failed: {ex.Message}"); }
    }

    // ── Info ─────────────────────────────────────────────────

    private static void ShowInfo(string path)
    {
        Printer.SectionHeader("File Information", "◫");

        if (!File.Exists(path) && !Directory.Exists(path))
        {
            Printer.Err($"Path not found: {path}");
            return;
        }

        try
        {
            var j = ArqyonCore.ParseJson(ArqyonCore.GetFileInfoJson(path));

            if (j.TryGetProperty("error", out var err))
            {
                Printer.Err(err.GetString() ?? "Unknown error");
                return;
            }

            Printer.Row("Path",       j.GetProperty("path").GetString()      ?? "—");
            Printer.Row("Type",       j.GetProperty("type").GetString()      ?? "—");
            Printer.Row("Extension",  j.GetProperty("extension").GetString() ?? "—");
            Printer.Row("Size",       j.GetProperty("size").GetString()      ?? "—");
            Printer.Row("Created",    j.GetProperty("created").GetString()   ?? "—");
            Printer.Row("Modified",   j.GetProperty("modified").GetString()  ?? "—");
            Printer.Row("Accessed",   j.GetProperty("accessed").GetString()  ?? "—");
            Printer.Row("Attributes", j.GetProperty("attributes").GetString()?? "—");
        }
        catch (Exception ex) { Printer.Err($"Info read failed: {ex.Message}"); }
    }
}
