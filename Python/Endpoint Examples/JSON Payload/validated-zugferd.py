import json
import os
import requests

# Upload a hybrid PDF, then validate its ZUGFeRD / Factur-X package by resource ID.
api_url = "https://api.pdfrest.com"
# api_url = "https://eu-api.pdfrest.com"  # EU/GDPR service
api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
zugferd_pdf = "/path/to/zugferd-invoice.pdf"

with open(zugferd_pdf, "rb") as source:
    upload = requests.post(f"{api_url}/upload", data=source, headers={"Api-Key": api_key, "Content-Type": "application/pdf", "Content-Filename": os.path.basename(zugferd_pdf)})
upload.raise_for_status()

response = requests.post(f"{api_url}/validated-zugferd", json={"id": upload.json()["files"][0]["id"]}, headers={"Accept": "application/json", "Api-Key": api_key})
print(json.dumps(response.json(), indent=2))
response.raise_for_status()
