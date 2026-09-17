import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import org.json.JSONObject;

// Upload invoice XML, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
public class ZugferdPdf {
  private static final String API_URL = "https://api.pdfrest.com";
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  public static void main(String[] args) throws IOException {
    File invoiceXml = new File(args.length > 0 ? args[0] : "/path/to/invoice.xml");
    String apiKey =
        Dotenv.configure()
            .ignoreIfMalformed()
            .ignoreIfMissing()
            .load()
            .get("PDFREST_API_KEY", DEFAULT_API_KEY);
    String xmlId = upload(invoiceXml, apiKey, "application/xml");
    JSONObject options =
        new JSONObject()
            .put("locale", "de-DE")
            .put("label_language", "de")
            .put("font", "arial")
            .put("bold_font", "arialbold")
            .put("accent_color_rgb", new int[] {0, 92, 171});
    JSONObject payload =
        new JSONObject()
            .put("id", xmlId)
            .put("output", "zugferd_invoice")
            .put("render_options", options);
    send(
        new Request.Builder()
            .url(API_URL + "/zugferd-pdf")
            .header("Api-Key", apiKey)
            .post(RequestBody.create(payload.toString(), MediaType.parse("application/json")))
            .build());
  }

  private static String upload(File file, String apiKey, String contentType) throws IOException {
    Request request =
        new Request.Builder()
            .url(API_URL + "/upload")
            .header("Api-Key", apiKey)
            .header("Content-Filename", file.getName())
            .post(RequestBody.create(file, MediaType.parse(contentType)))
            .build();
    try (Response response = new OkHttpClient().newCall(request).execute()) {
      String body = response.body() == null ? "{}" : response.body().string();
      if (!response.isSuccessful()) {
        throw new IOException(body);
      }
      return new JSONObject(body).getJSONArray("files").getJSONObject(0).getString("id");
    }
  }

  private static void send(Request request) throws IOException {
    try (Response response = new OkHttpClient().newCall(request).execute()) {
      String body = response.body() == null ? "" : response.body().string();
      System.out.println(body);
      if (!response.isSuccessful()) {
        System.exit(1);
      }
    }
  }
}
