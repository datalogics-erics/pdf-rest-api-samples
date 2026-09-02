/**
 * Refry a PDF by converting it to PostScript and back to PDF.
 *
 * PDF refrying is the common name for a PDF -> PostScript -> PDF roundtrip.
 * Some print, prepress, and legacy production workflows use it to rebuild or
 * normalize page content, flatten certain PDF constructs, or prepare a file
 * for downstream systems. The process is intentionally lossy and may remove
 * tags, forms, layers, annotations, transparency, metadata, and editability.
 * Use this workflow when a downstream system requires rebuilt page content or
 * a PostScript-based interchange file.
 *
 * The PostScript-to-PDF step uses a custom .joboptions profile in this sample.
 * A .joboptions file contains Adobe Distiller-compatible conversion settings;
 * it is optional, and default settings are used when omitted. pdfRest applies
 * the profile with Datalogics PDF Converter SDK. Datalogics maintains the SDK
 * in partnership with Adobe, using the same Adobe technology that powers
 * Distiller.
 *
 * Run: node refry-pdf.js <pdf> <jobOptions> [outputPdf]
 */
const axios = require("axios");
const FormData = require("form-data");
const fs = require("fs");
const path = require("path");

const apiUrl = (process.env.PDFREST_URL || "https://api.pdfrest.com").replace(/\/$/, "");
const apiKey = process.env.PDFREST_API_KEY || "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
const inputPath = process.argv[2] || "/path/to/sample.pdf";
const jobOptionsPath = process.argv[3] || "/path/to/custom.joboptions";
const outputPath = process.argv[4] || path.join(__dirname, "refried.pdf");

async function postMultipart(endpoint, fields) {
  const form = new FormData();
  for (const field of fields) {
    if (field.path) {
      form.append(field.name, fs.createReadStream(field.path), {
        filename: path.basename(field.path),
        contentType: field.contentType,
      });
    } else {
      form.append(field.name, field.value);
    }
  }
  const response = await axios.post(`${apiUrl}/${endpoint}`, form, {
    headers: { Accept: "application/json", "Api-Key": apiKey, ...form.getHeaders() },
    maxBodyLength: Infinity,
  });
  console.log(`${endpoint}: ${response.status}`);
  return response.data;
}

async function main() {
  const postscript = await postMultipart("postscript", [
    { name: "file", path: inputPath, contentType: "application/pdf" },
    { name: "ps_level", value: "3" },
    { name: "page_range", value: "all" },
    { name: "binary_output", value: "true" },
    { name: "scale", value: "1" },
    { name: "rotate", value: "false" },
    { name: "shrink_to_fit", value: "true" },
    { name: "print_annotations", value: "true" },
    { name: "output", value: "refry_intermediate" },
  ]);
  const finalPdf = await postMultipart("pdf", [
    { name: "id", value: postscript.outputId },
    { name: "job_options", path: jobOptionsPath, contentType: "application/octet-stream" },
    { name: "output", value: "refried" },
  ]);
  const download = await axios.get(`${apiUrl}/resource/${finalPdf.outputId}?format=file`, {
    headers: { "Api-Key": apiKey },
    responseType: "arraybuffer",
  });
  fs.writeFileSync(outputPath, download.data);
  console.log(JSON.stringify(finalPdf, null, 2));
  console.log(`Created ${outputPath}`);
}

main().catch((error) => {
  console.error(error.response ? error.response.data : error.message);
  process.exitCode = 1;
});
