using System.Security.Cryptography;
using System.Text;

namespace Wissian.Documents;

public sealed record ValidatedAttachment(string FileName, string MediaType, byte[] Bytes, string Sha256);
public static class AttachmentPolicy
{
    public const int MaxFileBytes = 10 * 1024 * 1024;
    public const long MaxOrganizationBytes = 100L * 1024 * 1024;
    public static ValidatedAttachment Validate(string fileName, string mediaType, byte[] bytes)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 180 || fileName != fileName.Trim()
            || fileName.StartsWith('.') || fileName.Any(c => char.IsControl(c) || "/\\:*?\"<>|".Contains(c))
            || bytes.Length is < 1 or > MaxFileBytes) throw new ArgumentException("Anexo inválido.");
        var data = bytes.AsSpan();
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowed = (extension, mediaType) switch
        {
            (".pdf", "application/pdf") => data.StartsWith("%PDF-"u8) && data.Length >= 12
                && data[^Math.Min(data.Length, 1024)..].IndexOf("%%EOF"u8) >= 0,
            (".png", "image/png") => data.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            (".jpg" or ".jpeg", "image/jpeg") => data.Length >= 4 && data[0] == 0xff && data[1] == 0xd8
                && data[^2] == 0xff && data[^1] == 0xd9,
            (".txt", "text/plain") => IsText(bytes),
            _ => false
        };
        if (!allowed) throw new ArgumentException("Tipo ou conteúdo do anexo inválido.");
        // A signature check is only a format gate. Malware inspection is a separate required boundary.
        return new(fileName, mediaType, bytes, Convert.ToHexString(SHA256.HashData(bytes)));
    }
    private static bool IsText(byte[] bytes)
    {
        try { return new UTF8Encoding(false, true).GetString(bytes).All(c => !char.IsControl(c) || c is '\r' or '\n' or '\t'); }
        catch (DecoderFallbackException) { return false; }
    }
}

public sealed record AttachmentInspection(string State, string Scanner)
{
    public static AttachmentInspection Unavailable { get; } = new("quarantined", "unavailable");
}
public interface IAttachmentInspector
{
    Task<AttachmentInspection> InspectAsync(byte[] bytes, string extension, CancellationToken ct);
}
