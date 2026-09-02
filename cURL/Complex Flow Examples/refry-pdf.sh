#!/bin/sh

# Refry a PDF by converting it to PostScript and back to PDF.
#
# PDF refrying is the common name for a PDF -> PostScript -> PDF roundtrip.
# Some print, prepress, and legacy production workflows use it to rebuild or
# normalize page content, flatten certain PDF constructs, or prepare a file
# for downstream systems. The process is intentionally lossy and may remove
# tags, forms, layers, annotations, transparency, metadata, and editability.
# Use this workflow when a downstream system requires rebuilt page content or
# a PostScript-based interchange file.
#
# The PostScript-to-PDF step uses a custom .joboptions profile in this sample.
# A .joboptions file contains Adobe Distiller-compatible conversion settings;
# it is optional, and default settings are used when omitted. pdfRest applies
# the profile with Datalogics PDF Converter SDK. Datalogics maintains the SDK
# in partnership with Adobe, using the same Adobe technology that powers
# Distiller.
#
# Run: sh refry-pdf.sh <pdf> <jobOptions> [outputPdf]

set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
API_URL="${PDFREST_URL:-https://api.pdfrest.com}"
API_KEY="${PDFREST_API_KEY:-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}"
INPUT_PATH="${1:-/path/to/sample.pdf}"
JOB_OPTIONS_PATH="${2:-/path/to/custom.joboptions}"
OUTPUT_PATH="${3:-$SCRIPT_DIR/refried.pdf}"

POSTSCRIPT_RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/postscript" \
  --header 'Accept: application/json' \
  --header 'Content-Type: multipart/form-data' \
  --header "Api-Key: $API_KEY" \
  --form "file=@$INPUT_PATH;type=application/pdf" \
  --form 'ps_level=3' \
  --form 'page_range=all' \
  --form 'binary_output=true' \
  --form 'scale=1' \
  --form 'rotate=false' \
  --form 'shrink_to_fit=true' \
  --form 'print_annotations=true' \
  --form 'output=refry_intermediate')
POSTSCRIPT_ID=$(printf '%s' "$POSTSCRIPT_RESPONSE" | jq -r '.outputId')

FINAL_RESPONSE=$(curl --fail-with-body --silent --show-error --location "$API_URL/pdf" \
  --header 'Accept: application/json' \
  --header 'Content-Type: multipart/form-data' \
  --header "Api-Key: $API_KEY" \
  --form "id=$POSTSCRIPT_ID" \
  --form "job_options=@$JOB_OPTIONS_PATH;type=application/octet-stream" \
  --form 'output=refried')
FINAL_ID=$(printf '%s' "$FINAL_RESPONSE" | jq -r '.outputId')

curl --fail-with-body --silent --show-error --location \
  "$API_URL/resource/$FINAL_ID?format=file" \
  --header "Api-Key: $API_KEY" \
  --output "$OUTPUT_PATH"
printf '%s\n' "$FINAL_RESPONSE" | jq .
echo "Created $OUTPUT_PATH"
