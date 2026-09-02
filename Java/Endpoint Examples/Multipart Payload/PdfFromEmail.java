import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.*;

public class PdfFromEmail {
  // By default, we use the US-based API service. This is the primary endpoint for global use.
  private static final String API_URL = "https://api.pdfrest.com";

  // For GDPR compliance and enhanced performance for European users, replace the URL above
  // with https://eu-api.pdfrest.com. For more information, visit
  // https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work.
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  // Converts an Email (.eml) file to PDF by sending the file directly in a
  // multipart request.
  public static void main(String[] args) throws IOException {
    File inputFile = new File(args.length > 0 ? args[0] : "/path/to/sample.eml");
    Dotenv dotenv = Dotenv.configure().ignoreIfMalformed().ignoreIfMissing().load();
    String apiKey = dotenv.get("PDFREST_API_KEY", DEFAULT_API_KEY);
    MultipartBody.Builder form =
        new MultipartBody.Builder()
            .setType(MultipartBody.FORM)
            .addFormDataPart(
                "file",
                inputFile.getName(),
                RequestBody.create(inputFile, MediaType.parse("message/rfc822")))
            .addFormDataPart("output", "pdf_from_email");
    Request request =
        new Request.Builder()
            .url(API_URL + "/pdf")
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
