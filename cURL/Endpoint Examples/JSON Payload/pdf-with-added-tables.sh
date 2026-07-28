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

TABLE_OBJECTS='[
  {
    "page": 1,
    "x": 54,
    "y": 540,
    "width": 504,
    "columns": [{"width": 210}, {"width": 144}, {"width": 150}],
    "tag_structure_type": "Table",
    "style": {"padding": {"top": 6, "right": 8, "bottom": 6, "left": 8}, "text_size": 10},
    "header_rows": [{"cells": [
      {"text": "Milestone", "tag_structure_type": "TH", "style": {"background_color_rgb": [26,72,112], "text_color_rgb": [255,255,255]}},
      {"text": "Owner", "tag_structure_type": "TH", "style": {"background_color_rgb": [26,72,112], "text_color_rgb": [255,255,255]}},
      {"text": "Status", "tag_structure_type": "TH", "style": {"background_color_rgb": [26,72,112], "text_color_rgb": [255,255,255]}}
    ]}],
    "rows": [
      {"cells": [{"text": "Requirements review"}, {"text": "Maya Chen"}, {"text": "Complete", "tag_structure_type": "TD", "style": {"background_color_rgb": [220,252,231]}}]},
      {"cells": [{"text": "Prototype delivery"}, {"text": "Jordan Lee"}, {"text": "In progress", "tag_structure_type": "TD", "style": {"background_color_rgb": [254,249,195]}}]},
      {"cells": [{"text": "Stakeholder approval"}, {"text": "Avery Patel"}, {"text": "Planned", "tag_structure_type": "TD", "style": {"background_color_rgb": [239,246,255]}}]}
    ],
    "footer_rows": [{"cells": [
      {"text": "Next review: Friday, 10:00 AM", "col_span": 3, "tag_structure_type": "TD", "style": {"background_color_rgb": [245,247,250], "text_color_rgb": [55,65,81]}}
    ]}]
  }
]'

jq -n --arg id "$UPLOAD_ID" --argjson tables "$TABLE_OBJECTS" '{id: $id, table_objects: $tables, tag_enabled: true, tag_language: "en-US", output: "project-status"}' |
  curl --location "$API_URL/pdf-with-added-tables" \
    --header 'Accept: application/json' \
    --header 'Api-Key: xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' \
    --header 'Content-Type: application/json' \
    --data-binary @- | jq -r '.'
