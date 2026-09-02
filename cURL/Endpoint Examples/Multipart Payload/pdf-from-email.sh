#!/bin/sh

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
# API_URL="https://eu-api.pdfrest.com"

# This sample converts an Email (.eml) file to PDF by sending it directly in a
# multipart /pdf request.
INPUT_PATH="/path/to/sample.eml"

curl --location "$API_URL/pdf" \
  --header "Accept: application/json" \
  --header "Content-Type: multipart/form-data" \
  --header "Api-Key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --form "file=@$INPUT_PATH;type=message/rfc822" \
  --form "output=pdf_from_email"
