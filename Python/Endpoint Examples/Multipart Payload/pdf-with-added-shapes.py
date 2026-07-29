import json

import requests
from requests_toolbelt import MultipartEncoder

# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# api_url = "https://eu-api.pdfrest.com"

# Add a lightly shaded review panel and a divider line to page one.
# Coordinates are measured from the lower-left corner in PDF units (72 units = 1 inch).
shape_objects = [
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
        "tag_is_artifact": True,
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
        "tag_structure_type": "Figure",
    },
]

with open("/path/to/input.pdf", "rb") as input_file:
    multipart = MultipartEncoder(
        fields={
            "file": ("input.pdf", input_file, "application/pdf"),
            "shape_objects": json.dumps(shape_objects),
            "tag_enabled": "true",
            "output": "review-panel",
        }
    )
    response = requests.post(
        f"{api_url}/pdf-with-added-shapes",
        data=multipart,
        headers={
            "Accept": "application/json",
            "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",  # Replace with your API key.
            "Content-Type": multipart.content_type,
        },
    )

print(f"Response status code: {response.status_code}")
print(json.dumps(response.json(), indent=2) if response.ok else response.text)

# To download the returned file, use the outputId with the get-resource-id endpoint sample.
