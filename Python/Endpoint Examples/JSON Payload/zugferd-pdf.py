import json
import os
import requests

# Upload invoice XML and PDF, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
# pdfRest preserves the supplied PDF when it agrees with the canonical XML.
api_url = "https://api.pdfrest.com"
# api_url = "https://eu-api.pdfrest.com"  # EU/GDPR service
api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
invoice_xml = "/path/to/invoice.xml"
invoice_pdf = "/path/to/invoice.pdf"

with open(invoice_xml, "rb") as source:
    upload = requests.post(f"{api_url}/upload", data=source, headers={"Api-Key": api_key, "Content-Type": "application/xml", "Content-Filename": os.path.basename(invoice_xml)})
upload.raise_for_status()

with open(invoice_pdf, "rb") as source:
    pdf_upload = requests.post(f"{api_url}/upload", data=source, headers={"Api-Key": api_key, "Content-Type": "application/pdf", "Content-Filename": os.path.basename(invoice_pdf)})
pdf_upload.raise_for_status()

# Fallback generation handles a mismatch or an unconfirmed PDF/XML match.
# The render options style only that replacement PDF, not a preserved supplied PDF.
response = requests.post(f"{api_url}/zugferd-pdf", json={
    "id": upload.json()["files"][0]["id"], "pdf_id": pdf_upload.json()["files"][0]["id"], "regenerate_pdf": True, "output": "zugferd_invoice",
    "render_options": {"locale": "de-DE", "label_language": "de", "font": "arial", "bold_font": "arialbold", "accent_color_rgb": [0, 92, 171]},
}, headers={"Accept": "application/json", "Api-Key": api_key})
print(json.dumps(response.json(), indent=2))
response.raise_for_status()
