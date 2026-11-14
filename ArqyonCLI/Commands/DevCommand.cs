using ArqyonCLI.Core;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ArqyonCLI.Commands;

[Description("Detect developer environment and installed tools")]
internal sealed class DevCommand : Command<DevCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--all")]
        [Description("Show all detected tools including not-found ones")]
        public bool ShowAll { get; set; }
    }

    public override int Execute(CommandContext ctx, Settings s)
    {
        OutputRenderer.Section("Developer Environment");

        var tools = new (string Label, Func<string> Getter)[]
        {
            ("Python",   NativeBridge.PythonVersion),
            ("Node.js",  NativeBridge.NodeVersion),
            ("Git",      NativeBridge.GitVersion),
            ("CMake",    NativeBridge.CMakeVersion),
            (".NET SDK", NativeBridge.DotnetVersion),
            ("GCC",      NativeBridge.GccVersion),
            ("Clang",    NativeBridge.ClangVersion),
        };

        int found = 0;
        foreach (var (label, getter) in tools)
        {
            string version = OutputRenderer.Spin(
                $"Detecting {label}...", getter);

            bool ok = !version.Contains("not found", StringComparison.OrdinalIgnoreCase)
                   && !version.Contains("'", StringComparison.Ordinal)   // cmd error
                   && version.Length > 2;

            if (ok) found++;
            if (ok || s.ShowAll)
                OutputRenderer.StatusRow(label, ok, ok ? version : "not found");
        }

        OutputRenderer.Blank();
        OutputRenderer.Info($"{found}/{tools.Length} tools detected.");
        OutputRenderer.Blank();
        return 0;
    }
}
