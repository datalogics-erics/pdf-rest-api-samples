/*
 * Generate a tagged invoice from JSON and CSV input using pdfRest.
 *
 * Set PDFREST_API_KEY before running this sample. It reads metadata.json,
 * style.json, and line-items.csv from invoice-data, then downloads the PDF
 * beside this source file. From the DotNET directory, run:
 * dotnet run -- create-invoice-from-structured-data
 */

using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace Samples.ComplexFlowExamples;

public static class CreateInvoiceFromStructuredData
{
    private static readonly string DataDirectory = Path.Combine("Complex Flow Examples", "invoice-data");
    private static readonly string OutputPath = Path.Combine("Complex Flow Examples", "invoice-from-structured-data.pdf");

    public static async Task Execute(string[] args)
    {
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) { Console.Error.WriteLine("Missing required environment variable: PDFREST_API_KEY"); return; }
        var baseUrl = (Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com").TrimEnd('/');
        using var client = new HttpClient(new HttpClientHandler { UseCookies = false }) { BaseAddress = new Uri(baseUrl) };
        var metadata = JObject.Parse(await File.ReadAllTextAsync(Path.Combine(DataDirectory, "metadata.json")));
        var style = JObject.Parse(await File.ReadAllTextAsync(Path.Combine(DataDirectory, "style.json")));
        var items = ReadCsv(Path.Combine(DataDirectory, "line-items.csv"));

        var id = (string)(await PostJson(client, apiKey, "blank-pdf", new JObject { ["page_size"] = "letter", ["page_count"] = 1, ["page_orientation"] = "portrait" }))!["outputId"]!;
        id = (string)(await PostMultipart(client, apiKey, "pdf-with-added-shapes", new Dictionary<string, object?> { ["id"] = id, ["shape_objects"] = Shapes(style, 1), ["tag_enabled"] = "true" }))!["outputId"]!;
        id = (string)(await PostMultipart(client, apiKey, "pdf-with-added-text", new Dictionary<string, object?> { ["id"] = id, ["text_objects"] = TextObjects(metadata, style), ["tag_enabled"] = "true", ["tag_language"] = "en-US" }))!["outputId"]!;
        id = (string)(await PostMultipart(client, apiKey, "pdf-with-added-tables", new Dictionary<string, object?> { ["id"] = id, ["table_objects"] = Tables(metadata, style, items), ["tag_enabled"] = "true", ["tag_language"] = "en-US" }))!["outputId"]!;
        var imagePath = Path.Combine(DataDirectory, "northstar-logo.png");
        var logoId = await UploadImage(client, apiKey, imagePath);
        id = (string)(await PostMultipart(client, apiKey, "pdf-with-added-image", new Dictionary<string, object?> { ["id"] = id, ["image_id"] = logoId, ["page"] = 1, ["x"] = 54, ["y"] = 716, ["width"] = 200, ["tag_alt_text"] = "Northstar Sample Supply logo", ["tag_structure_type"] = "Figure", ["tag_enabled"] = "true", ["tag_language"] = "en-US" }))!["outputId"]!;
        var info = await PostMultipart(client, apiKey, "pdf-info", new Dictionary<string, object?> { ["id"] = id, ["queries"] = "page_count" });
        var pageCount = info!["page_count"]!.Value<int>();
        id = (string)(await PostMultipart(client, apiKey, "pdf-with-added-shapes", new Dictionary<string, object?> { ["id"] = id, ["shape_objects"] = Shapes(style, pageCount, true), ["tag_enabled"] = "true" }))!["outputId"]!;
        id = (string)(await PostMultipart(client, apiKey, "pdf-with-added-text", new Dictionary<string, object?> { ["id"] = id, ["text_objects"] = Footer(metadata, style, pageCount), ["tag_enabled"] = "true", ["tag_language"] = "en-US", ["output"] = "invoice_from_structured_data" }))!["outputId"]!;
        var output = await client.GetByteArrayAsync($"resource/{id}?format=file");
        await File.WriteAllBytesAsync(OutputPath, output);
        Console.WriteLine($"Created {Path.GetFullPath(OutputPath)}");
    }

    private static async Task<string> UploadImage(HttpClient client, string apiKey, string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "upload");
        request.Headers.Add("Api-Key", apiKey);
        request.Headers.Add("Content-Filename", Path.GetFileName(path));
        request.Content = new ByteArrayContent(await File.ReadAllBytesAsync(path));
        request.Content.Headers.ContentType = new("application/octet-stream");
        var result = await Send(client, request, "upload");
        return (string)result["files"]![0]!["id"]!;
    }

    private static async Task<JObject> PostJson(HttpClient client, string key, string endpoint, JObject payload)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint); request.Headers.Add("Api-Key", key); request.Content = new StringContent(payload.ToString(Newtonsoft.Json.Formatting.None), Encoding.UTF8, "application/json");
        return await Send(client, request, endpoint);
    }

    private static async Task<JObject> PostMultipart(HttpClient client, string key, string endpoint, Dictionary<string, object?> fields)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint); request.Headers.Add("Api-Key", key); using var form = new MultipartFormDataContent();
        foreach (var field in fields) {
            if (field.Value is string path && field.Key == "image_files") form.Add(new StreamContent(File.OpenRead(path)), field.Key, Path.GetFileName(path));
            else form.Add(new StringContent(field.Value is JToken token ? token.ToString(Newtonsoft.Json.Formatting.None) : field.Value?.ToString() ?? ""), field.Key);
        }
        request.Content = form; return await Send(client, request, endpoint);
    }

    private static async Task<JObject> Send(HttpClient client, HttpRequestMessage request, string endpoint)
    {
        using var response = await client.SendAsync(request); var text = await response.Content.ReadAsStringAsync(); Console.WriteLine($"{endpoint}: {(int)response.StatusCode}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"{endpoint} failed: {text}"); return JObject.Parse(text);
    }

    private static List<Dictionary<string, string>> ReadCsv(string path)
    {
        var lines = File.ReadAllLines(path); var headers = ParseCsvLine(lines[0]); var rows = new List<Dictionary<string, string>>();
        foreach (var line in lines.Skip(1).Where(line => !string.IsNullOrWhiteSpace(line))) { var values = ParseCsvLine(line); rows.Add(headers.Select((header, index) => new { header, value = values[index] }).ToDictionary(x => x.header, x => x.value)); }
        return rows;
    }

    private static List<string> ParseCsvLine(string line)
    { var fields = new List<string>(); var field = new StringBuilder(); var quoted = false; foreach (var character in line) { if (character == '"') quoted = !quoted; else if (character == ',' && !quoted) { fields.Add(field.ToString()); field.Clear(); } else field.Append(character); } fields.Add(field.ToString()); return fields; }
    private static string Rgb(JToken token) => string.Join(',', token.Values<int>());
    private static string Money(double value) => value.ToString("C2", CultureInfo.GetCultureInfo("en-US"));

    private static JArray TextObjects(JObject metadata, JObject style)
    {
        var objects = new JArray(); var seller = metadata["seller"]!; var customer = metadata["customer"]!; var primary = Rgb(style["primaryColorRgb"]!); var muted = Rgb(style["mutedTextColorRgb"]!);
        void Add(int x, int y, string text, double size, string color, int width, string structure = "P", bool bold = false) => objects.Add(new JObject { ["font"] = bold ? style["boldFont"] : style["bodyFont"], ["max_width"] = width, ["opacity"] = "1", ["page"] = "1", ["rotation"] = "0", ["text"] = text, ["text_color_rgb"] = color, ["text_size"] = size, ["x"] = x, ["y"] = y, ["tag_structure_type"] = structure });
        Add(405, 724, "INVOICE", 22, primary, 153, "H1", true); Add(405, 696, $"Invoice {metadata["invoiceNumber"]}", 9, muted, 153, "P", true); Add(405, 682, $"Issued {metadata["issueDate"]}", 9, muted, 153); Add(405, 668, $"Due {metadata["dueDate"]}", 9, muted, 153);
        Add(66, 622, "FROM", 8, primary, 216, "H2", true); Add(66, 606, (string)seller["name"]!, 9, "34,34,34", 216, "P", true); Add(66, 592, (string)seller["taxId"]!, 8, muted, 216); Add(66, 578, (string)seller["addressLine1"]!, 8, muted, 216); Add(66, 566, $"{seller["city"]}, {seller["region"]} {seller["postalCode"]}", 8, muted, 216);
        Add(330, 622, "BILL TO", 8, primary, 216, "H2", true); Add(330, 606, (string)customer["name"]!, 9, "34,34,34", 216, "P", true); Add(330, 592, (string)customer["addressLine1"]!, 8, muted, 216); Add(330, 578, $"{customer["city"]}, {customer["region"]} {customer["postalCode"]}", 8, muted, 216); return objects;
    }

    private static JArray Tables(JObject metadata, JObject style, List<Dictionary<string, string>> items)
    {
        var subtotal = items.Sum(item => double.Parse(item["quantity"]) * double.Parse(item["unitPrice"])); var tax = Math.Round(subtotal * (double)metadata["taxRate"]!, 2); var total = subtotal + tax; var primary = style["primaryColorRgb"]!;
        var headers = new JArray("Description", "Qty", "Unit Price", "Amount"); var header = new JArray(headers.Select((text, index) => new JObject { ["text"] = text, ["tag_structure_type"] = "TH", ["style"] = new JObject { ["background_color_rgb"] = primary, ["text_color_rgb"] = new JArray(255, 255, 255), ["text_align"] = index > 0 ? "right" : "left" } }));
        var rows = new JArray();
        foreach (var item in items)
        {
            rows.Add(new JObject
            {
                ["cells"] = new JArray(
                    new JObject { ["text"] = item["description"] },
                    new JObject { ["text"] = item["quantity"], ["style"] = new JObject { ["text_align"] = "right" } },
                    new JObject { ["text"] = Money(double.Parse(item["unitPrice"])), ["style"] = new JObject { ["text_align"] = "right" } },
                    new JObject { ["text"] = Money(double.Parse(item["quantity"]) * double.Parse(item["unitPrice"])), ["style"] = new JObject { ["text_align"] = "right" } })
            });
        }
        JObject Summary(string label, double value, bool highlight = false) { var cellStyle = new JObject { ["text_align"] = "right" }; if (highlight) { cellStyle["background_color_rgb"] = style["accentColorRgb"]; cellStyle["text_size"] = 10; } return new JObject { ["cells"] = new JArray(new JObject { ["text"] = label, ["col_span"] = 3, ["style"] = cellStyle }, new JObject { ["text"] = Money(value), ["style"] = cellStyle }) }; }
        var tableStyle = new JObject { ["padding"] = new JObject { ["top"] = 5, ["right"] = 6, ["bottom"] = 5, ["left"] = 6 }, ["text_size"] = style["tableHeaderFontSize"], ["text_color_rgb"] = style["textColorRgb"] };
        var columns = new JArray(new JObject { ["width"] = 276 }, new JObject { ["width"] = 54 }, new JObject { ["width"] = 84 }, new JObject { ["width"] = 90 });
        var footer = new JArray(Summary("Subtotal", subtotal), Summary($"Tax {(double)metadata["taxRate"]! * 100:0.00}%", tax), Summary("Total", total, true));
        var table = new JObject { ["page"] = 1, ["x"] = 54, ["y"] = 510, ["width"] = 504, ["columns"] = columns, ["continuation_page_top_margin"] = 85, ["page_bottom_margin"] = 96, ["final_page_bottom_margin"] = 164, ["overflow_behavior"] = "split-row", ["row_split_behavior"] = "prefer-next-page", ["repeat_header_on_overflow"] = true, ["show_footer_on_last_page"] = true, ["tag_structure_type"] = "Table", ["style"] = tableStyle, ["header_rows"] = new JArray(new JObject { ["cells"] = header }), ["rows"] = rows, ["footer_rows"] = footer };
        return new JArray(table);
    }

    private static JArray Shapes(JObject style, int page, bool footer = false) { var border = Rgb(style["borderColorRgb"]!); if (footer) return new JArray(new JObject { ["type"] = "rectangle", ["page"] = page, ["x"] = 54, ["y"] = 70, ["width"] = 504, ["height"] = 104, ["fill_color_rgb"] = "248,250,251", ["stroke_color_rgb"] = border, ["stroke_width"] = 0.5, ["tag_is_artifact"] = true }); return new JArray(new JObject { ["type"] = "rectangle", ["page"] = 1, ["x"] = 54, ["y"] = 540, ["width"] = 240, ["height"] = 96, ["fill_color_rgb"] = Rgb(style["accentColorRgb"]!), ["stroke_color_rgb"] = border, ["stroke_width"] = 0.5, ["tag_is_artifact"] = true }, new JObject { ["type"] = "rectangle", ["page"] = 1, ["x"] = 318, ["y"] = 540, ["width"] = 240, ["height"] = 96, ["fill_color_rgb"] = Rgb(style["accentColorRgb"]!), ["stroke_color_rgb"] = border, ["stroke_width"] = 0.5, ["tag_is_artifact"] = true }); }

    private static JArray Footer(JObject metadata, JObject style, int pageCount) { var footer = new JArray(); var muted = Rgb(style["mutedTextColorRgb"]!); void Add(string text, int page, int x, int y, double size, int width, string structure = "P", bool bold = false) => footer.Add(new JObject { ["font"] = bold ? style["boldFont"] : style["bodyFont"], ["max_width"] = width, ["opacity"] = "1", ["page"] = page.ToString(), ["rotation"] = "0", ["text"] = text, ["text_color_rgb"] = muted, ["text_size"] = size, ["x"] = x, ["y"] = y, ["tag_structure_type"] = structure }); Add("Payment terms", pageCount, 66, 156, 8, 480, "H2", true); Add((string)metadata["paymentTerms"]!, pageCount, 66, 142, 7.5, 480); Add("Notes", pageCount, 66, 112, 8, 480, "H2", true); Add((string)metadata["notes"]!, pageCount, 66, 98, 7.5, 480); for (int page = 1; page <= pageCount; page++) { Add("Generated from structured JSON and CSV input with pdfRest.", page, 54, 54, 7.5, 400); Add($"Page {page} of {pageCount}", page, 490, 54, 7.5, 68); } return footer; }
}
