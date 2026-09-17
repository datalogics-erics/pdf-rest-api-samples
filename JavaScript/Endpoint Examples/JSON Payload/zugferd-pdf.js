// Upload invoice XML, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
const axios = require("axios");
const fs = require("fs");
const path = require("path");

const apiUrl = "https://api.pdfrest.com";
// const apiUrl = "https://eu-api.pdfrest.com"; // EU/GDPR service
const apiKey = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
const invoiceXml = "/path/to/invoice.xml";

async function createZugferdPdf() {
  const upload = await axios.post(`${apiUrl}/upload`, fs.createReadStream(invoiceXml), {
    headers: { "Api-Key": apiKey, "Content-Type": "application/xml", "Content-Filename": path.basename(invoiceXml) },
    maxBodyLength: Infinity,
  });
  const response = await axios.post(`${apiUrl}/zugferd-pdf`, {
    id: upload.data.files[0].id, output: "zugferd_invoice",
    render_options: { locale: "de-DE", label_language: "de", font: "arial", bold_font: "arialbold", accent_color_rgb: [0, 92, 171] },
  }, { headers: { "Api-Key": apiKey } });
  console.log(JSON.stringify(response.data, null, 2));
}

createZugferdPdf().catch((error) => { console.error(error.response?.data || error.message); process.exitCode = 1; });
