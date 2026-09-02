import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.concurrent.TimeUnit;
import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import org.json.JSONObject;

/*
 * Refry a PDF by converting it to PostScript and back to PDF.
 *
 * PDF refrying is the common name for a PDF -> PostScript -> PDF roundtrip.
 * Some print, prepress, and legacy production workflows use it to rebuild,
 * flatten, or normalize page content for downstream systems. The process is
 * intentionally lossy and may remove tags, forms, layers, annotations,
 * transparency, metadata, and editability. Use this workflow when a downstream
 * system requires rebuilt page content or a PostScript-based interchange file.
 *
 * The PostScript-to-PDF step uses a custom .joboptions profile in this sample.
 * A .joboptions file contains Adobe Distiller-compatible conversion settings;
 * it is optional, and default settings are used when omitted. pdfRest applies
 * the profile with Datalogics PDF Converter SDK. Datalogics maintains the SDK
 * in partnership with Adobe, using the same Adobe technology that powers
 * Distiller.
 *
 * Run: java RefryPdf <pdf> <jobOptions> [outputPdf]
 */
public class RefryPdf {
  private static final String DEFAULT_API_URL = "https://api.pdfrest.com";
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
  private static final OkHttpClient CLIENT =
      new OkHttpClient.Builder().readTimeout(120, TimeUnit.SECONDS).build();

  public static void main(String[] args) throws IOException {
    File inputFile = new File(args.length > 0 ? args[0] : "/path/to/sample.pdf");
    File jobOptionsFile = new File(args.length > 1 ? args[1] : "/path/to/custom.joboptions");
    Path outputPath = args.length > 2 ? Path.of(args[2]) : Path.of("refried.pdf");

    Dotenv dotenv = Dotenv.configure().ignoreIfMalformed().ignoreIfMissing().load();
    String apiUrl = dotenv.get("PDFREST_URL", DEFAULT_API_URL).replaceAll("/$", "");
    String apiKey = dotenv.get("PDFREST_API_KEY", DEFAULT_API_KEY);

    JSONObject postscript =
        postMultipart(
            apiUrl,
            apiKey,
            "postscript",
            new MultipartBody.Builder()
                .setType(MultipartBody.FORM)
                .addFormDataPart(
                    "file",
                    inputFile.getName(),
                    RequestBody.create(inputFile, MediaType.parse("application/pdf")))
                .addFormDataPart("ps_level", "3")
                .addFormDataPart("page_range", "all")
                .addFormDataPart("binary_output", "true")
                .addFormDataPart("scale", "1")
                .addFormDataPart("rotate", "false")
                .addFormDataPart("shrink_to_fit", "true")
                .addFormDataPart("print_annotations", "true")
                .addFormDataPart("output", "refry_intermediate")
                .build());

    JSONObject finalPdf =
        postMultipart(
            apiUrl,
            apiKey,
            "pdf",
            new MultipartBody.Builder()
                .setType(MultipartBody.FORM)
                .addFormDataPart("id", postscript.getString("outputId"))
                .addFormDataPart(
                    "job_options",
                    jobOptionsFile.getName(),
                    RequestBody.create(jobOptionsFile, MediaType.parse("application/octet-stream")))
                .addFormDataPart("output", "refried")
                .build());

    Request downloadRequest =
        new Request.Builder()
            .url(apiUrl + "/resource/" + finalPdf.getString("outputId") + "?format=file")
            .header("Api-Key", apiKey)
            .build();
    try (Response response = CLIENT.newCall(downloadRequest).execute()) {
      if (!response.isSuccessful() || response.body() == null) {
        throw new IOException("resource download failed: " + response.code());
      }
      Files.write(outputPath, response.body().bytes());
    }

    System.out.println(finalPdf.toString(2));
    System.out.println("Created " + outputPath.toAbsolutePath());
  }

  private static JSONObject postMultipart(
      String apiUrl, String apiKey, String endpoint, RequestBody body) throws IOException {
    Request request =
        new Request.Builder()
            .url(apiUrl + "/" + endpoint)
            .header("Api-Key", apiKey)
            .header("Accept", "application/json")
            .post(body)
            .build();
    try (Response response = CLIENT.newCall(request).execute()) {
      String text = response.body() == null ? "" : response.body().string();
      System.out.println(endpoint + ": " + response.code());
      if (!response.isSuccessful()) {
        throw new IOException(endpoint + " failed: " + text);
      }
      return new JSONObject(text);
    }
  }
}
