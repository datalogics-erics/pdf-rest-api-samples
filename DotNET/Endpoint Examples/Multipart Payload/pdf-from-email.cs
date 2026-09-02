/*
 * What this sample does:
 * - Converts an Email (.eml) file to PDF through the /pdf endpoint.
 * - Sends the Email file directly in a multipart request.
 *
 * Setup (environment):
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region.
 *
 * Usage:
 *   dotnet run -- pdf-from-email-multipart <inputFile>
 */
using System.Net.Http.Headers;

namespace Samples.EndpointExamples.MultipartPayload;

public static class PdfFromEmail
{
    public static async Task Execute(string[] args)
    {
        var inputPath = args.Length > 0 ? args[0] : "/path/to/sample.eml";
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Missing PDFREST_API_KEY");
        var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var form = new MultipartFormDataContent();
        var inputContent = new ByteArrayContent(await File.ReadAllBytesAsync(inputPath));
        inputContent.Headers.ContentType = new MediaTypeHeaderValue("message/rfc822");
        form.Add(inputContent, "file", Path.GetFileName(inputPath));
        form.Add(new StringContent("pdf_from_email"), "output");
        var response = await client.PostAsync("pdf", form);
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine(result);
        if (!response.IsSuccessStatusCode) Environment.ExitCode = 1;
    }
}
