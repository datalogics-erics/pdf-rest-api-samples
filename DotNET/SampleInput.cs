using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Samples;

internal static class SampleInput
{
    internal static string RequireFile(string[] args, string command)
    {
        if (args.Length > 0 && File.Exists(args[0]))
        {
            return args[0];
        }

        Console.Error.WriteLine($"{command} requires an existing <inputFile>");
        Environment.Exit(1);
        return string.Empty;
    }

    internal static string RequireApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("PDFREST_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }

        Console.Error.WriteLine("Missing required environment variable: PDFREST_API_KEY");
        Environment.Exit(1);
        return string.Empty;
    }

    internal static HttpClient CreateClient() => new()
    {
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("PDFREST_URL") ?? "https://api.pdfrest.com"),
    };

    internal static HttpRequestMessage JsonRequest(string path, JObject payload, string apiKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        request.Headers.Accept.Add(new("application/json"));
        request.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
        return request;
    }

    internal static async Task<string> UploadPdf(HttpClient client, string inputPath, string apiKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "upload");
        request.Headers.TryAddWithoutValidation("Api-Key", apiKey);
        request.Headers.TryAddWithoutValidation("Content-Filename", Path.GetFileName(inputPath));
        request.Content = new ByteArrayContent(await File.ReadAllBytesAsync(inputPath));
        request.Content.Headers.ContentType = new("application/octet-stream");
        using var response = await client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine(responseText);
            Environment.Exit(1);
        }

        var inputId = JObject.Parse(responseText)["files"]?[0]?["id"]?.Value<string>();
        if (!string.IsNullOrWhiteSpace(inputId))
        {
            return inputId;
        }

        Console.Error.WriteLine("The PDF upload did not return a resource ID.");
        Environment.Exit(1);
        return string.Empty;
    }

    internal static async Task PrintResponse(HttpClient client, HttpRequestMessage request)
    {
        using var response = await client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response status code: {(int)response.StatusCode}");
        Console.WriteLine(responseText);
        if (!response.IsSuccessStatusCode)
        {
            Environment.Exit(1);
        }
    }
}
