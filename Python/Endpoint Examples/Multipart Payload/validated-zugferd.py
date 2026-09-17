import os
import requests
from requests_toolbelt import MultipartEncoder

# Validate a hybrid ZUGFeRD / Factur-X PDF without modifying it.
api_url = "https://api.pdfrest.com"
# api_url = "https://eu-api.pdfrest.com"  # EU/GDPR service
api_key = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
zugferd_pdf = "/path/to/zugferd-invoice.pdf"

with open(zugferd_pdf, "rb") as source:
    form = MultipartEncoder(fields={"file": (os.path.basename(zugferd_pdf), source, "application/pdf")})
    response = requests.post(f"{api_url}/validated-zugferd", data=form, headers={"Accept": "application/json", "Content-Type": form.content_type, "Api-Key": api_key})

print(response.text)
response.raise_for_status()
