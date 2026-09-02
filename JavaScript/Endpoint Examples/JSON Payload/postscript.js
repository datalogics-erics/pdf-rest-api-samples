var axios = require("axios");
var fs = require("fs");

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

/* This sample uploads a PDF, then converts it through the JSON /postscript flow.
 * Pairing this endpoint with /pdf creates a PDF -> PostScript -> PDF workflow commonly
 * called PDF refrying. Some print and prepress workflows use it to rebuild, flatten, or
 * normalize page content, but the lossy roundtrip can discard PDF-specific features.
 * These settings request Level 3, text-safe output, all pages at original scale,
 * shrink-to-fit without rotation, and printable annotations.
 */
var inputPath = "/path/to/sample.pdf";
async function upload(path) {
  var response = await axios.post(apiUrl + "/upload", fs.createReadStream(path), { headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", "Content-Type": "application/octet-stream", "Content-Filename": path.split("/").pop() }, maxBodyLength: Infinity });
  return response.data.files[0].id;
}

async function main() {
  var inputId = await upload(inputPath);
  var payload = { id: inputId, ps_level: 3, page_range: "all", binary_output: false, scale: 1, rotate: false, shrink_to_fit: true, print_annotations: true, output: "postscript_from_pdf" };
  var response = await axios.post(apiUrl + "/postscript", payload, { headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", "Content-Type": "application/json" } });
  console.log(JSON.stringify(response.data, null, 2));
}

main().catch((error) => { console.error(error.response ? error.response.data : error.message); process.exitCode = 1; });
