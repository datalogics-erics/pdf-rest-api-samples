/**
 * Add an accessible, styled project-status table to a PDF.
 * The table includes header cells, colored status cells, and a footer row.
 */
const axios = require("axios");
const FormData = require("form-data");
const fs = require("fs");

// By default, we use the US-based API service. This is the primary endpoint for global use.
const apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
// const apiUrl = "https://eu-api.pdfrest.com";

const tables = [
  {
    page: 1,
    x: 54,
    y: 540,
    width: 504,
    columns: [{ width: 210 }, { width: 144 }, { width: 150 }],
    tag_structure_type: "Table",
    style: {
      padding: { top: 6, right: 8, bottom: 6, left: 8 },
      text_size: 10,
    },
    header_rows: [
      {
        cells: [
          { text: "Milestone", tag_structure_type: "TH", style: { background_color_rgb: [26, 72, 112], text_color_rgb: [255, 255, 255] } },
          { text: "Owner", tag_structure_type: "TH", style: { background_color_rgb: [26, 72, 112], text_color_rgb: [255, 255, 255] } },
          { text: "Status", tag_structure_type: "TH", style: { background_color_rgb: [26, 72, 112], text_color_rgb: [255, 255, 255] } },
        ],
      },
    ],
    rows: [
      { cells: [{ text: "Requirements review" }, { text: "Maya Chen" }, { text: "Complete", tag_structure_type: "TD", style: { background_color_rgb: [220, 252, 231] } }] },
      { cells: [{ text: "Prototype delivery" }, { text: "Jordan Lee" }, { text: "In progress", tag_structure_type: "TD", style: { background_color_rgb: [254, 249, 195] } }] },
      { cells: [{ text: "Stakeholder approval" }, { text: "Avery Patel" }, { text: "Planned", tag_structure_type: "TD", style: { background_color_rgb: [239, 246, 255] } }] },
    ],
    footer_rows: [
      { cells: [{ text: "Next review: Friday, 10:00 AM", col_span: 3, tag_structure_type: "TD", style: { background_color_rgb: [245, 247, 250], text_color_rgb: [55, 65, 81] } }] },
    ],
  },
];

const form = new FormData();
form.append("file", fs.createReadStream("/path/to/input.pdf"));
form.append("table_objects", JSON.stringify(tables));
form.append("tag_enabled", "true");
form.append("tag_language", "en-US");
form.append("output", "project-status");

axios({
  method: "post",
  maxBodyLength: Infinity,
  url: `${apiUrl}/pdf-with-added-tables`,
  headers: {
    "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // Replace with your API key.
    ...form.getHeaders(),
  },
  data: form,
})
  .then((response) => console.log(JSON.stringify(response.data, null, 2)))
  .catch((error) => console.log(error.response?.data || error.message));

// To download the returned file, use the outputId with the get-resource-id endpoint sample.
