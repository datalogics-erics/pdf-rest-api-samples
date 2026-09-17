// Validate a hybrid ZUGFeRD / Factur-X PDF without modifying it.
const axios = require("axios");
const FormData = require("form-data");
const fs = require("fs");

const apiUrl = "https://api.pdfrest.com";
// const apiUrl = "https://eu-api.pdfrest.com"; // EU/GDPR service
const apiKey = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
const zugferdPdf = "/path/to/zugferd-invoice.pdf";

const form = new FormData();
form.append("file", fs.createReadStream(zugferdPdf));
axios.post(`${apiUrl}/validated-zugferd`, form, {
  headers: { "Api-Key": apiKey, ...form.getHeaders() }, maxBodyLength: Infinity,
}).then((response) => console.log(JSON.stringify(response.data, null, 2)))
  .catch((error) => { console.error(error.response?.data || error.message); process.exitCode = 1; });
