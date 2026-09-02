var axios = require("axios");
var fs = require("fs");
var FormData = require("form-data");

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

// This sample converts an Email (.eml) file to PDF by sending it directly in a
// multipart /pdf request.
var inputPath = "/path/to/sample.eml";
var form = new FormData();
form.append("file", fs.createReadStream(inputPath), { contentType: "message/rfc822" });
form.append("output", "pdf_from_email");

axios.post(apiUrl + "/pdf", form, { headers: { "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", ...form.getHeaders() }, maxBodyLength: Infinity })
.then((response) => { console.log(JSON.stringify(response.data, null, 2)); })
.catch((error) => { console.error(error.response ? error.response.data : error.message); process.exitCode = 1; });
