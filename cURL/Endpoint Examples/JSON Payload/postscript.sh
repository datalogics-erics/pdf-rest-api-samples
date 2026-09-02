#!/bin/sh

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# This sample uploads a PDF, then converts it through the JSON /postscript flow.
# Pairing this endpoint with /pdf creates a PDF -> PostScript -> PDF workflow commonly
# called PDF refrying. Some print and prepress workflows use it to rebuild, flatten, or
# normalize page content, but the lossy roundtrip can discard PDF-specific features.
# These settings request Level 3, text-safe output, all pages at original scale,
# shrink-to-fit without rotation, and printable annotations.
INPUT_PATH="/path/to/sample.pdf"

INPUT_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --header "Content-Filename: $(basename "$INPUT_PATH")" \
  --header "Content-Type: application/octet-stream" \
  --data-binary "@$INPUT_PATH" | jq -r ".files[0].id")
if [ -z "$INPUT_ID" ] || [ "$INPUT_ID" = "null" ]; then echo "PDF upload failed" >&2; exit 1; fi
PAYLOAD=$(jq -n --arg id "$INPUT_ID" \
  '{"id": $id, "ps_level": 3, "page_range": "all", "binary_output": false,
    "scale": 1, "rotate": false, "shrink_to_fit": true,
    "print_annotations": true, "output": "postscript_from_pdf"}')
curl --location "$API_URL/postscript" \
  --header "Accept: application/json" \
  --header "Content-Type: application/json" \
  --header "Api-Key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --data "$PAYLOAD"
