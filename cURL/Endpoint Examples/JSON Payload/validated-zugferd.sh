#!/bin/sh

# Upload a hybrid PDF, then validate its ZUGFeRD / Factur-X package by resource ID.
API_URL="https://api.pdfrest.com"
# API_URL="https://eu-api.pdfrest.com" # EU/GDPR service
API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key
ZUGFERD_PDF="/path/to/zugferd-invoice.pdf"

PDF_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Filename: $(basename "$ZUGFERD_PDF")" \
  --header "Content-Type: application/pdf" \
  --data-binary "@$ZUGFERD_PDF" | jq -r '.files[0].id')

test -n "$PDF_ID" && test "$PDF_ID" != "null" || { echo "PDF upload failed" >&2; exit 1; }

curl --location "$API_URL/validated-zugferd" \
  --header "Accept: application/json" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Type: application/json" \
  --data "{\"id\":\"$PDF_ID\"}"
