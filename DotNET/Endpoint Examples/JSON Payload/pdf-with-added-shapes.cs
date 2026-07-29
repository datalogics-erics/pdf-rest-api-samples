/*
 * What this sample does:
 * - Uploads a PDF, then adds a review-panel background and divider line by resource ID.
 * - Routed from Program.cs as: `dotnet run -- pdf-with-added-shapes <inputFile>`.
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

public static class PdfWithAddedShapes
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "pdf-with-added-shapes");
        var apiKey = SampleInput.RequireApiKey();
        using var client = SampleInput.CreateClient();
        var inputId = await SampleInput.UploadPdf(client, inputPath, apiKey);
        var shapes = JArray.Parse("""
            [
              {"type":"rectangle","page":1,"x":54,"y":540,"width":504,"height":108,"fill_color_rgb":"245,247,250","stroke_color_rgb":"26,72,112","stroke_width":1,"tag_is_artifact":true},
              {"type":"line","page":1,"x1":72,"y1":576,"x2":540,"y2":576,"stroke_color_rgb":"26,72,112","stroke_width":1.5,"tag_actual_text":"Review section divider","tag_structure_type":"Figure"}
            ]
            """);
        var payload = new JObject
        {
            ["id"] = inputId,
            ["shape_objects"] = shapes,
            ["tag_enabled"] = true,
            ["output"] = "review-panel",
        };

        using var request = SampleInput.JsonRequest("pdf-with-added-shapes", payload, apiKey);
        await SampleInput.PrintResponse(client, request);
    }
}
