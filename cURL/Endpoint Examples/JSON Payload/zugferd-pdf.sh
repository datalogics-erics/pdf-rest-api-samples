#!/bin/sh

# Upload invoice XML, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
API_URL="https://api.pdfrest.com"
# API_URL="https://eu-api.pdfrest.com" # EU/GDPR service
API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key
INVOICE_XML="/path/to/invoice.xml"

XML_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Filename: $(basename "$INVOICE_XML")" \
  --header "Content-Type: application/xml" \
  --data-binary "@$INVOICE_XML" | jq -r '.files[0].id')

test -n "$XML_ID" && test "$XML_ID" != "null" || { echo "XML upload failed" >&2; exit 1; }

curl --location "$API_URL/zugferd-pdf" \
  --header "Accept: application/json" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Type: application/json" \
  --data "{\"id\":\"$XML_ID\",\"output\":\"zugferd_invoice\",\"render_options\":{\"locale\":\"de-DE\",\"label_language\":\"de\",\"font\":\"arial\",\"bold_font\":\"arialbold\",\"accent_color_rgb\":[0,92,171]}}"
