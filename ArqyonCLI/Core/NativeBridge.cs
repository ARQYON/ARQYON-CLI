using System.Runtime.InteropServices;

namespace ArqyonCLI.Core;

/// <summary>
/// P/Invoke bridge to ArqyonNative.dll (C++ layer).
/// All string results are written into caller-allocated char buffers.
/// </summary>
internal static class NativeBridge
{
    private const string Dll = "ArqyonNative";

    // ─── System Info ─────────────────────────────────────────────────────────
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_os_name([Out] byte[] buf, int len);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_cpu_name([Out] byte[] buf, int len);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int get_cpu_core_count();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern long get_total_ram_mb();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern long get_free_ram_mb();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_uptime([Out] byte[] buf, int len);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_hostname([Out] byte[] buf, int len);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_username([Out] byte[] buf, int len);

    // ─── Disk ─────────────────────────────────────────────────────────────────
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern long get_disk_total_gb(string drive);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern long get_disk_free_gb(string drive);

    // ─── Dev Environment ──────────────────────────────────────────────────────
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_python_version([Out] byte[] buf, int len);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_node_version([Out] byte[] buf, int len);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_git_version([Out] byte[] buf, int len);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_cmake_version([Out] byte[] buf, int len);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_dotnet_version([Out] byte[] buf, int len);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_gcc_version([Out] byte[] buf, int len);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_clang_version([Out] byte[] buf, int len);

    // ─── Hashing ──────────────────────────────────────────────────────────────
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern int hash_md5(string filepath, [Out] byte[] outBuf, int outLen);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern int hash_sha256(string filepath, [Out] byte[] outBuf, int outLen);

    // ─── Diagnostics ──────────────────────────────────────────────────────────
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern int check_internet(string host);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void get_env_var(string name, [Out] byte[] outBuf, int outLen);

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private static string Read(Action<byte[], int> fn, int size = 512)
    {
        var buf = new byte[size];
        fn(buf, size);
        int end = Array.IndexOf(buf, (byte)0);
        return System.Text.Encoding.ASCII.GetString(buf, 0, end < 0 ? size : end).Trim();
    }

    // ─── Public API ───────────────────────────────────────────────────────────
    public static string OsName()       => Read((b, l) => get_os_name(b, l));
    public static string CpuName()      => Read((b, l) => get_cpu_name(b, l));
    public static int    CpuCores()     => get_cpu_core_count();
    public static long   TotalRamMb()   => get_total_ram_mb();
    public static long   FreeRamMb()    => get_free_ram_mb();
    public static string Uptime()       => Read((b, l) => get_uptime(b, l));
    public static string Hostname()     => Read((b, l) => get_hostname(b, l));
    public static string Username()     => Read((b, l) => get_username(b, l));

    public static long DiskTotalGb(string drive) => get_disk_total_gb(drive);
    public static long DiskFreeGb(string drive)  => get_disk_free_gb(drive);

    public static string PythonVersion() => Read((b, l) => get_python_version(b, l));
    public static string NodeVersion()   => Read((b, l) => get_node_version(b, l));
    public static string GitVersion()    => Read((b, l) => get_git_version(b, l));
    public static string CMakeVersion()  => Read((b, l) => get_cmake_version(b, l));
    public static string DotnetVersion() => Read((b, l) => get_dotnet_version(b, l));
    public static string GccVersion()    => Read((b, l) => get_gcc_version(b, l));
    public static string ClangVersion()  => Read((b, l) => get_clang_version(b, l));

    public static string HashMd5(string path)
    {
        var buf = new byte[64];
        return hash_md5(path, buf, buf.Length) == 0
            ? System.Text.Encoding.ASCII.GetString(buf).TrimEnd('\0')
            : "error";
    }

    public static string HashSha256(string path)
    {
        var buf = new byte[80];
        return hash_sha256(path, buf, buf.Length) == 0
            ? System.Text.Encoding.ASCII.GetString(buf).TrimEnd('\0')
            : "error";
    }

    public static bool CheckInternet(string host = "8.8.8.8") => check_internet(host) == 1;

    public static string EnvVar(string name)
    {
        var buf = new byte[512];
        get_env_var(name, buf, buf.Length);
        int end = Array.IndexOf(buf, (byte)0);
        return System.Text.Encoding.ASCII.GetString(buf, 0, end < 0 ? 512 : end).Trim();
    }
}
