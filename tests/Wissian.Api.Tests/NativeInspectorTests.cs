using System.Text;
using Wissian.Host.Modules;

namespace Wissian.Api.Tests;

public static class NativeInspectorTests
{
    public static async Task<int> RunAsync()
    {
        using var inspector = new WindowsAttachmentInspector();
        var benign = await inspector.InspectAsync(Encoding.UTF8.GetBytes("Documento sintético de integração.\n"), ".txt", default);
        // Official harmless EICAR test pattern, assembled only in memory. Never execute or persist it.
        // https://www.eicar.org/download-anti-malware-testfile/
        var standardTest = Encoding.ASCII.GetBytes(string.Join("", new[]
        { "X5O!P%@AP[4\\PZX54(P^)7CC)7}$", "EICAR-STANDARD-ANTIVIRUS-TEST-FILE!", "$H+H*" }));
        var detected = await inspector.InspectAsync(standardTest, ".txt", default);
        Array.Clear(standardTest);
        var pass = benign.State == "accepted" && detected.State == "blocked";
        Console.WriteLine($"NATIVE INSPECTOR: benign={benign.State}; EICAR={detected.State}; {(pass ? "PASS" : "FAIL")}");
        return pass ? 0 : 1;
    }
}
