import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import org.json.JSONObject;

// Upload a hybrid PDF, then validate its ZUGFeRD / Factur-X package by resource ID.
public class ValidatedZugferd {
  private static final String API_URL = "https://api.pdfrest.com";
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
  private static final OkHttpClient CLIENT =
      new OkHttpClient.Builder().readTimeout(60, TimeUnit.SECONDS).build();

  public static void main(String[] args) throws IOException {
    File zugferdPdf = new File(args.length > 0 ? args[0] : "/path/to/zugferd-invoice.pdf");
    String apiKey =
        Dotenv.configure()
            .ignoreIfMalformed()
            .ignoreIfMissing()
            .load()
            .get("PDFREST_API_KEY", DEFAULT_API_KEY);
    String pdfId = upload(zugferdPdf, apiKey);
    JSONObject payload = new JSONObject().put("id", pdfId);
    send(
        new Request.Builder()
            .url(API_URL + "/validated-zugferd")
            .header("Api-Key", apiKey)
            .post(RequestBody.create(payload.toString(), MediaType.parse("application/json")))
            .build());
  }

  private static String upload(File file, String apiKey) throws IOException {
    Request request =
        new Request.Builder()
            .url(API_URL + "/upload")
            .header("Api-Key", apiKey)
            .header("Content-Filename", file.getName())
            .post(RequestBody.create(file, MediaType.parse("application/pdf")))
            .build();
    try (Response response = CLIENT.newCall(request).execute()) {
      String body = response.body() == null ? "{}" : response.body().string();
      if (!response.isSuccessful()) {
        throw new IOException(body);
      }
      return new JSONObject(body).getJSONArray("files").getJSONObject(0).getString("id");
    }
  }

  private static void send(Request request) throws IOException {
    try (Response response = CLIENT.newCall(request).execute()) {
      String body = response.body() == null ? "" : response.body().string();
      System.out.println(body);
      if (!response.isSuccessful()) {
        System.exit(1);
      }
    }
  }
}
