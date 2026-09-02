import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.*;

public class PdfFromPostscript {
  // By default, we use the US-based API service. This is the primary endpoint for global use.
  private static final String API_URL = "https://api.pdfrest.com";

  // For GDPR compliance and enhanced performance for European users, replace the URL above
  // with https://eu-api.pdfrest.com. For more information, visit
  // https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work.
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  /* This sample converts PostScript to PDF with a custom .joboptions profile.
   * A .joboptions file contains Adobe Distiller-compatible conversion settings. The
   * profile is optional; omit job_options to use default settings. pdfRest applies a
   * supplied profile with Datalogics PDF Converter SDK. Datalogics maintains the SDK
   * in partnership with Adobe, using the same Adobe technology that powers Distiller.
   * Pair with /postscript for PDF
   * refrying: PDF -> PostScript -> PDF. Some print and prepress workflows use this lossy
   * roundtrip to rebuild or normalize page content.
   */
  public static void main(String[] args) throws IOException {
    File inputFile = new File(args.length > 0 ? args[0] : "/path/to/sample.ps");
    File jobOptionsFile = new File(args.length > 1 ? args[1] : "/path/to/custom.joboptions");
    Dotenv dotenv = Dotenv.configure().ignoreIfMalformed().ignoreIfMissing().load();
    String apiKey = dotenv.get("PDFREST_API_KEY", DEFAULT_API_KEY);
    MultipartBody.Builder form =
        new MultipartBody.Builder()
            .setType(MultipartBody.FORM)
            .addFormDataPart(
                "file",
                inputFile.getName(),
                RequestBody.create(inputFile, MediaType.parse("application/postscript")))
            .addFormDataPart(
                "job_options",
                jobOptionsFile.getName(),
                RequestBody.create(jobOptionsFile, MediaType.parse("application/octet-stream")))
            .addFormDataPart("output", "pdf_from_postscript");
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
