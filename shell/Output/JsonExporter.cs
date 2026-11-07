// ============================================================
//  ARQYON SHELL — JsonExporter.cs
//  Writes diagnostic reports to .json or .html files.
// ============================================================

using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ArqyonShell.Output;

internal static class JsonExporter
{
  // ── JSON ──────────────────────────────────────────────────

  public static string ExportJson(string json, string? outputPath = null)
  {
    // Pretty-print the JSON
    string pretty = json;
    try
    {
      var doc = JsonDocument.Parse(json);
      pretty = JsonSerializer.Serialize(doc.RootElement,
          new JsonSerializerOptions { WriteIndented = true });
    }
    catch { /* keep raw if parse fails */ }

    string path = outputPath ?? $"arqyon-diag-{DateTime.Now:yyyyMMdd-HHmmss}.json";
    File.WriteAllText(path, pretty, Encoding.UTF8);
    return path;
  }

  // ── HTML ──────────────────────────────────────────────────

  public static string ExportHtml(string json, string? outputPath = null)
  {
    string path = outputPath ?? $"arqyon-diag-{DateTime.Now:yyyyMMdd-HHmmss}.html";

    string pretty = json;
    try
    {
      var doc = JsonDocument.Parse(json);
      pretty = JsonSerializer.Serialize(doc.RootElement,
          new JsonSerializerOptions { WriteIndented = true });
    }
    catch { }

    string escaped = System.Net.WebUtility.HtmlEncode(pretty);

    string html = $$"""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <title>ARQYON Diagnostic Report</title>
  <style>
    :root {
      --bg: #0d0d0d; --surface: #141414; --border: #2a2a2a;
      --accent: #00d4ff; --text: #e0e0e0; --dim: #666;
      --green: #22c55e; --yellow: #eab308;
    }
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      background: var(--bg); color: var(--text);
      font-family: 'Cascadia Code', 'Fira Code', 'Consolas', monospace;
      font-size: 14px; padding: 2rem;
    }
    header { border-bottom: 1px solid var(--border); padding-bottom: 1rem; margin-bottom: 2rem; }
    h1 { color: var(--accent); font-size: 1.6rem; letter-spacing: 0.15em; }
    .meta { color: var(--dim); margin-top: 0.4rem; font-size: 0.85rem; }
    pre {
      background: var(--surface); border: 1px solid var(--border);
      border-radius: 6px; padding: 1.5rem; overflow-x: auto;
      white-space: pre-wrap; word-break: break-all;
      color: var(--green); line-height: 1.6;
    }
    footer { margin-top: 2rem; color: var(--dim); font-size: 0.8rem; text-align: center; }
  </style>
</head>
<body>
  <header>
    <h1>ARQYON Diagnostic Report</h1>
    <div class="meta">Generated: {{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}} UTC</div>
  </header>
  <pre>{{escaped}}</pre>
  <footer>ARQYON CLI — Think Beyond. Build Without Limits.</footer>
</body>
</html>
""";

    File.WriteAllText(path, html, Encoding.UTF8);
    return path;
  }

}
