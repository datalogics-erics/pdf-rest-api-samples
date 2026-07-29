import json

import requests

# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# api_url = "https://eu-api.pdfrest.com"

api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"  # Replace with your API key.
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

# Upload the input PDF first, then use its resource ID in a JSON endpoint call.
with open("/path/to/input.pdf", "rb") as input_file:
    upload_response = requests.post(
        f"{api_url}/upload",
        data=input_file,
        headers={
            "Api-Key": api_key,
            "Content-Filename": "input.pdf",
            "Content-Type": "application/octet-stream",
        },
    )

print(f"Upload response status code: {upload_response.status_code}")
if not upload_response.ok:
    print(upload_response.text)
else:
    input_id = upload_response.json()["files"][0]["id"]
    response = requests.post(
        f"{api_url}/pdf-with-added-shapes",
        json={
            "id": input_id,
            "shape_objects": shape_objects,
            "tag_enabled": True,
            "output": "review-panel",
        },
        headers={"Accept": "application/json", "Api-Key": api_key},
    )
    print(f"Processing response status code: {response.status_code}")
    print(json.dumps(response.json(), indent=2) if response.ok else response.text)
