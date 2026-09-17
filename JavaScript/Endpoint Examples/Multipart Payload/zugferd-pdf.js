// Create a ZUGFeRD / Factur-X PDF/A-3 invoice from XML and an existing invoice PDF.
const axios = require("axios");
const FormData = require("form-data");
const fs = require("fs");

const apiUrl = "https://api.pdfrest.com";
// const apiUrl = "https://eu-api.pdfrest.com"; // EU/GDPR service
const apiKey = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
const invoiceXml = "/path/to/invoice.xml";
const invoicePdf = "/path/to/invoice.pdf";

const form = new FormData();
form.append("file", fs.createReadStream(invoiceXml));
form.append("pdf_file", fs.createReadStream(invoicePdf));
form.append("regenerate_pdf", "true");
form.append("render_options", JSON.stringify({
  locale: "de-DE", label_language: "de", font: "arial", bold_font: "arialbold",
  accent_color_rgb: [0, 92, 171],
}));
form.append("output", "zugferd_invoice");

axios.post(`${apiUrl}/zugferd-pdf`, form, {
  headers: { "Api-Key": apiKey, ...form.getHeaders() }, maxBodyLength: Infinity,
}).then((response) => console.log(JSON.stringify(response.data, null, 2)))
  .catch((error) => { console.error(error.response?.data || error.message); process.exitCode = 1; });
