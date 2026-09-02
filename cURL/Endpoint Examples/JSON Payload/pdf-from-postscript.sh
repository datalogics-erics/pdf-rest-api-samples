#!/bin/sh

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# This sample converts a PostScript (.ps) file to PDF with a custom .joboptions profile.
# A .joboptions file contains Adobe Distiller-compatible conversion settings. The
# profile is optional; omit job_options to use default settings. pdfRest applies a
# supplied profile with Datalogics PDF Converter SDK. Datalogics maintains the SDK
# in partnership with Adobe, using the same Adobe technology that powers Distiller.
# Pairing this /pdf call with /postscript creates a PDF -> PostScript -> PDF workflow
# commonly called PDF refrying. Some print and prepress workflows use it to rebuild or
# normalize page content, but the lossy roundtrip can discard PDF-specific features.
INPUT_PATH="/path/to/sample.ps"
JOB_OPTIONS_PATH="/path/to/custom.joboptions"

INPUT_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --header "Content-Filename: $(basename "$INPUT_PATH")" \
  --header "Content-Type: application/octet-stream" \
  --data-binary "@$INPUT_PATH" | jq -r ".files[0].id")
if [ -z "$INPUT_ID" ] || [ "$INPUT_ID" = "null" ]; then echo "PostScript upload failed" >&2; exit 1; fi
JOB_OPTIONS_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --header "Content-Filename: $(basename "$JOB_OPTIONS_PATH")" \
  --header "Content-Type: application/octet-stream" \
  --data-binary "@$JOB_OPTIONS_PATH" | jq -r ".files[0].id")
if [ -z "$JOB_OPTIONS_ID" ] || [ "$JOB_OPTIONS_ID" = "null" ]; then echo "Job options upload failed" >&2; exit 1; fi
PAYLOAD=$(jq -n --arg id "$INPUT_ID" --arg job_options_id "$JOB_OPTIONS_ID" \
  '{"id": $id, "job_options_id": $job_options_id, "output": "pdf_from_postscript"}')
curl --location "$API_URL/pdf" \
  --header "Accept: application/json" \
  --header "Content-Type: application/json" \
  --header "Api-Key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --data "$PAYLOAD"
