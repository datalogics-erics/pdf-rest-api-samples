#!/bin/sh

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# This sample converts PDF to PostScript through multipart /postscript.
# Pairing this endpoint with /pdf creates a PDF -> PostScript -> PDF workflow commonly
# called PDF refrying. Some print and prepress workflows use it to rebuild, flatten, or
# normalize page content, but the lossy roundtrip can discard PDF-specific features.
# These settings request Level 3, text-safe output, all pages at original scale,
# shrink-to-fit without rotation, and printable annotations.
INPUT_PATH="/path/to/sample.pdf"

curl --location "$API_URL/postscript" \
  --header "Accept: application/json" \
  --header "Content-Type: multipart/form-data" \
  --header "Api-Key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --form "file=@$INPUT_PATH;type=application/pdf" \
  --form "ps_level=3" \
  --form "page_range=all" \
  --form "binary_output=false" \
  --form "scale=1" \
  --form "rotate=false" \
  --form "shrink_to_fit=true" \
  --form "print_annotations=true" \
  --form "output=postscript_from_pdf"
