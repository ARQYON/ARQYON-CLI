using ArqyonCLI.Core;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ArqyonCLI.Commands;

[Description("Show system information")]
internal sealed class SystemCommand : Command<SystemCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--disk")]
        [Description("Include disk usage")]
        public bool ShowDisk { get; set; }

        [CommandOption("--drive <DRIVE>")]
        [Description("Drive to inspect (default: C:\\)")]
        [DefaultValue("C:\\")]
        public string Drive { get; set; } = @"C:\";
    }

    public override int Execute(CommandContext ctx, Settings s)
    {
        OutputRenderer.Section("System Information");

        OutputRenderer.Row("OS",       NativeBridge.OsName());
        OutputRenderer.Row("Hostname", NativeBridge.Hostname());
        OutputRenderer.Row("User",     NativeBridge.Username());
        OutputRenderer.Row("CPU",      NativeBridge.CpuName());
        OutputRenderer.Row("Cores",    NativeBridge.CpuCores().ToString());

        long totalMb = NativeBridge.TotalRamMb();
        long freeMb  = NativeBridge.FreeRamMb();
        long usedMb  = totalMb - freeMb;
        OutputRenderer.Row("RAM (Total)", $"{totalMb:N0} MB ({totalMb / 1024.0:F1} GB)");
        OutputRenderer.Row("RAM (Used)",  $"{usedMb:N0} MB");
        OutputRenderer.Row("RAM (Free)",  $"{freeMb:N0} MB");
        OutputRenderer.Row("Uptime",      NativeBridge.Uptime());

        if (s.ShowDisk)
        {
            OutputRenderer.Blank();
            OutputRenderer.Section($"Disk - {s.Drive}");
            long total = NativeBridge.DiskTotalGb(s.Drive);
            long free  = NativeBridge.DiskFreeGb(s.Drive);
            if (total < 0)
            {
                OutputRenderer.Error($"Cannot read drive: {s.Drive}");
            }
            else
            {
                long used = total - free;
                OutputRenderer.Row("Total", $"{total} GB");
                OutputRenderer.Row("Used",  $"{used} GB");
                OutputRenderer.Row("Free",  $"{free} GB");

                // Visual bar
                AnsiConsole.Write(new BreakdownChart()
                    .Width(60)
                    .AddItem("Used", used, Color.Cyan1)
                    .AddItem("Free", free, Color.Grey));
            }
        }

        OutputRenderer.Blank();
        return 0;
    }
}
