#!/bin/sh

# Create a ZUGFeRD / Factur-X PDF/A-3 invoice from XML and an existing invoice PDF.
# pdfRest preserves the supplied PDF when it agrees with the canonical XML.
API_URL="https://api.pdfrest.com"
# API_URL="https://eu-api.pdfrest.com" # EU/GDPR service
API_KEY="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" # Replace with your API key
INVOICE_XML="/path/to/invoice.xml"
INVOICE_PDF="/path/to/invoice.pdf"

# `regenerate_pdf` enables a fallback replacement PDF for a mismatch or unconfirmed match.
# `render_options` style that fallback PDF only; they do not alter a preserved supplied PDF.
curl --location "$API_URL/zugferd-pdf" \
  --header "Accept: application/json" \
  --header "Api-Key: $API_KEY" \
  --form "file=@$INVOICE_XML;type=application/xml" \
  --form "pdf_file=@$INVOICE_PDF;type=application/pdf" \
  --form "regenerate_pdf=true" \
  --form 'render_options={"locale":"de-DE","label_language":"de","font":"arial","bold_font":"arialbold","accent_color_rgb":[0,92,171]}' \
  --form "output=zugferd_invoice"
