/*
 * What this sample does:
 * - Converts PDF to PostScript and then converts the PostScript back to PDF.
 * - This PDF -> PostScript -> PDF roundtrip is commonly called PDF refrying.
 * - Some print, prepress, and legacy production workflows use it to rebuild,
 *   flatten, or normalize page content for downstream systems.
 * - Refrying is intentionally lossy and may remove tags, forms, layers,
 *   annotations, transparency, metadata, and editability.
 * - The PostScript-to-PDF step uses a custom .joboptions profile in this
 *   sample. A .joboptions file contains Adobe Distiller-compatible conversion
 *   settings; it is optional, and default settings are used when omitted.
 * - pdfRest applies the profile with Datalogics PDF Converter SDK. Datalogics
 *   maintains the SDK in partnership with Adobe, using the same Adobe
 *   technology that powers Distiller.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR
 *   compliance and proximity, use PDFREST_URL=https://eu-api.pdfrest.com
 *
 * Usage:
 *   dotnet run -- refry-pdf <pdf> <jobOptions> [outputPdf]
 *
 * Output:
 * - Prints each API result and downloads refried.pdf unless outputPdf is set.
 */

using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace Samples.ComplexFlowExamples;

public static class RefryPdf
{
    public static async Task Execute(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("refry-pdf requires <pdf> <jobOptions> [outputPdf]");
        }

        var inputPath = args[0];
        var jobOptionsPath = args[1];
        var outputPath = args.Length > 2 ? args[2] : Path.Combine("Complex Flow Examples", "refried.pdf");
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Missing PDFREST_API_KEY");
        }

        var baseUrl = (Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com").TrimEnd('/');
        using var client = new HttpClient(new HttpClientHandler { UseCookies = false })
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(2),
        };
        client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var postscript = await ConvertToPostscript(client, inputPath);
        var finalPdf = await ConvertToPdf(client, postscript["outputId"]!.Value<string>()!, jobOptionsPath);
        var finalId = finalPdf["outputId"]!.Value<string>()!;

        var bytes = await client.GetByteArrayAsync($"resource/{Uri.EscapeDataString(finalId)}?format=file");
        await File.WriteAllBytesAsync(outputPath, bytes);
        Console.WriteLine(finalPdf.ToString());
        Console.WriteLine($"Created {Path.GetFullPath(outputPath)}");
    }

    private static async Task<JObject> ConvertToPostscript(HttpClient client, string inputPath)
    {
        using var form = new MultipartFormDataContent();
        var input = new ByteArrayContent(await File.ReadAllBytesAsync(inputPath));
        input.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(input, "file", Path.GetFileName(inputPath));
        form.Add(new StringContent("3"), "ps_level");
        form.Add(new StringContent("all"), "page_range");
        form.Add(new StringContent("true"), "binary_output");
        form.Add(new StringContent("1"), "scale");
        form.Add(new StringContent("false"), "rotate");
        form.Add(new StringContent("true"), "shrink_to_fit");
        form.Add(new StringContent("true"), "print_annotations");
        form.Add(new StringContent("refry_intermediate"), "output");
        return await Post(client, "postscript", form);
    }

    private static async Task<JObject> ConvertToPdf(
        HttpClient client,
        string postscriptId,
        string jobOptionsPath)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(postscriptId), "id");
        var jobOptions = new ByteArrayContent(await File.ReadAllBytesAsync(jobOptionsPath));
        jobOptions.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(jobOptions, "job_options", Path.GetFileName(jobOptionsPath));
        form.Add(new StringContent("refried"), "output");
        return await Post(client, "pdf", form);
    }

    private static async Task<JObject> Post(
        HttpClient client,
        string endpoint,
        HttpContent content)
    {
        using var response = await client.PostAsync(endpoint, content);
        var text = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"{endpoint}: {(int)response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"{endpoint} failed: {text}");
        }

        return JObject.Parse(text);
    }
}
