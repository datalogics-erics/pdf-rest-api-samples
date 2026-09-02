/*
 * What this sample does:
 * - Converts an Email (.eml) file to PDF through the /pdf endpoint.
 * - Uploads the Email file first, then passes its resource ID in the JSON request.
 *
 * Setup (environment):
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region.
 *
 * Usage:
 *   dotnet run -- pdf-from-email <inputFile>
 */
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Samples.EndpointExamples.JsonPayload;

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
        var inputId = await UploadAsync(client, inputPath);
        var payload = new JObject { ["id"] = inputId, ["output"] = "pdf_from_email" };
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
