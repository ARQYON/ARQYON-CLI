using ArqyonCLI.Core;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ArqyonCLI.Commands;

[Description("Hash a file (MD5 / SHA-256)")]
internal sealed class HashCommand : Command<HashCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<FILE>")]
        [Description("Path to the file to hash")]
        public string FilePath { get; set; } = "";

        [CommandOption("-a|--algo <ALGO>")]
        [Description("Hash algorithm: md5 | sha256 | all  (default: all)")]
        [DefaultValue("all")]
        public string Algorithm { get; set; } = "all";
    }

    public override int Execute(CommandContext ctx, Settings s)
    {
        if (!File.Exists(s.FilePath))
        {
            OutputRenderer.Error($"File not found: {s.FilePath}");
            return 1;
        }

        var info = new FileInfo(s.FilePath);
        OutputRenderer.Section("File Hash");
        OutputRenderer.Row("File", info.FullName);
        OutputRenderer.Row("Size", $"{info.Length:N0} bytes ({info.Length / 1024.0:F2} KB)");
        OutputRenderer.Blank();

        bool doMd5    = s.Algorithm is "md5"    or "all";
        bool doSha256 = s.Algorithm is "sha256" or "all";

        if (doMd5)
        {
            string hash = OutputRenderer.Spin("Computing MD5...", () => NativeBridge.HashMd5(s.FilePath));
            OutputRenderer.Row("MD5", hash, "yellow3");
        }

        if (doSha256)
        {
            string hash = OutputRenderer.Spin("Computing SHA-256...", () => NativeBridge.HashSha256(s.FilePath));
            OutputRenderer.Row("SHA-256", hash, "cyan1");
        }

        OutputRenderer.Blank();
        return 0;
    }
}
