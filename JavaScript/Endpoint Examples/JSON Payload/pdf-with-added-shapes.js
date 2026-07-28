/**
 * Upload a PDF, then add a review-panel background and divider line by resource ID.
 * PDF coordinates begin at the lower-left corner; 72 PDF units equal one inch.
 */
const axios = require("axios");
const fs = require("fs");

// By default, we use the US-based API service. This is the primary endpoint for global use.
const apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
// const apiUrl = "https://eu-api.pdfrest.com";

const apiKey = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"; // Replace with your API key.
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

async function addShapes() {
  try {
    const uploadResponse = await axios({
      method: "post",
      maxBodyLength: Infinity,
      url: `${apiUrl}/upload`,
      headers: {
        "Api-Key": apiKey,
        "Content-Filename": "input.pdf",
        "Content-Type": "application/octet-stream",
      },
      data: fs.createReadStream("/path/to/input.pdf"),
    });

    const inputId = uploadResponse.data.files[0].id;
    const response = await axios.post(
      `${apiUrl}/pdf-with-added-shapes`,
      { id: inputId, shape_objects: shapes, tag_enabled: true, output: "review-panel" },
      { headers: { "Api-Key": apiKey } },
    );
    console.log(JSON.stringify(response.data, null, 2));
  } catch (error) {
    console.log(error.response?.data || error.message);
  }
}

addShapes();
