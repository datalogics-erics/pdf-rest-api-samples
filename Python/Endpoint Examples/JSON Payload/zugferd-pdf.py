import json
import os
import requests

# Upload invoice XML, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
api_url = "https://api.pdfrest.com"
# api_url = "https://eu-api.pdfrest.com"  # EU/GDPR service
api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
invoice_xml = "/path/to/invoice.xml"

with open(invoice_xml, "rb") as source:
    upload = requests.post(f"{api_url}/upload", data=source, headers={"Api-Key": api_key, "Content-Type": "application/xml", "Content-Filename": os.path.basename(invoice_xml)})
upload.raise_for_status()

response = requests.post(f"{api_url}/zugferd-pdf", json={
    "id": upload.json()["files"][0]["id"], "output": "zugferd_invoice",
    "render_options": {"locale": "de-DE", "label_language": "de", "font": "arial", "bold_font": "arialbold", "accent_color_rgb": [0, 92, 171]},
}, headers={"Accept": "application/json", "Api-Key": api_key})
print(json.dumps(response.json(), indent=2))
response.raise_for_status()
