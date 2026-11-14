using ArqyonCLI.Core;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ArqyonCLI.Commands;

[Description("Run a Python automation plugin")]
internal sealed class RunCommand : Command<RunCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[PLUGIN]")]
        [Description("Plugin name or .py file path")]
        public string? Plugin { get; set; }

        [CommandOption("-l|--list")]
        [Description("List available plugins")]
        public bool List { get; set; }

        [CommandArgument(1, "[ARGS]")]
        [Description("Arguments to forward to the plugin")]
        public string[] PluginArgs { get; set; } = [];
    }

    public override int Execute(CommandContext ctx, Settings s)
    {
        if (s.List || string.IsNullOrEmpty(s.Plugin))
        {
            OutputRenderer.Section("Available Plugins");
            var plugins = PythonBridge.ListPlugins().ToList();
            if (plugins.Count == 0)
            {
                OutputRenderer.Info("No plugins found in the plugins/ directory.");
            }
            else
            {
                foreach (var p in plugins)
                    AnsiConsole.MarkupLine($"  [cyan1]·[/] {p}");
            }
            OutputRenderer.Blank();
            return 0;
        }

        OutputRenderer.Section($"Running Plugin: {s.Plugin}");

        var (exitCode, output, error) = PythonBridge.RunPlugin(s.Plugin, s.PluginArgs);

        if (exitCode == -1)
        {
            OutputRenderer.Error(error);
            return 1;
        }

        if (!string.IsNullOrWhiteSpace(output))
            AnsiConsole.WriteLine(output);

        if (!string.IsNullOrWhiteSpace(error))
            OutputRenderer.Warning(error);

        OutputRenderer.Info($"Plugin exited with code {exitCode}.");
        OutputRenderer.Blank();
        return exitCode;
    }
}
