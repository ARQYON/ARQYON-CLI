using ArqyonCLI.Core;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ArqyonCLI.Commands;

[Description("Run environment diagnostics and health checks")]
internal sealed class DoctorCommand : Command<DoctorCommand.Settings>
{
    public sealed class Settings : CommandSettings { }

    public override int Execute(CommandContext ctx, Settings s)
    {
        OutputRenderer.Section("ARQYON Doctor");
        OutputRenderer.Info("Running diagnostics...");
        OutputRenderer.Blank();

        int passed = 0, failed = 0;

        // ── Internet ─────────────────────────────────────────────────────────
        bool internet = OutputRenderer.Spin("Checking internet...", () => NativeBridge.CheckInternet());
        OutputRenderer.StatusRow("Internet (8.8.8.8)", internet);
        if (internet) passed++; else failed++;

        // ── Dev tools ─────────────────────────────────────────────────────────
        var tools = new (string Name, Func<string> Getter)[]
        {
            ("Git",      NativeBridge.GitVersion),
            ("Python",   NativeBridge.PythonVersion),
            ("Node.js",  NativeBridge.NodeVersion),
            (".NET SDK", NativeBridge.DotnetVersion),
        };

        foreach (var (name, getter) in tools)
        {
            string v = OutputRenderer.Spin($"Checking {name}...", getter);
            bool ok = !v.Contains("not found", StringComparison.OrdinalIgnoreCase) && v.Length > 2;
            OutputRenderer.StatusRow(name, ok, ok ? v : "not installed");
            if (ok) passed++; else failed++;
        }

        // ── Environment variables ─────────────────────────────────────────────
        var envVars = new[] { "PATH", "USERPROFILE", "TEMP" };
        foreach (var ev in envVars)
        {
            string val = OutputRenderer.Spin($"Checking ${ev}...", () => NativeBridge.EnvVar(ev));
            bool ok = !val.Contains("not set");
            OutputRenderer.StatusRow($"${ev}", ok, ok ? "(set)" : "not set");
            if (ok) passed++; else failed++;
        }

        // ── Disk space check ──────────────────────────────────────────────────
        long freeGb = NativeBridge.DiskFreeGb(@"C:\");
        bool diskOk = freeGb > 5;
        OutputRenderer.StatusRow("Disk free (C:\\) > 5 GB", diskOk, $"{freeGb} GB free");
        if (diskOk) passed++; else failed++;

        // ── Summary ───────────────────────────────────────────────────────────
        OutputRenderer.Blank();
        OutputRenderer.Section("Summary");
        OutputRenderer.StatusRow($"{passed} checks passed", true, "");
        if (failed > 0)
            OutputRenderer.StatusRow($"{failed} checks failed", false, "");

        OutputRenderer.Blank();
        return failed > 0 ? 1 : 0;
    }
}
