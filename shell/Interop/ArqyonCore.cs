// ============================================================
//  ARQYON SHELL — ArqyonCore.cs
//  P/Invoke bridge to arqyon_core.dll.
//
//  MEMORY RULE: Every method that returns a string internally
//  calls a native char*, then immediately frees it via
//  FreeCoreString(). Callers receive a managed string and
//  never touch native memory.
// ============================================================

using System;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ArqyonShell.Interop;

internal static class ArqyonCore
{
    private const string DLL = "arqyon_core";

    // ── Raw P/Invoke declarations ─────────────────────────────
    // All char* returns must be freed via FreeCoreString.
    // These are private — consumers use the safe wrappers below.

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern void FreeCoreString(IntPtr ptr);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetCpuInfo();

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetRamInfo();

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetOsInfo();

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetDiskInfo();

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr DetectLanguages();

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ScanEnvPath();

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr HashFileSHA256([MarshalAs(UnmanagedType.LPStr)] string path);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr HashFileMD5([MarshalAs(UnmanagedType.LPStr)] string path);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetFileInfo([MarshalAs(UnmanagedType.LPStr)] string path);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr RunDiagnostics();

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern int RunPythonScript(
        [MarshalAs(UnmanagedType.LPStr)] string scriptPath,
        [MarshalAs(UnmanagedType.LPStr)] string args);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetPythonVersion();

    // ── Safe wrapper: IntPtr → managed string + free ─────────

    private static string MarshalAndFree(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return string.Empty;
        string result = Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
        FreeCoreString(ptr);
        return result;
    }

    // ── Public API ────────────────────────────────────────────

    public static string GetCpuInfoJson()       => MarshalAndFree(GetCpuInfo());
    public static string GetRamInfoJson()       => MarshalAndFree(GetRamInfo());
    public static string GetOsInfoJson()        => MarshalAndFree(GetOsInfo());
    public static string GetDiskInfoJson()      => MarshalAndFree(GetDiskInfo());
    public static string DetectLanguagesJson()  => MarshalAndFree(DetectLanguages());
    public static string ScanEnvPathJson()      => MarshalAndFree(ScanEnvPath());
    public static string RunDiagnosticsJson()   => MarshalAndFree(RunDiagnostics());
    public static string GetPythonVersionStr()  => MarshalAndFree(GetPythonVersion());

    public static string HashFileSHA256Str(string path) => MarshalAndFree(HashFileSHA256(path));
    public static string HashFileMD5Str(string path)    => MarshalAndFree(HashFileMD5(path));
    public static string GetFileInfoJson(string path)   => MarshalAndFree(GetFileInfo(path));

    public static int RunScript(string scriptPath, string args = "")
        => RunPythonScript(scriptPath, args);

    // ── Convenience: parse JSON root object ──────────────────
    public static JsonElement ParseJson(string json)
    {
        try   { return JsonDocument.Parse(json).RootElement; }
        catch { return JsonDocument.Parse("{}").RootElement; }
    }
}
