#!/bin/sh

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# This example adds a lightly shaded review panel and a divider line to page one.
# Coordinates are measured from the lower-left corner in PDF units (72 units = 1 inch).
SHAPE_OBJECTS='[
  {
    "type": "rectangle",
    "page": 1,
    "x": 54,
    "y": 540,
    "width": 504,
    "height": 108,
    "fill_color_rgb": "245,247,250",
    "stroke_color_rgb": "26,72,112",
    "stroke_width": 1,
    "tag_is_artifact": true
  },
  {
    "type": "line",
    "page": 1,
    "x1": 72,
    "y1": 576,
    "x2": 540,
    "y2": 576,
    "stroke_color_rgb": "26,72,112",
    "stroke_width": 1.5,
    "tag_actual_text": "Review section divider",
    "tag_structure_type": "Figure"
  }
]'

curl -X POST "$API_URL/pdf-with-added-shapes" \
  -H "Accept: application/json" \
  -H "Content-Type: multipart/form-data" \
  -H "Api-Key: xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  -F "file=@/path/to/input.pdf" \
  -F "shape_objects=$SHAPE_OBJECTS" \
  -F "tag_enabled=true" \
  -F "output=review-panel"
