/*
 * What this sample does:
 * - Validates a hybrid ZUGFeRD / Factur-X PDF without modifying it.
 *
 * Setup (environment):
 * - Copy .env.example to .env and set PDFREST_API_KEY=your_api_key_here.
 * - Optionally set PDFREST_URL=https://eu-api.pdfrest.com for the EU/GDPR service.
 *
 * Usage: dotnet run -- validated-zugferd-multipart /path/to/zugferd-invoice.pdf
 */
using System.Net.Http.Headers;

namespace Samples.EndpointExamples.MultipartPayload;

public static class ValidatedZugferd
{
    public static async Task Execute(string[] args)
    {
        var zugferdPdf = args.Length > 0 ? args[0] : "/path/to/zugferd-invoice.pdf";
        if (!File.Exists(zugferdPdf)) throw new FileNotFoundException("ZUGFeRD PDF not found.", zugferdPdf);
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY") ?? throw new InvalidOperationException("Missing PDFREST_API_KEY");
        var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        using var form = new MultipartFormDataContent();
        var pdf = new ByteArrayContent(await File.ReadAllBytesAsync(zugferdPdf));
        pdf.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(pdf, "file", Path.GetFileName(zugferdPdf));
        var response = await client.PostAsync("validated-zugferd", form);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        if (!response.IsSuccessStatusCode) Environment.ExitCode = 1;
    }
}
