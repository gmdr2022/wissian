using System.Runtime.InteropServices;
using Wissian.Documents;

namespace Wissian.Host.Modules;

// No shell, administrator rights, uploaded paths, or changes to the system antivirus configuration.
public sealed class WindowsAttachmentInspector : IAttachmentInspector, IDisposable
{
    private readonly SemaphoreSlim slots = new(2, 2);
    public async Task<AttachmentInspection> InspectAsync(byte[] bytes, string extension, CancellationToken ct)
    {
        if (!OperatingSystem.IsWindows() || bytes.Length is < 1 or > AttachmentPolicy.MaxFileBytes
            || extension is not (".txt" or ".pdf" or ".png" or ".jpg" or ".jpeg") || !slots.Wait(0))
            return AttachmentInspection.Unavailable;
        // A stuck native provider retains its slot; a timeout never creates unlimited background scans.
        var task = Task.Run(() =>
        {
            try { return Scan(bytes, extension); }
            finally { slots.Release(); }
        }, CancellationToken.None);
        try { return await task.WaitAsync(TimeSpan.FromSeconds(20), ct); }
        catch (TimeoutException) { return AttachmentInspection.Unavailable; }
    }
    private static AttachmentInspection Scan(byte[] bytes, string extension)
    {
        nint context = 0;
        try
        {
            if (AmsiInitialize("Wissian.Documents/1.0", out context) != 0 || context == 0) return AttachmentInspection.Unavailable;
            var hr = AmsiScanBuffer(context, bytes, (uint)bytes.Length, "attachment" + extension, 0, out var result);
            return Classify(hr, result);
        }
        catch (DllNotFoundException) { return AttachmentInspection.Unavailable; }
        catch (EntryPointNotFoundException) { return AttachmentInspection.Unavailable; }
        catch (BadImageFormatException) { return AttachmentInspection.Unavailable; }
        finally { if (context != 0) AmsiUninitialize(context); }
    }
    public static AttachmentInspection Classify(int hresult, uint result) => hresult != 0
        ? AttachmentInspection.Unavailable
        : result is 0 or 1 ? new("accepted", "windows-amsi")
        : result >= 0x4000 ? new("blocked", "windows-amsi") : new("quarantined", "windows-amsi");

    [DllImport("amsi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int AmsiInitialize(string appName, out nint context);
    [DllImport("amsi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int AmsiScanBuffer(nint context, byte[] buffer, uint length, string contentName, nint session, out uint result);
    [DllImport("amsi.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern void AmsiUninitialize(nint context);
    // Semaphore has no native handle unless AvailableWaitHandle is requested. In-flight native scans
    // may finish after shutdown, so keep their semaphore alive until GC rather than disposing under them.
    public void Dispose() { }
}
