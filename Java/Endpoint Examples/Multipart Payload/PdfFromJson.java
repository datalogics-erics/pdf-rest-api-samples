import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.*;
import org.json.JSONArray;
import org.json.JSONObject;

public class PdfFromJson {
  private static final String API_URL = System.getenv().getOrDefault("PDFREST_URL", "https://api.pdfrest.com");
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  public static void main(String[] args) throws IOException {
    File inputFile = new File(args.length > 0 ? args[0] : "/path/to/sample.json");
    Dotenv dotenv = Dotenv.configure().ignoreIfMalformed().ignoreIfMissing().load();
    String apiKey = dotenv.get("PDFREST_API_KEY", System.getenv().getOrDefault("PDFREST_API_KEY", DEFAULT_API_KEY));
    JSONObject options = new JSONObject("{\"title\":\"Structured Content Sample\",\"language\":\"en-US\",\"enable_tagging\":true,\"page_setup\":{\"size\":\"Letter\",\"orientation\":\"portrait\",\"margin\":{\"top\":36,\"right\":42,\"bottom\":36,\"left\":42}},\"style\":{\"font\":\"Arial\",\"heading_font\":\"Arial\",\"code_font\":\"Courier\",\"text_size\":11,\"text_color_rgb\":[34,34,34],\"heading_scale\":1.35,\"table\":{\"column_width_weights\":[2,3,2],\"keep_header_with_first_row\":true,\"repeat_headers_on_overflow\":true,\"show_borders\":true,\"border_width\":0.75,\"border_color_rgb\":[180,188,200],\"header_fill_color_rgb\":[33,64,98],\"header_text_color_rgb\":[255,255,255],\"row_fill_color_rgb\":[250,250,252],\"alternate_row_fill_color_rgb\":[235,240,246],\"cell_padding\":{\"top\":6,\"right\":8,\"bottom\":6,\"left\":8}}},\"data_presentation\":\"hierarchy\"}");
    MultipartBody.Builder form = new MultipartBody.Builder().setType(MultipartBody.FORM)
        .addFormDataPart("file", inputFile.getName(), RequestBody.create(inputFile, MediaType.parse("application/octet-stream")))
        .addFormDataPart("structured_text_options", options.toString());
    Request request = new Request.Builder().url(API_URL + "/pdf").header("Api-Key", apiKey).post(form.build()).build();
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

