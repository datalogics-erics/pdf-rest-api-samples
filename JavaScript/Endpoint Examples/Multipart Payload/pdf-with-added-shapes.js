/**
 * Add a review-panel background and divider line to a PDF.
 * PDF coordinates begin at the lower-left corner; 72 PDF units equal one inch.
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

const shapes = [
  {
    type: "rectangle",
    page: 1,
    x: 54,
    y: 540,
    width: 504,
    height: 108,
    fill_color_rgb: "245,247,250",
    stroke_color_rgb: "26,72,112",
    stroke_width: 1,
    tag_is_artifact: true,
  },
  {
    type: "line",
    page: 1,
    x1: 72,
    y1: 576,
    x2: 540,
    y2: 576,
    stroke_color_rgb: "26,72,112",
    stroke_width: 1.5,
    tag_actual_text: "Review section divider",
    tag_structure_type: "Figure",
  },
];

const form = new FormData();
form.append("file", fs.createReadStream("/path/to/input.pdf"));
form.append("shape_objects", JSON.stringify(shapes));
form.append("tag_enabled", "true");
form.append("output", "review-panel");

axios({
  method: "post",
  maxBodyLength: Infinity,
  url: `${apiUrl}/pdf-with-added-shapes`,
  headers: {
    "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // Replace with your API key.
    ...form.getHeaders(),
  },
  data: form,
})
  .then((response) => console.log(JSON.stringify(response.data, null, 2)))
  .catch((error) => console.log(error.response?.data || error.message));

// To download the returned file, use the outputId with the get-resource-id endpoint sample.
