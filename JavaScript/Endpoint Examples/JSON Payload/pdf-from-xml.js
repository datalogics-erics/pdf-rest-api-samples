const axios = require("axios");
const fs = require("fs");
const FormData = require("form-data");

// By default, we use the US-based API service. This is the primary endpoint for global use.
const apiUrl = process.env.PDFREST_URL || "https://api.pdfrest.com";
const apiKey = process.env.PDFREST_API_KEY || "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

// This sample uploads XML input, then calls /pdf with a JSON payload.
// It demonstrates structured_text_options and the format-specific conversion options.
const inputPath = process.argv[2] || "/path/to/sample.xml";
async function upload(path) {
  const response = await axios.post(apiUrl + "/upload", fs.createReadStream(path), { headers: { "Api-Key": apiKey, "Content-Type": "application/octet-stream", "Content-Filename": path.split("/").pop() }, maxBodyLength: Infinity });
  return response.data.files[0].id;
}

async function main() {
  const inputId = await upload(inputPath);
  const payload = { id: inputId, structured_text_options: {"title":"Structured Content Sample","language":"en-US","enable_tagging":true,"page_setup":{"size":"Letter","orientation":"portrait","margin":{"top":36,"right":42,"bottom":36,"left":42}},"style":{"font":"Arial","heading_font":"Arial","code_font":"Courier","text_size":11,"text_color_rgb":[34,34,34],"heading_scale":1.35,"table":{"column_width_weights":[2,3,2],"keep_header_with_first_row":true,"repeat_headers_on_overflow":true,"show_borders":true,"border_width":0.75,"border_color_rgb":[180,188,200],"header_fill_color_rgb":[33,64,98],"header_text_color_rgb":[255,255,255],"row_fill_color_rgb":[250,250,252],"alternate_row_fill_color_rgb":[235,240,246],"cell_padding":{"top":6,"right":8,"bottom":6,"left":8}}},"data_presentation":"hierarchy"} };
  const response = await axios.post(apiUrl + "/pdf", payload, { headers: { "Api-Key": apiKey, "Content-Type": "application/json" } });
  console.log(JSON.stringify(response.data, null, 2));
}

main().catch((error) => { console.error(error.response ? error.response.data : error.message); process.exitCode = 1; });

