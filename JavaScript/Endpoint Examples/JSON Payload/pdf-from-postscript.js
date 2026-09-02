var axios = require("axios");
var fs = require("fs");

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

/* This sample converts a PostScript (.ps) file to PDF with a custom .joboptions profile.
 * A .joboptions file contains Adobe Distiller-compatible conversion settings. The
 * profile is optional; omit job_options to use default settings. pdfRest applies a
 * supplied profile with Datalogics PDF Converter SDK. Datalogics maintains the SDK
 * in partnership with Adobe, using the same Adobe technology that powers Distiller.
 * Pairing this /pdf call with /postscript creates a PDF -> PostScript -> PDF workflow
 * commonly called PDF refrying. Some print and prepress workflows use it to rebuild or
 * normalize page content, but the lossy roundtrip can discard PDF-specific features.
 */
var inputPath = "/path/to/sample.ps";
var jobOptionsPath = "/path/to/custom.joboptions";
async function upload(path) {
  var response = await axios.post(apiUrl + "/upload", fs.createReadStream(path), { headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", "Content-Type": "application/octet-stream", "Content-Filename": path.split("/").pop() }, maxBodyLength: Infinity });
  return response.data.files[0].id;
}

async function main() {
  var inputId = await upload(inputPath);
  var jobOptionsId = await upload(jobOptionsPath);
  var payload = { id: inputId, job_options_id: jobOptionsId, output: "pdf_from_postscript" };
  var response = await axios.post(apiUrl + "/pdf", payload, { headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", "Content-Type": "application/json" } });
  console.log(JSON.stringify(response.data, null, 2));
}

main().catch((error) => { console.error(error.response ? error.response.data : error.message); process.exitCode = 1; });
