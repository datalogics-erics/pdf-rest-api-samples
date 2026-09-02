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
 *   dotnet run -- pdf-from-postscript <postscriptFile> <jobOptionsFile>
 */
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Samples.EndpointExamples.JsonPayload;

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
        var inputId = await UploadAsync(client, inputPath);
        var jobOptionsId = await UploadAsync(client, jobOptionsPath);
        var payload = new JObject
        {
            ["id"] = inputId,
            ["job_options_id"] = jobOptionsId,
            ["output"] = "pdf_from_postscript",
        };
        using var content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("pdf", content);
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine(result);
        if (!response.IsSuccessStatusCode) Environment.ExitCode = 1;
    }

    private static async Task<string> UploadAsync(HttpClient client, string path)
    {
        using var content = new ByteArrayContent(await File.ReadAllBytesAsync(path));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        using var request = new HttpRequestMessage(HttpMethod.Post, "upload") { Content = content };
        request.Headers.Add("Content-Filename", Path.GetFileName(path));
        var response = await client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException(result);
        return JObject.Parse(result)["files"]![0]!["id"]!.Value<string>()!;
    }
}
