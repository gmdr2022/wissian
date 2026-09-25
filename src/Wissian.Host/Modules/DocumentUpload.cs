using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Wissian.Documents;

namespace Wissian.Host.Modules;

public sealed class DocumentTransferSlots : IDisposable
{
    private readonly SemaphoreSlim slots = new(2, 2);
    public bool TryEnter() => slots.Wait(0);
    public void Exit() => slots.Release();
    public void Dispose() => slots.Dispose();
}

internal static class DocumentUpload
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
    public static async Task<(DocumentCommand Command, ValidatedAttachment File)> ReadAsync(HttpContext context)
    {
        try { return await ReadMultipartAsync(context); }
        catch (IOException error) when (error is not BadHttpRequestException)
        { throw new ArgumentException("Envio multipart incompleto.", error); }
    }
    private static async Task<(DocumentCommand Command, ValidatedAttachment File)> ReadMultipartAsync(HttpContext context)
    {
        var request = context.Request;
        const long limit = AttachmentPolicy.MaxFileBytes + 16 * 1024;
        if (request.ContentLength > limit) throw new BadHttpRequestException("Anexo excede o limite.", 413);
        var bodyLimit = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (bodyLimit is not { IsReadOnly: false }) throw new InvalidOperationException("Limite de upload indisponível.");
        bodyLimit.MaxRequestBodySize = limit;
        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var type) || type.MediaType != "multipart/form-data")
            throw new ArgumentException("Envio multipart obrigatório.");
        var boundary = HeaderUtilities.RemoveQuotes(type.Boundary).Value;
        if (string.IsNullOrEmpty(boundary) || boundary.Length > 128) throw new ArgumentException("Separador inválido.");
        var reader = new MultipartReader(boundary, request.Body)
        { HeadersCountLimit = 8, HeadersLengthLimit = 4096, BodyLengthLimit = limit };
        DocumentCommand? command = null;
        ValidatedAttachment? file = null;
        var count = 0;
        while (await reader.ReadNextSectionAsync(context.RequestAborted) is { } section)
        {
            if (++count > 2 || !ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition)
                || disposition.DispositionType != "form-data") throw new ArgumentException("Partes inválidas.");
            var name = HeaderUtilities.RemoveQuotes(disposition.Name).Value;
            if (name == "command" && command is null && !disposition.FileName.HasValue && !disposition.FileNameStar.HasValue)
            {
                command = JsonSerializer.Deserialize<DocumentCommand>(await ReadBounded(section.Body, 8192, context.RequestAborted), Json)
                    ?? throw new ArgumentException("Comando obrigatório.");
            }
            else if (name == "file" && file is null)
            {
                var fileName = HeaderUtilities.RemoveQuotes(disposition.FileNameStar.HasValue ? disposition.FileNameStar : disposition.FileName).Value;
                file = AttachmentPolicy.Validate(fileName ?? "", section.ContentType ?? "",
                    await ReadBounded(section.Body, AttachmentPolicy.MaxFileBytes, context.RequestAborted));
            }
            else throw new ArgumentException("Parte inesperada ou repetida.");
        }
        if (command is null || file is null) throw new ArgumentException("Comando e anexo obrigatórios.");
        return (DocumentStore.Normalize(command, true), file);
    }
    private static async Task<byte[]> ReadBounded(Stream source, int limit, CancellationToken ct)
    {
        using var target = new MemoryStream();
        var buffer = new byte[16384];
        int read;
        while ((read = await source.ReadAsync(buffer, ct)) != 0)
        {
            if (target.Length + read > limit) throw new BadHttpRequestException("Parte excede o limite.", 413);
            await target.WriteAsync(buffer.AsMemory(0, read), ct);
        }
        return target.ToArray();
    }
}
