/*
 * What this sample does:
 * - Creates a ZUGFeRD / Factur-X PDF/A-3 invoice from invoice XML through multipart/form-data.
 *
 * Setup (environment):
 * - Copy .env.example to .env and set PDFREST_API_KEY=your_api_key_here.
 * - Optionally set PDFREST_URL=https://eu-api.pdfrest.com for the EU/GDPR service.
 *
 * Usage: dotnet run -- zugferd-pdf-multipart /path/to/invoice.xml
 */
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Samples.EndpointExamples.MultipartPayload;

public static class ZugferdPdf
{
    public static async Task Execute(string[] args)
    {
        var invoiceXml = args.Length > 0 ? args[0] : "/path/to/invoice.xml";
        if (!File.Exists(invoiceXml)) throw new FileNotFoundException("Invoice XML not found.", invoiceXml);
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY") ?? throw new InvalidOperationException("Missing PDFREST_API_KEY");
        var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        using var form = new MultipartFormDataContent();
        var xml = new ByteArrayContent(await File.ReadAllBytesAsync(invoiceXml));
        xml.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        form.Add(xml, "file", Path.GetFileName(invoiceXml));
        form.Add(new StringContent(new JObject { ["locale"] = "de-DE", ["label_language"] = "de", ["font"] = "arial", ["bold_font"] = "arialbold", ["accent_color_rgb"] = new JArray(0, 92, 171) }.ToString()), "render_options");
        form.Add(new StringContent("zugferd_invoice"), "output");
        var response = await client.PostAsync("zugferd-pdf", form);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        if (!response.IsSuccessStatusCode) Environment.ExitCode = 1;
    }
}
