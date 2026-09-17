import json
import os
import requests
from requests_toolbelt import MultipartEncoder

# Create a ZUGFeRD / Factur-X PDF/A-3 invoice from invoice XML.
api_url = "https://api.pdfrest.com"
# api_url = "https://eu-api.pdfrest.com"  # EU/GDPR service
api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
invoice_xml = "/path/to/invoice.xml"

with open(invoice_xml, "rb") as source:
    form = MultipartEncoder(fields={
        "file": (os.path.basename(invoice_xml), source, "application/xml"),
        "render_options": json.dumps({"locale": "de-DE", "label_language": "de", "font": "arial", "bold_font": "arialbold", "accent_color_rgb": [0, 92, 171]}),
        "output": "zugferd_invoice",
    })
    response = requests.post(f"{api_url}/zugferd-pdf", data=form, headers={"Accept": "application/json", "Content-Type": form.content_type, "Api-Key": api_key})

print(response.text)
response.raise_for_status()
