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

curl --location "$API_URL/pdf" \
  --header "Accept: application/json" \
  --header "Content-Type: multipart/form-data" \
  --header "Api-Key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --form "file=@$INPUT_PATH;type=application/postscript" \
  --form "job_options=@$JOB_OPTIONS_PATH;type=application/octet-stream" \
  --form "output=pdf_from_postscript"
