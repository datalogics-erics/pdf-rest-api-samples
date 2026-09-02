#!/bin/sh

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# This sample converts an Email (.eml) file to PDF. It uploads the Email file
# first, then calls /pdf with its resource ID.
INPUT_PATH="/path/to/sample.eml"

INPUT_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --header "Content-Filename: $(basename "$INPUT_PATH")" \
  --header "Content-Type: application/octet-stream" \
  --data-binary "@$INPUT_PATH" | jq -r ".files[0].id")
if [ -z "$INPUT_ID" ] || [ "$INPUT_ID" = "null" ]; then echo "Input upload failed" >&2; exit 1; fi
PAYLOAD=$(jq -n --arg id "$INPUT_ID" \
  '{"id": $id, "output": "pdf_from_email"}')
curl --location "$API_URL/pdf" \
  --header "Accept: application/json" \
  --header "Content-Type: application/json" \
  --header "Api-Key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --data "$PAYLOAD"
