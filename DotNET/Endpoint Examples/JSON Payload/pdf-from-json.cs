/*
 * What this sample does:
 * - Converts structured input to PDF through the /pdf endpoint.
 *
 * Setup (environment):
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region.
 *
 * Usage:
 *   dotnet run -- pdf-from-json <inputFile>
 */
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Samples.EndpointExamples.JsonPayload;

public static class PdfFromJson
{
    public static async Task Execute(string[] args)
    {
        var inputPath = args.Length > 0 ? args[0] : "/path/to/sample.json";
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Missing PDFREST_API_KEY");
        var baseUrl = Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com";
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.TryAddWithoutValidation("Api-Key", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var options = JObject.Parse(@"{""title"":""Structured Content Sample"",""language"":""en-US"",""enable_tagging"":true,""page_setup"":{""size"":""Letter"",""orientation"":""portrait"",""margin"":{""top"":36,""right"":42,""bottom"":36,""left"":42}},""style"":{""font"":""Arial"",""heading_font"":""Arial"",""code_font"":""Courier"",""text_size"":11,""text_color_rgb"":[34,34,34],""heading_scale"":1.35,""table"":{""column_width_weights"":[2,3,2],""keep_header_with_first_row"":true,""repeat_headers_on_overflow"":true,""show_borders"":true,""border_width"":0.75,""border_color_rgb"":[180,188,200],""header_fill_color_rgb"":[33,64,98],""header_text_color_rgb"":[255,255,255],""row_fill_color_rgb"":[250,250,252],""alternate_row_fill_color_rgb"":[235,240,246],""cell_padding"":{""top"":6,""right"":8,""bottom"":6,""left"":8}}},""data_presentation"":""hierarchy""}");
        var inputId = await UploadAsync(client, inputPath);
        var payload = new JObject { ["id"] = inputId, ["structured_text_options"] = options };
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

