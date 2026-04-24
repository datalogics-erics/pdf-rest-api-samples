/*
 * What this sample does:
 * - Applies TDM rights metadata to a PDF via multipart/form-data.
 * - Routed from Program.cs as: `dotnet run -- tdm-reserved-pdf-multipart <inputFile>`.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 *
 * Usage:
 *   dotnet run -- tdm-reserved-pdf-multipart /path/to/input.pdf
 *
 * Output:
 * - Prints the JSON response. Validation errors (args/env) exit non-zero.
 */

using System.Text;

namespace Samples.EndpointExamples.MultipartPayload
{
    public static class TdmReservedPdf
    {
        public static async Task Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.Error.WriteLine("tdm-reserved-pdf-multipart requires <inputFile>");
                Environment.Exit(1);
                return;
            }
            var inputPath = args[0];
            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"File not found: {inputPath}");
                Environment.Exit(1);
                return;
            }
            var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.Error.WriteLine("Missing required environment variable: PDFREST_API_KEY");
                Environment.Exit(1);
                return;
            }
            var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";

            using (var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) })
            using (var tdmReservedPdfRequest = new HttpRequestMessage(HttpMethod.Post, "tdm-reserved-pdf"))
            {
                tdmReservedPdfRequest.Headers.TryAddWithoutValidation("Api-Key", apiKey);
                tdmReservedPdfRequest.Headers.Accept.Add(new("application/json"));
                var multipartContent = new MultipartFormDataContent();

                var byteArray = File.ReadAllBytes(inputPath);
                var byteAryContent = new ByteArrayContent(byteArray);
                multipartContent.Add(byteAryContent, "file", Path.GetFileName(inputPath));
                byteAryContent.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");

                var policyValue = new ByteArrayContent(Encoding.UTF8.GetBytes("https://example.com/tdm-policy"));
                multipartContent.Add(policyValue, "policy");

                tdmReservedPdfRequest.Content = multipartContent;
                var response = await httpClient.SendAsync(tdmReservedPdfRequest);
                var apiResult = await response.Content.ReadAsStringAsync();

                Console.WriteLine("TDM metadata response received.");
                Console.WriteLine(apiResult);
            }
        }
    }
}
