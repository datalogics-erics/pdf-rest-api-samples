/*
 * What this sample does:
 * - Adds a review-panel background and divider line to a PDF via multipart/form-data.
 * - Routed from Program.cs as: `dotnet run -- pdf-with-added-shapes-multipart <inputFile>`.
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

public static class PdfWithAddedShapes
{
    public static async Task Execute(string[] args)
    {
        var inputPath = SampleInput.RequireFile(args, "pdf-with-added-shapes-multipart");
        var apiKey = SampleInput.RequireApiKey();
        var shapes = JArray.Parse("""
            [
              {"type":"rectangle","page":1,"x":54,"y":540,"width":504,"height":108,"fill_color_rgb":"245,247,250","stroke_color_rgb":"26,72,112","stroke_width":1,"tag_is_artifact":true},
              {"type":"line","page":1,"x1":72,"y1":576,"x2":540,"y2":576,"stroke_color_rgb":"26,72,112","stroke_width":1.5,"tag_actual_text":"Review section divider","tag_structure_type":"Figure"}
            ]
            """);

        using var client = SampleInput.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "pdf-with-added-shapes");
        request.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        request.Headers.Accept.Add(new("application/json"));
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(await File.ReadAllBytesAsync(inputPath)), "file", Path.GetFileName(inputPath));
        content.Add(new StringContent(shapes.ToString(), Encoding.UTF8), "shape_objects");
        content.Add(new StringContent("true"), "tag_enabled");
        content.Add(new StringContent("review-panel"), "output");
        request.Content = content;

        await SampleInput.PrintResponse(client, request);
    }
}
