import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import okhttp3.*;
import org.json.JSONObject;

// Create a ZUGFeRD / Factur-X PDF/A-3 invoice from invoice XML.
public class ZugferdPdf {
  private static final String API_URL = "https://api.pdfrest.com";
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  public static void main(String[] args) throws IOException {
    File invoiceXml = new File(args.length > 0 ? args[0] : "/path/to/invoice.xml");
    String apiKey = Dotenv.configure().ignoreIfMalformed().ignoreIfMissing().load().get("PDFREST_API_KEY", DEFAULT_API_KEY);
    JSONObject options = new JSONObject().put("locale", "de-DE").put("label_language", "de").put("font", "arial").put("bold_font", "arialbold").put("accent_color_rgb", new int[] {0, 92, 171});
    MultipartBody body = new MultipartBody.Builder().setType(MultipartBody.FORM)
        .addFormDataPart("file", invoiceXml.getName(), RequestBody.create(invoiceXml, MediaType.parse("application/xml")))
        .addFormDataPart("render_options", options.toString()).addFormDataPart("output", "zugferd_invoice").build();
    send(new Request.Builder().url(API_URL + "/zugferd-pdf").header("Api-Key", apiKey).post(body).build());
  }

  private static void send(Request request) throws IOException {
    try (Response response = new OkHttpClient().newCall(request).execute()) {
      String body = response.body() == null ? "" : response.body().string(); System.out.println(body);
      if (!response.isSuccessful()) System.exit(1);
    }
  }
}
