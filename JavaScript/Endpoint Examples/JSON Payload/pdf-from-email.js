var axios = require("axios");
var fs = require("fs");

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

// This sample converts an Email (.eml) file to PDF. It uploads the Email file
// first, then calls /pdf with its resource ID.
var inputPath = "/path/to/sample.eml";
async function upload(path) {
  var response = await axios.post(apiUrl + "/upload", fs.createReadStream(path), { headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", "Content-Type": "application/octet-stream", "Content-Filename": path.split("/").pop() }, maxBodyLength: Infinity });
  return response.data.files[0].id;
}

async function main() {
  var inputId = await upload(inputPath);
  var payload = { id: inputId, output: "pdf_from_email" };
  var response = await axios.post(apiUrl + "/pdf", payload, { headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", "Content-Type": "application/json" } });
  console.log(JSON.stringify(response.data, null, 2));
}

main().catch((error) => { console.error(error.response ? error.response.data : error.message); process.exitCode = 1; });
