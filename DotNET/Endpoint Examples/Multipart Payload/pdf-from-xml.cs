/*
 * What this sample does:
 * - Converts structured input to PDF through the /pdf endpoint.
 *
 * Setup (environment):
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region.
 *
 * Usage:
 *   dotnet run -- pdf-from-xml-multipart <inputFile>
 */
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Samples.EndpointExamples.MultipartPayload;

public static class PdfFromXml
{
    public static async Task Execute(string[] args)
    {
        var inputPath = args.Length > 0 ? args[0] : "/path/to/sample.xml";
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Missing PDFREST_API_KEY");
        var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var options = JObject.Parse(@"{""title"":""Structured Content Sample"",""language"":""en-US"",""enable_tagging"":true,""page_setup"":{""size"":""Letter"",""orientation"":""portrait"",""margin"":{""top"":36,""right"":42,""bottom"":36,""left"":42}},""style"":{""font"":""Arial"",""heading_font"":""Arial"",""code_font"":""Courier"",""text_size"":11,""text_color_rgb"":[34,34,34],""heading_scale"":1.35,""table"":{""column_width_weights"":[2,3,2],""keep_header_with_first_row"":true,""repeat_headers_on_overflow"":true,""show_borders"":true,""border_width"":0.75,""border_color_rgb"":[180,188,200],""header_fill_color_rgb"":[33,64,98],""header_text_color_rgb"":[255,255,255],""row_fill_color_rgb"":[250,250,252],""alternate_row_fill_color_rgb"":[235,240,246],""cell_padding"":{""top"":6,""right"":8,""bottom"":6,""left"":8}}},""data_presentation"":""hierarchy""}");
        using var form = new MultipartFormDataContent();
        var inputContent = new ByteArrayContent(await File.ReadAllBytesAsync(inputPath));
        inputContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(inputContent, "file", Path.GetFileName(inputPath));
        form.Add(new StringContent(options.ToString()), "structured_text_options");
        var response = await client.PostAsync("pdf", form);
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine(result);
        if (!response.IsSuccessStatusCode) Environment.ExitCode = 1;
    }
}
