import json
import os
import requests

# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

# This sample uploads XML input, then calls /pdf with a JSON payload.
# It demonstrates structured_text_options and the format-specific conversion options.
input_path = "/path/to/sample.xml"

def upload(path):
    with open(path, "rb") as source:
        response = requests.post(api_url + "/upload", data=source, headers={
            "Content-Type": "application/octet-stream",
            "Content-Filename": os.path.basename(path),
            "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
        })
    if not response.ok:
        print(response.text)
        raise SystemExit(1)
    return response.json()["files"][0]["id"]

input_id = upload(input_path)
payload = {
    "id": input_id,
    "structured_text_options": json.loads(r'''{
  "title": "Structured Content Sample",
  "language": "en-US",
  "enable_tagging": true,
  "page_setup": {
    "size": "Letter",
    "orientation": "portrait",
    "margin": {
      "top": 36,
      "right": 42,
      "bottom": 36,
      "left": 42
    }
  },
  "style": {
    "font": "Arial",
    "heading_font": "Arial",
    "code_font": "Courier",
    "text_size": 11,
    "text_color_rgb": [
      34,
      34,
      34
    ],
    "heading_scale": 1.35,
    "table": {
      "column_width_weights": [
        2,
        3,
        2
      ],
      "keep_header_with_first_row": true,
      "repeat_headers_on_overflow": true,
      "show_borders": true,
      "border_width": 0.75,
      "border_color_rgb": [
        180,
        188,
        200
      ],
      "header_fill_color_rgb": [
        33,
        64,
        98
      ],
      "header_text_color_rgb": [
        255,
        255,
        255
      ],
      "row_fill_color_rgb": [
        250,
        250,
        252
      ],
      "alternate_row_fill_color_rgb": [
        235,
        240,
        246
      ],
      "cell_padding": {
        "top": 6,
        "right": 8,
        "bottom": 6,
        "left": 8
      }
    }
  },
  "data_presentation": "hierarchy"
}'''),
}
response = requests.post(api_url + "/pdf", json=payload, headers={
    "Accept": "application/json",
    "Content-Type": "application/json",
    "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
})

print("Response status code: " + str(response.status_code))
if response.ok:
    print(json.dumps(response.json(), indent=2))
else:
    print(response.text)
    raise SystemExit(1)

