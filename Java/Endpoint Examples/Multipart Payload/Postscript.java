import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.*;

public class Postscript {
  // By default, we use the US-based API service. This is the primary endpoint for global use.
  private static final String API_URL = "https://api.pdfrest.com";

  // For GDPR compliance and enhanced performance for European users, replace the URL above
  // with https://eu-api.pdfrest.com. For more information, visit
  // https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work.
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  /* This sample converts PDF to PostScript through /postscript. Pairing this endpoint
   * with /pdf creates a PDF -> PostScript -> PDF workflow commonly called PDF refrying.
   * Some print and prepress workflows use it to rebuild, flatten, or normalize page
   * content, but the lossy roundtrip can discard PDF-specific features. These settings
   * request Level 3, text-safe output, all pages at original scale, shrink-to-fit without
   * rotation, and printable annotations.
   */
  public static void main(String[] args) throws IOException {
    File inputFile = new File(args.length > 0 ? args[0] : "/path/to/sample.pdf");
    Dotenv dotenv = Dotenv.configure().ignoreIfMalformed().ignoreIfMissing().load();
    String apiKey = dotenv.get("PDFREST_API_KEY", DEFAULT_API_KEY);
    MultipartBody.Builder form =
        new MultipartBody.Builder()
            .setType(MultipartBody.FORM)
            .addFormDataPart(
                "file",
                inputFile.getName(),
                RequestBody.create(inputFile, MediaType.parse("application/pdf")))
            .addFormDataPart("ps_level", "3")
            .addFormDataPart("page_range", "all")
            .addFormDataPart("binary_output", "false")
            .addFormDataPart("scale", "1")
            .addFormDataPart("rotate", "false")
            .addFormDataPart("shrink_to_fit", "true")
            .addFormDataPart("print_annotations", "true")
            .addFormDataPart("output", "postscript_from_pdf");
    Request request =
        new Request.Builder()
            .url(API_URL + "/postscript")
            .header("Api-Key", apiKey)
            .post(form.build())
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
}
