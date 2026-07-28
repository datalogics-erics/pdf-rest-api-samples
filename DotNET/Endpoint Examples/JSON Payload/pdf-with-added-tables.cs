/*
 * What this sample does:
 * - Uploads a PDF, then adds a tagged project-status table by resource ID.
 * - Routed from Program.cs as: `dotnet run -- pdf-with-added-tables <inputFile>`.
 *
 * Setup (environment):
 * - Copy .env.example to .env
 * - Set PDFREST_API_KEY=your_api_key_here
 * - Optional: set PDFREST_URL to override the API region. For EU/GDPR compliance and proximity, use:
 *     PDFREST_URL=https://eu-api.pdfrest.com
 *   For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */

using Newtonsoft.Json.Linq;
using Samples;

namespace Samples.EndpointExamples.JsonPayload;

public static class PdfWithAddedTables
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "pdf-with-added-tables");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        var inputId = await SampleInput.UploadPdf(client, inputPath, apiKey);
        var payload = new JObject
        {
            ["id"] = inputId,
            ["table_objects"] = CreateTable(),
            ["tag_enabled"] = true,
            ["tag_language"] = "en-US",
            ["output"] = "project-status",
        };

        using var request = SampleInput.JsonRequest("pdf-with-added-tables", payload, apiKey);
        await SampleInput.PrintResponse(client, request);
    }

    private static JArray CreateTable() => JArray.Parse("""
        [{"page":1,"x":54,"y":540,"width":504,"columns":[{"width":210},{"width":144},{"width":150}],"tag_structure_type":"Table","style":{"padding":{"top":6,"right":8,"bottom":6,"left":8},"text_size":10},"header_rows":[{"cells":[{"text":"Milestone","tag_structure_type":"TH","style":{"background_color_rgb":[26,72,112],"text_color_rgb":[255,255,255]}},{"text":"Owner","tag_structure_type":"TH","style":{"background_color_rgb":[26,72,112],"text_color_rgb":[255,255,255]}},{"text":"Status","tag_structure_type":"TH","style":{"background_color_rgb":[26,72,112],"text_color_rgb":[255,255,255]}}]}],"rows":[{"cells":[{"text":"Requirements review"},{"text":"Maya Chen"},{"text":"Complete","tag_structure_type":"TD","style":{"background_color_rgb":[220,252,231]}}]},{"cells":[{"text":"Prototype delivery"},{"text":"Jordan Lee"},{"text":"In progress","tag_structure_type":"TD","style":{"background_color_rgb":[254,249,195]}}]},{"cells":[{"text":"Stakeholder approval"},{"text":"Avery Patel"},{"text":"Planned","tag_structure_type":"TD","style":{"background_color_rgb":[239,246,255]}}]}],"footer_rows":[{"cells":[{"text":"Next review: Friday, 10:00 AM","col_span":3,"tag_structure_type":"TD","style":{"background_color_rgb":[245,247,250],"text_color_rgb":[55,65,81]}}]}]}]
        """);
}
