const axios = require("axios");
const fs = require("fs");
const FormData = require("form-data");

// By default, we use the US-based API service. This is the primary endpoint for global use.
const apiUrl = process.env.PDFREST_URL || "https://api.pdfrest.com";
const apiKey = process.env.PDFREST_API_KEY || "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

// This sample converts Markdown input to a tagged PDF through multipart /pdf.
// It demonstrates structured_text_options, table styling, tagging, and uploaded Markdown image mapping.
const inputPath = process.argv[2] || "/path/to/sample.md";
const imagePath = process.argv[3] || "/path/to/logo.png";
const form = new FormData();
form.append("file", fs.createReadStream(inputPath));
form.append("image_files", fs.createReadStream(imagePath));
form.append("structured_text_options", JSON.stringify({"title":"Structured Content Sample","language":"en-US","enable_tagging":true,"page_setup":{"size":"Letter","orientation":"portrait","margin":{"top":36,"right":42,"bottom":36,"left":42}},"style":{"font":"Arial","heading_font":"Arial","code_font":"Courier","text_size":11,"text_color_rgb":[34,34,34],"heading_scale":1.35,"table":{"column_width_weights":[2,3,2],"keep_header_with_first_row":true,"repeat_headers_on_overflow":true,"show_borders":true,"border_width":0.75,"border_color_rgb":[180,188,200],"header_fill_color_rgb":[33,64,98],"header_text_color_rgb":[255,255,255],"row_fill_color_rgb":[250,250,252],"alternate_row_fill_color_rgb":[235,240,246],"cell_padding":{"top":6,"right":8,"bottom":6,"left":8}}},"markdown":{"image_alt_text":{"sample-logo":"Sample logo"},"missing_image_alt_text":"fail","image_sources":{"sample-logo":{"upload_index":0}}}}));
form.append("output", "pdf_from_markdown");

axios.post(apiUrl + "/pdf", form, { headers: { "Api-Key": apiKey, ...form.getHeaders() }, maxBodyLength: Infinity })
.then((response) => { console.log(JSON.stringify(response.data, null, 2)); })
.catch((error) => { console.error(error.response ? error.response.data : error.message); process.exitCode = 1; });

