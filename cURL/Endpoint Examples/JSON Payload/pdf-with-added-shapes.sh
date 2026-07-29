#!/bin/sh

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# Upload the input PDF first, then use its resource ID in a JSON endpoint call.
UPLOAD_ID=$(curl --location "$API_URL/upload" \
  --header 'Api-Key: xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' \
  --header 'Content-Filename: input.pdf' \
  --data-binary '@/path/to/input.pdf' | jq -r '.files[0].id')

echo "File successfully uploaded with an ID of: $UPLOAD_ID"

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

jq -n --arg id "$UPLOAD_ID" --argjson shapes "$SHAPE_OBJECTS" '{id: $id, shape_objects: $shapes, tag_enabled: true, output: "review-panel"}' |
  curl --location "$API_URL/pdf-with-added-shapes" \
    --header 'Accept: application/json' \
    --header 'Api-Key: xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' \
    --header 'Content-Type: application/json' \
    --data-binary @- | jq -r '.'
