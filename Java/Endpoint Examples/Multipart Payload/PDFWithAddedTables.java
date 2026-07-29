import io.github.cdimascio.dotenv.Dotenv;
import java.io.File;
import java.io.IOException;
import java.util.concurrent.TimeUnit;
import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import org.json.JSONArray;
import org.json.JSONObject;

public class PDFWithAddedTables {

  // By default, we use the US-based API service. This is the primary endpoint for global use.
  private static final String API_URL = "https://api.pdfrest.com";

  // For GDPR compliance and enhanced performance for European users, you can switch to the EU-based
  // service by commenting out the URL above and uncommenting the URL below.
  // For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
  // private static final String API_URL = "https://eu-api.pdfrest.com";

  private static final String DEFAULT_FILE_PATH = "/path/to/input.pdf";
  private static final String DEFAULT_API_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  // This tagged table uses fixed widths that add up to the table width: 210 + 144 + 150 = 504.
  private static final String TABLE_OBJECTS =
      """
      [
        {
          "page": 1,
          "x": 54,
          "y": 540,
          "width": 504,
          "columns": [{"width": 210}, {"width": 144}, {"width": 150}],
          "tag_structure_type": "Table",
          "style": {
            "padding": {"top": 6, "right": 8, "bottom": 6, "left": 8},
            "text_size": 10
          },
          "header_rows": [
            {
              "cells": [
                {"text": "Milestone", "tag_structure_type": "TH", "style": {"background_color_rgb": [26, 72, 112], "text_color_rgb": [255, 255, 255]}},
                {"text": "Owner", "tag_structure_type": "TH", "style": {"background_color_rgb": [26, 72, 112], "text_color_rgb": [255, 255, 255]}},
                {"text": "Status", "tag_structure_type": "TH", "style": {"background_color_rgb": [26, 72, 112], "text_color_rgb": [255, 255, 255]}}
              ]
            }
          ],
          "rows": [
            {"cells": [{"text": "Requirements review"}, {"text": "Maya Chen"}, {"text": "Complete", "tag_structure_type": "TD", "style": {"background_color_rgb": [220, 252, 231]}}]},
            {"cells": [{"text": "Prototype delivery"}, {"text": "Jordan Lee"}, {"text": "In progress", "tag_structure_type": "TD", "style": {"background_color_rgb": [254, 249, 195]}}]},
            {"cells": [{"text": "Stakeholder approval"}, {"text": "Avery Patel"}, {"text": "Planned", "tag_structure_type": "TD", "style": {"background_color_rgb": [239, 246, 255]}}]}
          ],
          "footer_rows": [
            {
              "cells": [
                {"text": "Next review: Friday, 10:00 AM", "col_span": 3, "tag_structure_type": "TD", "style": {"background_color_rgb": [245, 247, 250], "text_color_rgb": [55, 65, 81]}}
              ]
            }
          ]
        }
      ]
      """;

  public static void main(String[] args) {
    File inputFile = new File(args.length > 0 ? args[0] : DEFAULT_FILE_PATH);
    Dotenv dotenv = Dotenv.configure().ignoreIfMalformed().ignoreIfMissing().load();

    RequestBody fileBody = RequestBody.create(inputFile, MediaType.parse("application/pdf"));
    RequestBody requestBody =
        new MultipartBody.Builder()
            .setType(MultipartBody.FORM)
            .addFormDataPart("file", inputFile.getName(), fileBody)
            .addFormDataPart("table_objects", new JSONArray(TABLE_OBJECTS).toString())
            .addFormDataPart("tag_enabled", "true")
            .addFormDataPart("tag_language", "en-US")
            .addFormDataPart("output", "project-status")
            .build();
    Request request =
        new Request.Builder()
            .header("Accept", "application/json")
            .header("Api-Key", dotenv.get("PDFREST_API_KEY", DEFAULT_API_KEY))
            .url(API_URL + "/pdf-with-added-tables")
            .post(requestBody)
            .build();

    OkHttpClient client = new OkHttpClient.Builder().readTimeout(60, TimeUnit.SECONDS).build();
    try (Response response = client.newCall(request).execute()) {
      System.out.println("Response status code: " + response.code());
      if (response.body() != null) {
        System.out.println(new JSONObject(response.body().string()).toString(2));
      }
    } catch (IOException error) {
      throw new RuntimeException(error);
    }
  }
}
