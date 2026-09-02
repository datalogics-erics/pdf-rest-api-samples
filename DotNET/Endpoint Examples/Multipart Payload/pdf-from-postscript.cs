/*
 * What this sample does:
 * - Converts PostScript to PDF with a custom .joboptions profile.
 * - A .joboptions file contains Adobe Distiller-compatible conversion settings.
 *   The profile is optional; omit job_options to use default settings.
 * - pdfRest applies a supplied profile with Datalogics PDF Converter SDK.
 *   Datalogics maintains the SDK in partnership with Adobe, using the same
 *   Adobe technology that powers Distiller.
 * - Pair with /postscript for PDF refrying: PDF -> PostScript -> PDF. Some print and
 *   prepress workflows use this lossy roundtrip to rebuild or normalize page content.
 *
 * Setup (environment):
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region.
 *
 * Usage:
 *   dotnet run -- pdf-from-postscript-multipart <postscriptFile> <jobOptionsFile>
 */
using System.Net.Http.Headers;

namespace Samples.EndpointExamples.MultipartPayload;

public static class PdfFromPostscript
{
    public static async Task Execute(string[] args)
    {
        var inputPath = args.Length > 0 ? args[0] : "/path/to/sample.ps";
        var jobOptionsPath = args.Length > 1 ? args[1] : "/path/to/custom.joboptions";
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Missing PDFREST_API_KEY");
        var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var form = new MultipartFormDataContent();
        var inputContent = new ByteArrayContent(await File.ReadAllBytesAsync(inputPath));
        inputContent.Headers.ContentType = new MediaTypeHeaderValue("application/postscript");
        form.Add(inputContent, "file", Path.GetFileName(inputPath));
        var jobOptionsContent = new ByteArrayContent(await File.ReadAllBytesAsync(jobOptionsPath));
        jobOptionsContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(jobOptionsContent, "job_options", Path.GetFileName(jobOptionsPath));
        form.Add(new StringContent("pdf_from_postscript"), "output");
        var response = await client.PostAsync("pdf", form);
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine(result);
        if (!response.IsSuccessStatusCode) Environment.ExitCode = 1;
    }
}
