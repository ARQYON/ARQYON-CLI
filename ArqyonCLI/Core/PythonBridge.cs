using System.Diagnostics;
using System.Text;

namespace ArqyonCLI.Core;

/// <summary>
/// Runs Python automation scripts as subprocesses.
/// Plugin scripts live in the /plugins directory next to arqyon.exe.
/// </summary>
internal static class PythonBridge
{
    private static readonly string PluginsDir =
        Path.Combine(AppContext.BaseDirectory, "plugins");

    // ─── Run a named plugin ───────────────────────────────────────────────────
    public static (int ExitCode, string Output, string Error) RunPlugin(
        string pluginName,
        string[] args)
    {
        string scriptPath = Path.Combine(PluginsDir, pluginName.EndsWith(".py")
            ? pluginName
            : pluginName + ".py");

        if (!File.Exists(scriptPath))
            return (-1, "", $"Plugin not found: {scriptPath}");

        return RunScript(scriptPath, args);
    }

    // ─── Run any script path ──────────────────────────────────────────────────
    public static (int ExitCode, string Output, string Error) RunScript(
        string scriptPath,
        string[] args)
    {
        var argString = string.Join(" ", args.Select(a => $"\"{a}\""));
        var psi = new ProcessStartInfo
        {
            FileName               = "python",
            Arguments              = $"\"{scriptPath}\" {argString}",
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding  = Encoding.UTF8,
        };

        using var proc = Process.Start(psi);
        if (proc is null) return (-1, "", "Failed to start python process");

        string stdout = proc.StandardOutput.ReadToEnd();
        string stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        return (proc.ExitCode, stdout, stderr);
    }

    // ─── List available plugins ───────────────────────────────────────────────
    public static IEnumerable<string> ListPlugins()
    {
        if (!Directory.Exists(PluginsDir)) yield break;
        foreach (var f in Directory.EnumerateFiles(PluginsDir, "*.py", SearchOption.TopDirectoryOnly))
            yield return Path.GetFileNameWithoutExtension(f);
    }
}
