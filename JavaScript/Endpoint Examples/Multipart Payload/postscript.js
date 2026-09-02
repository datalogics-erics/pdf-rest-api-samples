var axios = require("axios");
var fs = require("fs");
var FormData = require("form-data");

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

/* This sample converts PDF to PostScript through multipart /postscript.
 * Pairing this endpoint with /pdf creates a PDF -> PostScript -> PDF workflow commonly
 * called PDF refrying. Some print and prepress workflows use it to rebuild, flatten, or
 * normalize page content, but the lossy roundtrip can discard PDF-specific features.
 * These settings request Level 3, text-safe output, all pages at original scale,
 * shrink-to-fit without rotation, and printable annotations.
 */
var inputPath = "/path/to/sample.pdf";
var form = new FormData();
form.append("file", fs.createReadStream(inputPath), { contentType: "application/pdf" });
form.append("ps_level", "3");
form.append("page_range", "all");
form.append("binary_output", "false");
form.append("scale", "1");
form.append("rotate", "false");
form.append("shrink_to_fit", "true");
form.append("print_annotations", "true");
form.append("output", "postscript_from_pdf");

axios.post(apiUrl + "/postscript", form, { headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", ...form.getHeaders() }, maxBodyLength: Infinity })
.then((response) => { console.log(JSON.stringify(response.data, null, 2)); })
.catch((error) => { console.error(error.response ? error.response.data : error.message); process.exitCode = 1; });
