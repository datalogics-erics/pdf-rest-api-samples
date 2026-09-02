/*
 * What this sample does:
 * - Converts PDF to PostScript through the /postscript endpoint.
 * - Pair with /pdf for PDF refrying: PDF -> PostScript -> PDF. Some print and prepress
 *   workflows use this lossy roundtrip to rebuild, flatten, or normalize page content.
 * - Requests Level 3, text-safe output, all pages at original scale, shrink-to-fit
 *   without rotation, and printable annotations.
 *
 * Setup (environment):
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region.
 *
 * Usage:
 *   dotnet run -- postscript-multipart <inputFile>
 */
using System.Net.Http.Headers;

namespace Samples.EndpointExamples.MultipartPayload;

public static class Postscript
{
    public static async Task Execute(string[] args)
    {
        var inputPath = args.Length > 0 ? args[0] : "/path/to/sample.pdf";
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Missing PDFREST_API_KEY");
        var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var form = new MultipartFormDataContent();
        var inputContent = new ByteArrayContent(await File.ReadAllBytesAsync(inputPath));
        inputContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(inputContent, "file", Path.GetFileName(inputPath));
        form.Add(new StringContent("3"), "ps_level");
        form.Add(new StringContent("all"), "page_range");
        form.Add(new StringContent("false"), "binary_output");
        form.Add(new StringContent("1"), "scale");
        form.Add(new StringContent("false"), "rotate");
        form.Add(new StringContent("true"), "shrink_to_fit");
        form.Add(new StringContent("true"), "print_annotations");
        form.Add(new StringContent("postscript_from_pdf"), "output");
        var response = await client.PostAsync("postscript", form);
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine(result);
        if (!response.IsSuccessStatusCode) Environment.ExitCode = 1;
    }
}
