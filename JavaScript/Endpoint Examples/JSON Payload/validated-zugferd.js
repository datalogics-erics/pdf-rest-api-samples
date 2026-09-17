// Upload a hybrid PDF, then validate its ZUGFeRD / Factur-X package by resource ID.
const axios = require("axios");
const fs = require("fs");
const path = require("path");

const apiUrl = "https://api.pdfrest.com";
// const apiUrl = "https://eu-api.pdfrest.com"; // EU/GDPR service
const apiKey = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
const zugferdPdf = "/path/to/zugferd-invoice.pdf";

async function validateZugferdPdf() {
  const upload = await axios.post(`${apiUrl}/upload`, fs.createReadStream(zugferdPdf), {
    headers: { "Api-Key": apiKey, "Content-Type": "application/pdf", "Content-Filename": path.basename(zugferdPdf) },
    maxBodyLength: Infinity,
  });
  const response = await axios.post(`${apiUrl}/validated-zugferd`, { id: upload.data.files[0].id }, { headers: { "Api-Key": apiKey } });
  console.log(JSON.stringify(response.data, null, 2));
}

validateZugferdPdf().catch((error) => { console.error(error.response?.data || error.message); process.exitCode = 1; });
