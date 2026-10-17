using System.ComponentModel;
using System.Runtime.InteropServices;
using ProjectName.Contracts;

namespace ProjectName.Infrastructure;

public sealed class WindowsDpapiSecretProvider : ISecretProvider
{
    private readonly string _protectedRoot;
    private readonly string _canonicalRoot;

    public WindowsDpapiSecretProvider(string protectedRoot, string canonicalRoot)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("windows-dpapi-required");
        _protectedRoot = Path.GetFullPath(protectedRoot);
        _canonicalRoot = Path.GetFullPath(canonicalRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if ((_protectedRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar).StartsWith(_canonicalRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("protected-root-must-be-outside-canonical-root");
    }

    public bool Exists(SecretKeyReference reference) => File.Exists(PathFor(reference));

    public byte[] Read(SecretKeyReference reference)
    {
        var protectedBytes = File.ReadAllBytes(PathFor(reference));
        return Unprotect(protectedBytes, Entropy(reference));
    }

    public void Write(SecretKeyReference reference, byte[] value)
    {
        if (value is null || value.Length == 0) throw new ArgumentException("value-required", nameof(value));
        var path = PathFor(reference);
        Directory.CreateDirectory(_protectedRoot);
        var encrypted = Protect(value, Entropy(reference));
        using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        file.Write(encrypted);
        file.Flush(true);
    }

    private string PathFor(SecretKeyReference reference)
    {
        if (!StringComparer.OrdinalIgnoreCase.Equals(reference.Provider, "windows-dpapi"))
            throw new InvalidOperationException("provider-mismatch");
        if (reference.Generation < 1) throw new InvalidOperationException("key-generation-invalid");
        if (string.IsNullOrWhiteSpace(reference.KeyId) || reference.KeyId.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_'))
            throw new InvalidOperationException("key-id-invalid");
        var fileName = reference.KeyId + ".g" + reference.Generation + ".bin";
        return Path.Combine(_protectedRoot, fileName);
    }

    private static byte[] Entropy(SecretKeyReference reference) =>
        System.Text.Encoding.UTF8.GetBytes(reference.KeyId + "|" + reference.Generation + "|" + reference.LocationRef);

    private static byte[] Protect(byte[] input, byte[] entropy) => Transform(input, entropy, protect: true);
    private static byte[] Unprotect(byte[] input, byte[] entropy) => Transform(input, entropy, protect: false);

    private static byte[] Transform(byte[] input, byte[] entropy, bool protect)
    {
        var inputBlob = Blob.From(input);
        var entropyBlob = Blob.From(entropy);
        try
        {
            DATA_BLOB output;
            var ok = protect
                ? CryptProtectData(ref inputBlob.Value, null, ref entropyBlob.Value, IntPtr.Zero, IntPtr.Zero, 0, out output)
                : CryptUnprotectData(ref inputBlob.Value, IntPtr.Zero, ref entropyBlob.Value, IntPtr.Zero, IntPtr.Zero, 0, out output);
            if (!ok) throw new Win32Exception(Marshal.GetLastWin32Error());
            try
            {
                var result = new byte[output.cbData];
                Marshal.Copy(output.pbData, result, 0, output.cbData);
                return result;
            }
            finally { if (output.pbData != IntPtr.Zero) LocalFree(output.pbData); }
        }
        finally
        {
            inputBlob.Dispose();
            entropyBlob.Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DATA_BLOB { public int cbData; public IntPtr pbData; }

    private sealed class Blob : IDisposable
    {
        public DATA_BLOB Value;
        private Blob(byte[] data)
        {
            Value.cbData = data.Length;
            Value.pbData = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, Value.pbData, data.Length);
        }
        public static Blob From(byte[] data) => new(data);
        public void Dispose() { if (Value.pbData != IntPtr.Zero) { Marshal.FreeHGlobal(Value.pbData); Value.pbData = IntPtr.Zero; } }
    }

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptProtectData(ref DATA_BLOB pDataIn, string? szDataDescr, ref DATA_BLOB pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, int dwFlags, out DATA_BLOB pDataOut);

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptUnprotectData(ref DATA_BLOB pDataIn, IntPtr ppszDataDescr, ref DATA_BLOB pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, int dwFlags, out DATA_BLOB pDataOut);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr hMem);
}
