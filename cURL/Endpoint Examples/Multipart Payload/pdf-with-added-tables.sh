#!/bin/sh

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# This example adds an accessible project-status table with a header, colored status cells, and a footer.
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

curl -X POST "$API_URL/pdf-with-added-tables" \
  -H "Accept: application/json" \
  -H "Content-Type: multipart/form-data" \
  -H "Api-Key: xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  -F "file=@/path/to/input.pdf" \
  -F "table_objects=$TABLE_OBJECTS" \
  -F "tag_enabled=true" \
  -F "tag_language=en-US" \
  -F "output=project-status"
