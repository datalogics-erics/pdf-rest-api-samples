import json

import requests
from requests_toolbelt import MultipartEncoder

# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# api_url = "https://eu-api.pdfrest.com"

# Add an accessible project-status table with a header, colored status cells, and a footer.
table_objects = [
    {
        "page": 1,
        "x": 54,
        "y": 540,
        "width": 504,
        "columns": [{"width": 210}, {"width": 144}, {"width": 150}],
        "tag_structure_type": "Table",
        "style": {
            "padding": {"top": 6, "right": 8, "bottom": 6, "left": 8},
            "text_size": 10,
        },
        "header_rows": [
            {
                "cells": [
                    {"text": "Milestone", "tag_structure_type": "TH", "style": {"background_color_rgb": [26, 72, 112], "text_color_rgb": [255, 255, 255]}},
                    {"text": "Owner", "tag_structure_type": "TH", "style": {"background_color_rgb": [26, 72, 112], "text_color_rgb": [255, 255, 255]}},
                    {"text": "Status", "tag_structure_type": "TH", "style": {"background_color_rgb": [26, 72, 112], "text_color_rgb": [255, 255, 255]}},
                ]
            }
        ],
        "rows": [
            {"cells": [{"text": "Requirements review"}, {"text": "Maya Chen"}, {"text": "Complete", "tag_structure_type": "TD", "style": {"background_color_rgb": [220, 252, 231]}}]},
            {"cells": [{"text": "Prototype delivery"}, {"text": "Jordan Lee"}, {"text": "In progress", "tag_structure_type": "TD", "style": {"background_color_rgb": [254, 249, 195]}}]},
            {"cells": [{"text": "Stakeholder approval"}, {"text": "Avery Patel"}, {"text": "Planned", "tag_structure_type": "TD", "style": {"background_color_rgb": [239, 246, 255]}}]},
        ],
        "footer_rows": [
            {"cells": [{"text": "Next review: Friday, 10:00 AM", "col_span": 3, "tag_structure_type": "TD", "style": {"background_color_rgb": [245, 247, 250], "text_color_rgb": [55, 65, 81]}}]}
        ],
    }
]

with open("/path/to/input.pdf", "rb") as input_file:
    multipart = MultipartEncoder(
        fields={
            "file": ("input.pdf", input_file, "application/pdf"),
            "table_objects": json.dumps(table_objects),
            "tag_enabled": "true",
            "tag_language": "en-US",
            "output": "project-status",
        }
    )
    response = requests.post(
        f"{api_url}/pdf-with-added-tables",
        data=multipart,
        headers={
            "Accept": "application/json",
            "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",  # Replace with your API key.
            "Content-Type": multipart.content_type,
        },
    )

print(f"Response status code: {response.status_code}")
print(json.dumps(response.json(), indent=2) if response.ok else response.text)
