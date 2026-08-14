/**
 * Generate a tagged invoice from JSON and CSV input using pdfRest.
 *
 * Set PDFREST_API_KEY before running this sample. It reads metadata.json,
 * style.json, and line-items.csv from invoice-data, then downloads the PDF
 * beside this script. Install dependencies with `npm install` and run with
 * `node create-invoice-from-structured-data.js`.
 */
const axios = require("axios");
const FormData = require("form-data");
const fs = require("fs");
const path = require("path");

const apiUrl = (process.env.PDFREST_URL || "https://api.pdfrest.com").replace(/\/$/, "");
const apiKey = process.env.PDFREST_API_KEY || "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
// Keep the JSON, CSV, and logo fixtures beside this sample so it can run independently.
const dataDir = path.join(__dirname, "invoice-data");
const outputPath = path.join(__dirname, "invoice-from-structured-data.pdf");
const tableWidths = [276, 54, 84, 90];

function parseCsv(text) {
  const rows = [];
  let row = [];
  let field = "";
  let quoted = false;
  for (const character of `${text}\n`) {
    if (character === '"') quoted = !quoted;
    else if (character === "," && !quoted) { row.push(field); field = ""; }
    else if (character === "\n" && !quoted) { row.push(field); rows.push(row); row = []; field = ""; }
    else field += character;
  }
  const headers = rows.shift();
  return rows.filter((values) => values.length > 1).map((values) =>
    Object.fromEntries(headers.map((header, index) => [header, values[index]]))
  );
}

function rgb(values) { return values.join(","); }
function money(value) { return `$${value.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`; }

async function postJson(endpoint, payload) {
  const response = await axios.post(`${apiUrl}/${endpoint}`, payload, { headers: { "Api-Key": apiKey } });
  console.log(`${endpoint}: ${response.status}`);
  return response.data;
}

async function postMultipart(endpoint, fields) {
  const form = new FormData();
  for (const [name, value] of Object.entries(fields)) {
    if (value && typeof value === "object" && value.path) form.append(name, fs.createReadStream(value.path), value.name);
    else form.append(name, String(value));
  }
  const response = await axios.post(`${apiUrl}/${endpoint}`, form, { headers: { "Api-Key": apiKey, ...form.getHeaders() }, maxBodyLength: Infinity });
  console.log(`${endpoint}: ${response.status}`);
  return response.data;
}

async function uploadFile(filePath) {
  const response = await axios.post(`${apiUrl}/upload`, fs.createReadStream(filePath), {
    headers: { "Api-Key": apiKey, "Content-Filename": path.basename(filePath), "Content-Type": "application/octet-stream" },
  });
  return response.data.files[0].id;
}

function textObjects(metadata, style) {
  const seller = metadata.seller;
  const customer = metadata.customer;
  const objects = [];
  const add = (x, y, text, size, color, width, structure = "P", bold = false) => objects.push({
    font: bold ? style.boldFont : style.bodyFont, max_width: width, opacity: "1", page: "1",
    rotation: "0", text, text_color_rgb: color, text_size: size, x, y,
    tag_structure_type: structure,
  });
  add(405, 724, "INVOICE", 22, rgb(style.primaryColorRgb), 153, "H1", true);
  add(405, 696, `Invoice ${metadata.invoiceNumber}`, 9, rgb(style.mutedTextColorRgb), 153, "P", true);
  add(405, 682, `Issued ${metadata.issueDate}`, 9, rgb(style.mutedTextColorRgb), 153);
  add(405, 668, `Due ${metadata.dueDate}`, 9, rgb(style.mutedTextColorRgb), 153);
  add(66, 622, "FROM", 8, rgb(style.primaryColorRgb), 216, "H2", true);
  add(66, 606, seller.name, 9, "34,34,34", 216, "P", true);
  add(66, 592, seller.taxId, 8, rgb(style.mutedTextColorRgb), 216);
  add(66, 578, seller.addressLine1, 8, rgb(style.mutedTextColorRgb), 216);
  add(66, 566, `${seller.city}, ${seller.region} ${seller.postalCode}`, 8, rgb(style.mutedTextColorRgb), 216);
  add(330, 622, "BILL TO", 8, rgb(style.primaryColorRgb), 216, "H2", true);
  add(330, 606, customer.name, 9, "34,34,34", 216, "P", true);
  add(330, 592, customer.addressLine1, 8, rgb(style.mutedTextColorRgb), 216);
  add(330, 578, `${customer.city}, ${customer.region} ${customer.postalCode}`, 8, rgb(style.mutedTextColorRgb), 216);
  return objects;
}

function tableObjects(metadata, style, items) {
  const subtotal = items.reduce((sum, item) => sum + Number(item.quantity) * Number(item.unitPrice), 0);
  const tax = Math.round(subtotal * metadata.taxRate * 100) / 100;
  const total = subtotal + tax;
  const headerStyle = { background_color_rgb: style.primaryColorRgb, text_color_rgb: [255, 255, 255] };
  const header = ["Description", "Qty", "Unit Price", "Amount"].map((text, index) => ({
    text, tag_structure_type: "TH", style: { ...headerStyle, ...(index ? { text_align: "right" } : {}) },
  }));
  const rows = items.map((item) => ({ cells: [
    { text: item.description }, { text: item.quantity, style: { text_align: "right" } },
    { text: money(Number(item.unitPrice)), style: { text_align: "right" } },
    { text: money(Number(item.quantity) * Number(item.unitPrice)), style: { text_align: "right" } },
  ] }));
  const summary = (label, value, extra = {}) => ({ cells: [
    { text: label, col_span: 3, style: { text_align: "right", ...extra } },
    { text: money(value), style: { text_align: "right", ...extra } },
  ] });
  return [{ page: 1, x: 54, y: 510, width: 504, columns: tableWidths.map((width) => ({ width })),
    continuation_page_top_margin: 85, page_bottom_margin: 96, final_page_bottom_margin: 164,
    overflow_behavior: "split-row", row_split_behavior: "prefer-next-page", repeat_header_on_overflow: true,
    show_footer_on_last_page: true, tag_structure_type: "Table",
    style: { border: { top: { color_rgb: style.borderColorRgb, width: 0.5 }, right: { color_rgb: style.borderColorRgb, width: 0.5 }, bottom: { color_rgb: style.borderColorRgb, width: 0.5 }, left: { color_rgb: style.borderColorRgb, width: 0.5 } }, padding: { top: 5, right: 6, bottom: 5, left: 6 }, text_size: style.tableHeaderFontSize, text_color_rgb: style.textColorRgb },
    header_rows: [{ cells: header }], rows, footer_rows: [summary("Subtotal", subtotal), summary(`Tax (${(metadata.taxRate * 100).toFixed(2)}%)`, tax), summary("Total", total, { background_color_rgb: style.accentColorRgb, text_size: 10 })] }];
}

function shapeObjects(style, page, footer = false) {
  const border = { stroke_color_rgb: rgb(style.borderColorRgb), stroke_width: 0.5, tag_is_artifact: true };
  if (footer) return [{ type: "rectangle", page, x: 54, y: 70, width: 504, height: 104, fill_color_rgb: "248,250,251", ...border }];
  return [
    { type: "rectangle", page: 1, x: 54, y: 540, width: 240, height: 96, fill_color_rgb: rgb(style.accentColorRgb), ...border },
    { type: "rectangle", page: 1, x: 318, y: 540, width: 240, height: 96, fill_color_rgb: rgb(style.accentColorRgb), ...border },
  ];
}

async function main() {
  const metadata = JSON.parse(fs.readFileSync(path.join(dataDir, "metadata.json")));
  const style = JSON.parse(fs.readFileSync(path.join(dataDir, "style.json")));
  const items = parseCsv(fs.readFileSync(path.join(dataDir, "line-items.csv"), "utf8"));
  let currentId = (await postJson("blank-pdf", { page_size: "letter", page_count: 1, page_orientation: "portrait" })).outputId;
  currentId = (await postMultipart("pdf-with-added-shapes", { id: currentId, shape_objects: JSON.stringify(shapeObjects(style, 1)), tag_enabled: true })).outputId;
  currentId = (await postMultipart("pdf-with-added-text", { id: currentId, text_objects: JSON.stringify(textObjects(metadata, style)), tag_enabled: true, tag_language: "en-US" })).outputId;
  currentId = (await postMultipart("pdf-with-added-tables", { id: currentId, table_objects: JSON.stringify(tableObjects(metadata, style, items)), tag_enabled: true, tag_language: "en-US" })).outputId;
  const logoPath = path.join(dataDir, "northstar-logo.png");
  const logoId = await uploadFile(logoPath);
  currentId = (await postMultipart("pdf-with-added-image", { id: currentId, image_id: logoId, page: 1, x: 54, y: 716, width: 200, tag_alt_text: "Northstar Sample Supply logo", tag_structure_type: "Figure", tag_enabled: true, tag_language: "en-US" })).outputId;
  const pageCount = Number((await postMultipart("pdf-info", { id: currentId, queries: "page_count" })).page_count);
  currentId = (await postMultipart("pdf-with-added-shapes", { id: currentId, shape_objects: JSON.stringify(shapeObjects(style, pageCount, true)), tag_enabled: true })).outputId;
  const footer = [];
  const add = (text, y, size, structure = "P", bold = false) => footer.push({ font: bold ? style.boldFont : style.bodyFont, max_width: 480, opacity: "1", page: String(pageCount), rotation: "0", text, text_color_rgb: rgb(style.mutedTextColorRgb), text_size: size, x: 66, y, tag_structure_type: structure });
  add("Payment terms", 156, 8, "H2", true); add(metadata.paymentTerms, 142, 7.5); add("Notes", 112, 8, "H2", true); add(metadata.notes, 98, 7.5);
  for (let page = 1; page <= pageCount; page += 1) { add(`Generated from structured JSON and CSV input with pdfRest.`, 54, 7.5); footer[footer.length - 1].page = String(page); footer[footer.length - 1].x = 54; footer[footer.length - 1].max_width = 400; add(`Page ${page} of ${pageCount}`, 54, 7.5); footer[footer.length - 1].page = String(page); footer[footer.length - 1].x = 490; footer[footer.length - 1].max_width = 68; }
  const final = await postMultipart("pdf-with-added-text", { id: currentId, text_objects: JSON.stringify(footer), tag_enabled: true, tag_language: "en-US", output: "invoice_from_structured_data" });
  const file = await axios.get(`${apiUrl}/resource/${final.outputId}?format=file`, { headers: { "Api-Key": apiKey }, responseType: "arraybuffer" });
  fs.writeFileSync(outputPath, file.data);
  console.log(`Created ${outputPath}`);
}

main().catch((error) => { console.error(error.response?.data || error.message); process.exitCode = 1; });
