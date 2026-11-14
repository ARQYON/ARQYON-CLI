using ArqyonCLI.Commands;
using ArqyonCLI.Core;
using Spectre.Console.Cli;

// ─── Print banner on every run ────────────────────────────────────────────────
OutputRenderer.PrintBanner();

// ─── Build the CLI app ────────────────────────────────────────────────────────
var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("arqyon");
    config.SetApplicationVersion("1.0.0");
    config.AddExample(["system"]);
    config.AddExample(["system", "--disk"]);
    config.AddExample(["dev"]);
    config.AddExample(["hash", "myfile.exe"]);
    config.AddExample(["hash", "myfile.exe", "--algo", "sha256"]);
    config.AddExample(["doctor"]);
    config.AddExample(["run", "cleanup"]);

    // ── system ────────────────────────────────────────────────────────────────
    config.AddCommand<SystemCommand>("system")
        .WithDescription("Show system information (CPU, RAM, OS, uptime)")
        .WithExample(["system"])
        .WithExample(["system", "--disk", "--drive", "D:\\"]);

    // ── dev ───────────────────────────────────────────────────────────────────
    config.AddCommand<DevCommand>("dev")
        .WithDescription("Detect installed developer tools and runtimes")
        .WithExample(["dev"])
        .WithExample(["dev", "--all"]);

    // ── hash ──────────────────────────────────────────────────────────────────
    config.AddCommand<HashCommand>("hash")
        .WithDescription("Compute file hash (MD5 / SHA-256)")
        .WithExample(["hash", "build.exe"])
        .WithExample(["hash", "build.exe", "-a", "sha256"]);

    // ── doctor ────────────────────────────────────────────────────────────────
    config.AddCommand<DoctorCommand>("doctor")
        .WithDescription("Run environment diagnostics and health checks")
        .WithExample(["doctor"]);

    // ── run ───────────────────────────────────────────────────────────────────
    config.AddCommand<RunCommand>("run")
        .WithDescription("Run a Python automation plugin")
        .WithExample(["run", "--list"])
        .WithExample(["run", "cleanup"]);
});

return app.Run(args);
