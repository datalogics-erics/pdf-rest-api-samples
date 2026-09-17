import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;

// Validate a hybrid ZUGFeRD / Factur-X PDF without modifying it.
public class ValidatedZugferd {
  private static final String API_URL = "https://api.pdfrest.com";
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  public static void main(String[] args) throws IOException {
    File zugferdPdf = new File(args.length > 0 ? args[0] : "/path/to/zugferd-invoice.pdf");
    String apiKey =
        Dotenv.configure()
            .ignoreIfMalformed()
            .ignoreIfMissing()
            .load()
            .get("PDFREST_API_KEY", DEFAULT_API_KEY);
    MultipartBody body =
        new MultipartBody.Builder()
            .setType(MultipartBody.FORM)
            .addFormDataPart(
                "file",
                zugferdPdf.getName(),
                RequestBody.create(zugferdPdf, MediaType.parse("application/pdf")))
            .build();
    send(
        new Request.Builder()
            .url(API_URL + "/validated-zugferd")
            .header("Api-Key", apiKey)
            .post(body)
            .build());
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
