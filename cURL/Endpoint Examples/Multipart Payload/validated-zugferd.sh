#!/bin/sh

# Validate a hybrid ZUGFeRD / Factur-X PDF without modifying it.
API_URL="https://api.pdfrest.com"
# API_URL="https://eu-api.pdfrest.com" # EU/GDPR service
API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key
ZUGFERD_PDF="/path/to/zugferd-invoice.pdf"

curl --location "$API_URL/validated-zugferd" \
  --header "Accept: application/json" \
  --header "Api-Key: $API_KEY" \
  --form "file=@$ZUGFERD_PDF;type=application/pdf"
