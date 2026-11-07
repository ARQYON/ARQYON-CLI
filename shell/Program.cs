// ============================================================
//  ARQYON SHELL — Program.cs
//  Entry point. Parses top-level command and dispatches.
// ============================================================

using System;
using ArqyonShell.Commands;
using ArqyonShell.Output;

// No top-level args → show banner + help
if (args.Length == 0)
{
    Printer.Banner();
    ShowHelp();
    return 0;
}

string command = args[0].ToLower();

try
{
    switch (command)
    {
        case "--version":
        case "-v":
            ShowVersion();
            break;

        case "--help":
        case "-h":
        case "help":
            Printer.Banner();
            ShowHelp();
            break;

        case "system":
        case "sys":
            SystemCommand.Run(args);
            break;

        case "dev":
            DevCommand.Run(args);
            break;

        case "file":
            FileCommand.Run(args);
            break;

        case "diag":
        case "doctor":
            // "arqyon doctor" maps to diag doctor sub-mode
            if (command == "doctor")
            {
                var devArgs = new[] { "dev", "doctor" };
                DevCommand.Run(devArgs);
            }
            else
            {
                DiagCommand.Run(args);
            }
            break;

        case "run":
        case "plugin":
        case "plugins":
            RunCommand.Run(args);
            break;

        default:
            Printer.Blank();
            Printer.Err($"Unknown command: {command}");
            Printer.Info("Run 'arqyon --help' to see available commands.");
            Printer.Blank();
            return 1;
    }
}
catch (DllNotFoundException ex)
{
    Printer.Blank();
    Printer.ErrorBox(
        "arqyon_core.dll not found",
        $"Make sure arqyon_core.dll is in the same directory as this binary.\n  Detail: {ex.Message}"
    );
    return 2;
}
catch (Exception ex)
{
    Printer.Blank();
    Printer.ErrorBox("Unexpected Error", ex.Message);
    return 99;
}

return 0;

// ── Help ─────────────────────────────────────────────────────

static void ShowHelp()
{
    Printer.WriteLine("  USAGE", ConsoleColor.White);
    Printer.Blank();
    Printer.HelpEntry("arqyon system [cpu|ram|os|disk]", "System hardware & OS info");
    Printer.HelpEntry("arqyon dev langs",                "Detect installed runtimes");
    Printer.HelpEntry("arqyon dev env",                  "Inspect PATH entries");
    Printer.HelpEntry("arqyon dev doctor",               "Check dev environment health");
    Printer.HelpEntry("arqyon file hash <path>",         "SHA-256 file hash");
    Printer.HelpEntry("arqyon file hash <path> --md5",   "MD5 file hash");
    Printer.HelpEntry("arqyon file info <path>",         "File metadata");
    Printer.HelpEntry("arqyon diag",                     "Full diagnostic report");
    Printer.HelpEntry("arqyon diag --export",            "Save report as JSON");
    Printer.HelpEntry("arqyon diag --export --html",     "Save report as HTML");
    Printer.HelpEntry("arqyon run <script.py>",          "Run a Python script");
    Printer.HelpEntry("arqyon plugin list",              "List installed plugins");
    Printer.HelpEntry("arqyon --version",                "Show version");
    Printer.Blank();
}

static void ShowVersion()
{
    Printer.Blank();
    Printer.WriteLine("  ARQYON CLI  v0.1.0", ConsoleColor.Cyan);
    Printer.WriteLine("  C++ Core  |  C# Shell  |  Python Automation", ConsoleColor.DarkGray);
    Printer.WriteLine("  MIT License — ARQYON Organization", ConsoleColor.DarkGray);
    Printer.Blank();
}
