import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.*;
import org.json.JSONObject;

public class Postscript {
  // By default, we use the US-based API service. This is the primary endpoint for global use.
  private static final String API_URL = "https://api.pdfrest.com";

  // For GDPR compliance and enhanced performance for European users, replace the URL above
  // with https://eu-api.pdfrest.com. For more information, visit
  // https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work.
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  /* This sample uploads a PDF, then converts it through the JSON /postscript flow.
   * Pairing this endpoint with /pdf creates a PDF -> PostScript -> PDF workflow commonly
   * called PDF refrying. Some print and prepress workflows use it to rebuild, flatten,
   * or normalize page content, but the lossy roundtrip can discard PDF-specific features.
   * These settings request Level 3, text-safe output, all pages at original scale,
   * shrink-to-fit without rotation, and printable annotations.
   */
  public static void main(String[] args) throws IOException {
    File inputFile = new File(args.length > 0 ? args[0] : "/path/to/sample.pdf");
    Dotenv dotenv = Dotenv.configure().ignoreIfMalformed().ignoreIfMissing().load();
    String apiKey = dotenv.get("PDFREST_API_KEY", DEFAULT_API_KEY);
    String inputId = upload(inputFile, apiKey);
    JSONObject payload =
        new JSONObject()
            .put("id", inputId)
            .put("ps_level", 3)
            .put("page_range", "all")
            .put("binary_output", false)
            .put("scale", 1)
            .put("rotate", false)
            .put("shrink_to_fit", true)
            .put("print_annotations", true)
            .put("output", "postscript_from_pdf");
    Request request =
        new Request.Builder()
            .url(API_URL + "/postscript")
            .header("Api-Key", apiKey)
            .post(RequestBody.create(payload.toString(), MediaType.parse("application/json")))
            .build();
    send(request);
  }

  private static void send(Request request) throws IOException {
    OkHttpClient client = new OkHttpClient.Builder().readTimeout(60, TimeUnit.SECONDS).build();
    try (Response response = client.newCall(request).execute()) {
      System.out.println("Result code " + response.code());
      if (response.body() != null) System.out.println(response.body().string());
      if (!response.isSuccessful()) System.exit(1);
    }
  }

  private static String upload(File file, String apiKey) throws IOException {
    Request request =
        new Request.Builder()
            .url(API_URL + "/upload")
            .header("Api-Key", apiKey)
            .header("Content-Filename", file.getName())
            .post(RequestBody.create(file, MediaType.parse("application/octet-stream")))
            .build();
    OkHttpClient client = new OkHttpClient();
    try (Response response = client.newCall(request).execute()) {
      String body = response.body() == null ? "{}" : response.body().string();
      if (!response.isSuccessful()) throw new IOException(body);
      return new JSONObject(body).getJSONArray("files").getJSONObject(0).getString("id");
    }
  }
}
