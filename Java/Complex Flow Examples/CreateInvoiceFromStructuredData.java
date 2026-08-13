/*
 * Generate a tagged invoice from JSON and CSV input using pdfRest.
 *
 * Set PDFREST_API_KEY in .env or the environment before running this sample.
 * It reads metadata.json, style.json, and line-items.csv from invoice-data,
 * then downloads the PDF beside this class. Run from this directory with:
 * mvn package && java -cp target/pdf-rest-api-samples-1.0-SNAPSHOT-jar-with-dependencies.jar CreateInvoiceFromStructuredData
 */

import io.github.cdimascio.dotenv.Dotenv;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.TimeUnit;
import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import org.json.JSONArray;
import org.json.JSONObject;

public class CreateInvoiceFromStructuredData {
  private static final Path DATA_DIR = Path.of("invoice-data");
  private static final Path OUTPUT = Path.of("invoice-from-structured-data.pdf");
  private static final String DEFAULT_URL = "https://api.pdfrest.com";
  private static final String DEFAULT_KEY = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

  private static final OkHttpClient CLIENT = new OkHttpClient.Builder()
      .readTimeout(120, TimeUnit.SECONDS).build();

  private static JSONObject postJson(String baseUrl, String key, String endpoint, JSONObject payload) throws IOException {
    RequestBody body = RequestBody.create(payload.toString(), MediaType.parse("application/json"));
    Request request = new Request.Builder().url(baseUrl + "/" + endpoint).header("Api-Key", key)
        .header("Accept", "application/json").post(body).build();
    try (Response response = CLIENT.newCall(request).execute()) { return result(response, endpoint); }
  }

  private static JSONObject postMultipart(String baseUrl, String key, String endpoint, List<Part> parts) throws IOException {
    MultipartBody.Builder builder = new MultipartBody.Builder().setType(MultipartBody.FORM);
    for (Part part : parts) {
      if (part.file != null) builder.addFormDataPart(part.name, part.file.getFileName().toString(), RequestBody.create(part.file.toFile(), MediaType.parse("application/octet-stream")));
      else builder.addFormDataPart(part.name, part.value);
    }
    Request request = new Request.Builder().url(baseUrl + "/" + endpoint).header("Api-Key", key)
        .header("Accept", "application/json").post(builder.build()).build();
    try (Response response = CLIENT.newCall(request).execute()) { return result(response, endpoint); }
  }

  private static JSONObject result(Response response, String endpoint) throws IOException {
    String text = response.body() == null ? "" : response.body().string();
    System.out.println(endpoint + ": " + response.code());
    if (!response.isSuccessful()) throw new IOException(endpoint + " failed: " + text);
    return new JSONObject(text);
  }

  private static String rgb(JSONArray values) { List<String> result = new ArrayList<>(); for (Object value : values) result.add(value.toString()); return String.join(",", result); }
  private static String money(double value) { return String.format(java.util.Locale.US, "$%,.2f", value); }

  private static List<String[]> readCsv() throws IOException {
    List<String[]> rows = new ArrayList<>();
    for (String line : Files.readAllLines(DATA_DIR.resolve("line-items.csv"))) {
      if (line.startsWith("itemCode,")) continue;
      List<String> fields = new ArrayList<>(); StringBuilder field = new StringBuilder(); boolean quoted = false;
      for (char character : line.toCharArray()) { if (character == '"') quoted = !quoted; else if (character == ',' && !quoted) { fields.add(field.toString()); field.setLength(0); } else field.append(character); }
      fields.add(field.toString()); if (fields.size() == 4) rows.add(fields.toArray(new String[0]));
    }
    return rows;
  }

  private static JSONArray textObjects(JSONObject metadata, JSONObject style) {
    JSONObject seller = metadata.getJSONObject("seller"), customer = metadata.getJSONObject("customer");
    JSONArray objects = new JSONArray(); String primary = rgb(style.getJSONArray("primaryColorRgb")), muted = rgb(style.getJSONArray("mutedTextColorRgb"));
    addText(objects, style, 1, 405, 724, "INVOICE", 22, primary, 153, "H1", true);
    addText(objects, style, 1, 405, 696, "Invoice " + metadata.getString("invoiceNumber"), 9, muted, 153, "P", true);
    addText(objects, style, 1, 405, 682, "Issued " + metadata.getString("issueDate"), 9, muted, 153, "P", false);
    addText(objects, style, 1, 405, 668, "Due " + metadata.getString("dueDate"), 9, muted, 153, "P", false);
    addText(objects, style, 1, 66, 622, "FROM", 8, primary, 216, "H2", true); addText(objects, style, 1, 66, 606, seller.getString("name"), 9, "34,34,34", 216, "P", true); addText(objects, style, 1, 66, 592, seller.getString("taxId"), 8, muted, 216, "P", false); addText(objects, style, 1, 66, 578, seller.getString("addressLine1"), 8, muted, 216, "P", false); addText(objects, style, 1, 66, 566, seller.getString("city") + ", " + seller.getString("region") + " " + seller.getString("postalCode"), 8, muted, 216, "P", false);
    addText(objects, style, 1, 330, 622, "BILL TO", 8, primary, 216, "H2", true); addText(objects, style, 1, 330, 606, customer.getString("name"), 9, "34,34,34", 216, "P", true); addText(objects, style, 1, 330, 592, customer.getString("addressLine1"), 8, muted, 216, "P", false); addText(objects, style, 1, 330, 578, customer.getString("city") + ", " + customer.getString("region") + " " + customer.getString("postalCode"), 8, muted, 216, "P", false);
    return objects;
  }

  private static void addText(JSONArray objects, JSONObject style, int page, int x, int y, String text, double size, String color, int width, String structure, boolean bold) {
    objects.put(new JSONObject().put("font", bold ? style.getString("boldFont") : style.getString("bodyFont")).put("max_width", width).put("opacity", "1").put("page", String.valueOf(page)).put("rotation", "0").put("text", text).put("text_color_rgb", color).put("text_size", size).put("x", x).put("y", y).put("tag_structure_type", structure));
  }

  private static JSONArray tableObjects(JSONObject metadata, JSONObject style, List<String[]> items) {
    double subtotal = 0; for (String[] item : items) subtotal += Double.parseDouble(item[2]) * Double.parseDouble(item[3]); double tax = Math.round(subtotal * metadata.getDouble("taxRate") * 100) / 100.0; double total = subtotal + tax;
    JSONArray header = new JSONArray(); String[] names = {"Description", "Qty", "Unit Price", "Amount"}; for (int i = 0; i < names.length; i++) { JSONObject cell = new JSONObject().put("text", names[i]).put("tag_structure_type", "TH").put("style", new JSONObject().put("background_color_rgb", style.getJSONArray("primaryColorRgb")).put("text_color_rgb", new JSONArray(List.of(255, 255, 255)))); if (i > 0) cell.getJSONObject("style").put("text_align", "right"); header.put(cell); }
    JSONArray rows = new JSONArray();
    for (String[] item : items) {
      JSONArray cells = new JSONArray();
      cells.put(new JSONObject().put("text", item[1]));
      cells.put(new JSONObject().put("text", item[2]).put("style", new JSONObject().put("text_align", "right")));
      cells.put(new JSONObject().put("text", money(Double.parseDouble(item[3]))).put("style", new JSONObject().put("text_align", "right")));
      cells.put(new JSONObject().put("text", money(Double.parseDouble(item[2]) * Double.parseDouble(item[3]))).put("style", new JSONObject().put("text_align", "right")));
      rows.put(new JSONObject().put("cells", cells));
    }
    JSONArray footer = new JSONArray(); footer.put(summary("Subtotal", subtotal, null)); footer.put(summary("Tax (" + String.format(java.util.Locale.US, "%.2f", metadata.getDouble("taxRate") * 100) + "%)", tax, null)); footer.put(summary("Total", total, style.getJSONArray("accentColorRgb")));
    JSONObject table = new JSONObject().put("page", 1).put("x", 54).put("y", 510).put("width", 504).put("columns", new JSONArray(List.of(new JSONObject().put("width", 276), new JSONObject().put("width", 54), new JSONObject().put("width", 84), new JSONObject().put("width", 90)))).put("continuation_page_top_margin", 85).put("page_bottom_margin", 96).put("final_page_bottom_margin", 164).put("overflow_behavior", "split-row").put("row_split_behavior", "prefer-next-page").put("repeat_header_on_overflow", true).put("show_footer_on_last_page", true).put("tag_structure_type", "Table").put("style", new JSONObject().put("padding", new JSONObject().put("top", 5).put("right", 6).put("bottom", 5).put("left", 6)).put("text_size", style.getDouble("tableHeaderFontSize")).put("text_color_rgb", style.getJSONArray("textColorRgb"))).put("header_rows", new JSONArray(List.of(new JSONObject().put("cells", header)))).put("rows", rows).put("footer_rows", footer);
    return new JSONArray().put(table);
  }

  private static JSONObject summary(String label, double value, JSONArray accent) { JSONObject extra = new JSONObject().put("text_align", "right"); if (accent != null) extra.put("background_color_rgb", accent).put("text_size", 10); return new JSONObject().put("cells", new JSONArray(List.of(new JSONObject().put("text", label).put("col_span", 3).put("style", extra), new JSONObject().put("text", money(value)).put("style", extra)))); }
  private static JSONArray shapes(JSONObject style, int page, boolean footer) { String border = rgb(style.getJSONArray("borderColorRgb")); if (footer) return new JSONArray().put(new JSONObject().put("type", "rectangle").put("page", page).put("x", 54).put("y", 70).put("width", 504).put("height", 104).put("fill_color_rgb", "248,250,251").put("stroke_color_rgb", border).put("stroke_width", 0.5).put("tag_is_artifact", true)); return new JSONArray().put(new JSONObject().put("type", "rectangle").put("page", 1).put("x", 54).put("y", 540).put("width", 240).put("height", 96).put("fill_color_rgb", rgb(style.getJSONArray("accentColorRgb"))).put("stroke_color_rgb", border).put("stroke_width", 0.5).put("tag_is_artifact", true)).put(new JSONObject().put("type", "rectangle").put("page", 1).put("x", 318).put("y", 540).put("width", 240).put("height", 96).put("fill_color_rgb", rgb(style.getJSONArray("accentColorRgb"))).put("stroke_color_rgb", border).put("stroke_width", 0.5).put("tag_is_artifact", true)); }

  public static void main(String[] args) throws Exception {
    Dotenv dotenv = Dotenv.configure().ignoreIfMissing().load(); String apiUrl = (dotenv.get("PDFREST_URL", DEFAULT_URL)).replaceAll("/+$", ""); String apiKey = dotenv.get("PDFREST_API_KEY", DEFAULT_KEY);
    JSONObject metadata = new JSONObject(Files.readString(DATA_DIR.resolve("metadata.json"))), style = new JSONObject(Files.readString(DATA_DIR.resolve("style.json"))); List<String[]> items = readCsv();
    String id = postJson(apiUrl, apiKey, "blank-pdf", new JSONObject().put("page_size", "letter").put("page_count", 1).put("page_orientation", "portrait")).getString("outputId");
    id = postMultipart(apiUrl, apiKey, "pdf-with-added-shapes", List.of(Part.value("id", id), Part.value("shape_objects", shapes(style, 1, false).toString()), Part.value("tag_enabled", "true"))).getString("outputId");
    id = postMultipart(apiUrl, apiKey, "pdf-with-added-text", List.of(Part.value("id", id), Part.value("text_objects", textObjects(metadata, style).toString()), Part.value("tag_enabled", "true"), Part.value("tag_language", "en-US"))).getString("outputId");
    id = postMultipart(apiUrl, apiKey, "pdf-with-added-tables", List.of(Part.value("id", id), Part.value("table_objects", tableObjects(metadata, style, items).toString()), Part.value("tag_enabled", "true"), Part.value("tag_language", "en-US"))).getString("outputId");
    id = postMultipart(apiUrl, apiKey, "pdf-with-added-image", List.of(Part.value("id", id), Part.value("image_objects", new JSONObject().put("image_index", 0).put("page", 1).put("x", 54).put("y", 716).put("width", 200).put("tag_alt_text", "Northstar Sample Supply logo").put("tag_structure_type", "Figure").toString()), Part.file("image_files", DATA_DIR.resolve("northstar-logo.png")), Part.value("tag_enabled", "true"), Part.value("tag_language", "en-US"))).getString("outputId");
    int pageCount = postMultipart(apiUrl, apiKey, "pdf-info", List.of(Part.value("id", id), Part.value("queries", "page_count"))).getInt("page_count");
    id = postMultipart(apiUrl, apiKey, "pdf-with-added-shapes", List.of(Part.value("id", id), Part.value("shape_objects", shapes(style, pageCount, true).toString()), Part.value("tag_enabled", "true"))).getString("outputId");
    JSONArray footer = new JSONArray(); addText(footer, style, pageCount, 66, 156, "Payment terms", 8, rgb(style.getJSONArray("mutedTextColorRgb")), 480, "H2", true); addText(footer, style, pageCount, 66, 142, metadata.getString("paymentTerms"), 7.5, rgb(style.getJSONArray("mutedTextColorRgb")), 480, "P", false); addText(footer, style, pageCount, 66, 112, "Notes", 8, rgb(style.getJSONArray("mutedTextColorRgb")), 480, "H2", true); addText(footer, style, pageCount, 66, 98, metadata.getString("notes"), 7.5, rgb(style.getJSONArray("mutedTextColorRgb")), 480, "P", false);
    for (int page = 1; page <= pageCount; page++) { addText(footer, style, page, 54, 54, "Generated from structured JSON and CSV input with pdfRest.", 7.5, rgb(style.getJSONArray("mutedTextColorRgb")), 400, "P", false); addText(footer, style, page, 490, 54, "Page " + page + " of " + pageCount, 7.5, rgb(style.getJSONArray("mutedTextColorRgb")), 68, "P", false); }
    JSONObject finalResult = postMultipart(apiUrl, apiKey, "pdf-with-added-text", List.of(Part.value("id", id), Part.value("text_objects", footer.toString()), Part.value("tag_enabled", "true"), Part.value("tag_language", "en-US"), Part.value("output", "invoice_from_structured_data")));
    Request download = new Request.Builder().url(apiUrl + "/resource/" + finalResult.getString("outputId") + "?format=file").header("Api-Key", apiKey).get().build(); try (Response response = CLIENT.newCall(download).execute()) { if (!response.isSuccessful()) throw new IOException("Download failed: " + response.code()); Files.write(OUTPUT, response.body().bytes()); }
    System.out.println("Created " + OUTPUT.toAbsolutePath());
  }

  private record Part(String name, String value, Path file) { static Part value(String name, String value) { return new Part(name, value, null); } static Part file(String name, Path file) { return new Part(name, null, file); } }
}
