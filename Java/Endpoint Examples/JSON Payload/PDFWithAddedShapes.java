import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import org.json.JSONArray;
import org.json.JSONObject;

public class PDFWithAddedShapes {

  // By default, we use the US-based API service. This is the primary endpoint for global use.
  private static final String API_URL = "https://api.pdfrest.com";

  // For GDPR compliance and enhanced performance for European users, you can switch to the EU-based
  // service by commenting out the URL above and uncommenting the URL below.
  // For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
  // private static final String API_URL = "https://eu-api.pdfrest.com";

  private static final String DEFAULT_FILE_PATH = "/path/to/input.pdf";
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  public static void main(String[] args) {
    File inputFile = new File(args.length > 0 ? args[0] : DEFAULT_FILE_PATH);
    Dotenv dotenv = Dotenv.configure().ignoreIfMalformed().ignoreIfMissing().load();
    String apiKey = dotenv.get("PDFREST_API_KEY", DEFAULT_API_KEY);
    OkHttpClient client = new OkHttpClient.Builder().readTimeout(60, TimeUnit.SECONDS).build();

    String inputId = uploadFile(client, inputFile, apiKey);
    if (inputId == null) {
      return;
    }

    JSONArray shapes =
        new JSONArray()
            .put(new JSONObject().put("type", "rectangle").put("page", 1).put("x", 54).put("y", 540)
                .put("width", 504).put("height", 108).put("fill_color_rgb", "245,247,250")
                .put("stroke_color_rgb", "26,72,112").put("stroke_width", 1).put("tag_is_artifact", true))
            .put(new JSONObject().put("type", "line").put("page", 1).put("x1", 72).put("y1", 576)
                .put("x2", 540).put("y2", 576).put("stroke_color_rgb", "26,72,112").put("stroke_width", 1.5)
                .put("tag_actual_text", "Review section divider").put("tag_structure_type", "Figure"));
    JSONObject payload =
        new JSONObject()
            .put("id", inputId)
            .put("shape_objects", shapes)
            .put("tag_enabled", true)
            .put("output", "review-panel");

    Request request =
        new Request.Builder()
            .header("Accept", "application/json")
            .header("Api-Key", apiKey)
            .url(API_URL + "/pdf-with-added-shapes")
            .post(RequestBody.create(payload.toString(), MediaType.parse("application/json")))
            .build();
    try (Response response = client.newCall(request).execute()) {
      System.out.println("Processing response status code: " + response.code());
      if (response.body() != null) {
        System.out.println(new JSONObject(response.body().string()).toString(2));
      }
    } catch (IOException error) {
      throw new RuntimeException(error);
    }
  }

  private static String uploadFile(OkHttpClient client, File inputFile, String apiKey) {
    Request request =
        new Request.Builder()
            .header("Api-Key", apiKey)
            .header("Content-Filename", inputFile.getName())
            .url(API_URL + "/upload")
            .post(RequestBody.create(inputFile, MediaType.parse("application/octet-stream")))
            .build();
    try (Response response = client.newCall(request).execute()) {
      if (response.body() == null) {
        return null;
      }
      JSONObject result = new JSONObject(response.body().string());
      if (!response.isSuccessful()) {
        System.out.println(result.toString(2));
        return null;
      }
      return result.getJSONArray("files").getJSONObject(0).getString("id");
    } catch (IOException error) {
      throw new RuntimeException(error);
    }
  }
}
