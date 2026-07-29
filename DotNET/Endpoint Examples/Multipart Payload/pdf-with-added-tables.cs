/*
 * What this sample does:
 * - Adds a tagged project-status table with styled headers, status cells, and a footer.
 * - Routed from Program.cs as: `dotnet run -- pdf-with-added-tables-multipart <inputFile>`.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */

using Newtonsoft.Json.Linq;
using System.Text;
using Samples;

namespace Samples.EndpointExamples.MultipartPayload;

public static class PdfWithAddedTables
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "pdf-with-added-tables-multipart");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "pdf-with-added-tables");
        request.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        request.Headers.Accept.Add(new("application/json"));
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(await File.ReadAllBytesAsync(inputPath)), "file", Path.GetFileName(inputPath));
        content.Add(new StringContent(CreateTable().ToString(), Encoding.UTF8), "table_objects");
        content.Add(new StringContent("true"), "tag_enabled");
        content.Add(new StringContent("en-US"), "tag_language");
        content.Add(new StringContent("project-status"), "output");
        request.Content = content;

        await SampleInput.PrintResponse(client, request);
    }

    internal static JArray CreateTable() => JArray.Parse("""
        [{
          "page":1,"x":54,"y":540,"width":504,
          "columns":[{"width":210},{"width":144},{"width":150}],
          "tag_structure_type":"Table",
          "style":{"padding":{"top":6,"right":8,"bottom":6,"left":8},"text_size":10},
          "header_rows":[{"cells":[
            {"text":"Milestone","tag_structure_type":"TH","style":{"background_color_rgb":[26,72,112],"text_color_rgb":[255,255,255]}},
            {"text":"Owner","tag_structure_type":"TH","style":{"background_color_rgb":[26,72,112],"text_color_rgb":[255,255,255]}},
            {"text":"Status","tag_structure_type":"TH","style":{"background_color_rgb":[26,72,112],"text_color_rgb":[255,255,255]}}
          ]}],
          "rows":[
            {"cells":[{"text":"Requirements review"},{"text":"Maya Chen"},{"text":"Complete","tag_structure_type":"TD","style":{"background_color_rgb":[220,252,231]}}]},
            {"cells":[{"text":"Prototype delivery"},{"text":"Jordan Lee"},{"text":"In progress","tag_structure_type":"TD","style":{"background_color_rgb":[254,249,195]}}]},
            {"cells":[{"text":"Stakeholder approval"},{"text":"Avery Patel"},{"text":"Planned","tag_structure_type":"TD","style":{"background_color_rgb":[239,246,255]}}]}
          ],
          "footer_rows":[{"cells":[{"text":"Next review: Friday, 10:00 AM","col_span":3,"tag_structure_type":"TD","style":{"background_color_rgb":[245,247,250],"text_color_rgb":[55,65,81]}}]}]
        }]
        """);
}
