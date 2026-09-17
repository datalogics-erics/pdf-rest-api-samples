#!/bin/sh

# Create a ZUGFeRD / Factur-X PDF/A-3 invoice from an invoice XML file.
API_URL="https://api.pdfrest.com"
# API_URL="https://eu-api.pdfrest.com" # EU/GDPR service
API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key
INVOICE_XML="/path/to/invoice.xml"

curl --location "$API_URL/zugferd-pdf" \
  --header "Accept: application/json" \
  --header "Api-Key: $API_KEY" \
  --form "file=@$INVOICE_XML;type=application/xml" \
  --form 'render_options={"locale":"de-DE","label_language":"de","font":"arial","bold_font":"arialbold","accent_color_rgb":[0,92,171]}' \
  --form "output=zugferd_invoice"
