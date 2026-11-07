// ============================================================
//  ARQYON SHELL — RunCommand.cs
//  Handles: arqyon run <script.py> [args...]
//           arqyon plugin list
// ============================================================

using System;
using System.IO;
using ArqyonShell.Interop;
using ArqyonShell.Output;

namespace ArqyonShell.Commands;

internal static class RunCommand
{
    private static readonly string PluginsDir =
        Path.Combine(AppContext.BaseDirectory, "..", "automation", "plugins");

    public static void Run(string[] args)
    {
        if (args.Length < 2)
        {
            Printer.Warn("Usage: arqyon run <script.py> [args...]");
            Printer.Info("       arqyon plugin list");
            return;
        }

        string sub = args[1].ToLower();

        // arqyon plugin list
        if (sub == "plugin" || sub == "plugins")
        {
            if (args.Length > 2 && args[2].ToLower() == "list")
                ListPlugins();
            else
                Printer.Info("Usage: arqyon plugin list");
            return;
        }

        // arqyon run <script>
        RunScript(args);
    }

    // ── Run Script ───────────────────────────────────────────

    private static void RunScript(string[] args)
    {
        string scriptPath = args[1];

        // Resolve relative paths
        if (!Path.IsPathRooted(scriptPath))
            scriptPath = Path.GetFullPath(scriptPath);

        if (!File.Exists(scriptPath))
        {
            Printer.Err($"Script not found: {scriptPath}");
            return;
        }

        if (!scriptPath.EndsWith(".py", StringComparison.OrdinalIgnoreCase))
        {
            Printer.Warn("Only Python (.py) scripts are supported.");
            return;
        }

        // Collect any extra args after the script path
        string extraArgs = args.Length > 2
            ? string.Join(" ", args[2..])
            : string.Empty;

        Printer.SectionHeader($"Running: {Path.GetFileName(scriptPath)}", "▶");
        Printer.Row("Path",    scriptPath);
        Printer.Row("Args",    extraArgs.Length > 0 ? extraArgs : "(none)");
        Printer.Row("Runtime", ArqyonCore.GetPythonVersionStr());
        Printer.Blank();
        Printer.Divider();
        Printer.Blank();

        try
        {
            int exitCode = ArqyonCore.RunScript(scriptPath, extraArgs);
            Printer.Blank();
            Printer.Divider();

            if (exitCode == 0)
                Printer.Ok($"Script exited with code 0.");
            else
                Printer.Err($"Script exited with code {exitCode}.");
        }
        catch (Exception ex)
        {
            Printer.Err($"Runner failed: {ex.Message}");
        }

        Printer.Blank();
    }

    // ── Plugin List ──────────────────────────────────────────

    private static void ListPlugins()
    {
        Printer.SectionHeader("Installed Plugins", "⬡");

        if (!Directory.Exists(PluginsDir))
        {
            Printer.Warn($"Plugins directory not found: {PluginsDir}");
            Printer.Info("Create the directory and add .py files to it.");
            return;
        }

        var plugins = Directory.GetFiles(PluginsDir, "*.py");

        if (plugins.Length == 0)
        {
            Printer.Info("No plugins installed.");
            Printer.Info($"Drop .py files into: {PluginsDir}");
            return;
        }

        foreach (var p in plugins)
        {
            string name = Path.GetFileNameWithoutExtension(p);
            Printer.Row(name, p);
        }

        Printer.Blank();
        Printer.Ok($"{plugins.Length} plugin(s) found.");
        Printer.Blank();
    }
}
