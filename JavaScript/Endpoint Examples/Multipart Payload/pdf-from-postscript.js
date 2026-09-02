var axios = require("axios");
var fs = require("fs");
var FormData = require("form-data");

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
var form = new FormData();
form.append("file", fs.createReadStream(inputPath), { contentType: "application/postscript" });
form.append("job_options", fs.createReadStream(jobOptionsPath), { contentType: "application/octet-stream" });
form.append("output", "pdf_from_postscript");

axios.post(apiUrl + "/pdf", form, { headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", ...form.getHeaders() }, maxBodyLength: Infinity })
.then((response) => { console.log(JSON.stringify(response.data, null, 2)); })
.catch((error) => { console.error(error.response ? error.response.data : error.message); process.exitCode = 1; });
