#!/bin/sh

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="${PDFREST_URL:-https://api.pdfrest.com}"

# This sample converts XML input to a tagged PDF through multipart /pdf.
INPUT_PATH="${1:-/path/to/sample.xml}"
API_KEY="${PDFREST_API_KEY:-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}"
OPTIONS='{"title":"Structured Content Sample","language":"en-US","enable_tagging":true,"page_setup":{"size":"Letter","orientation":"portrait","margin":{"top":36,"right":42,"bottom":36,"left":42}},"style":{"font":"Arial","heading_font":"Arial","code_font":"Courier","text_size":11,"text_color_rgb":[34,34,34],"heading_scale":1.35,"table":{"column_width_weights":[2,3,2],"keep_header_with_first_row":true,"repeat_headers_on_overflow":true,"show_borders":true,"border_width":0.75,"border_color_rgb":[180,188,200],"header_fill_color_rgb":[33,64,98],"header_text_color_rgb":[255,255,255],"row_fill_color_rgb":[250,250,252],"alternate_row_fill_color_rgb":[235,240,246],"cell_padding":{"top":6,"right":8,"bottom":6,"left":8}}},"data_presentation":"hierarchy"}'

curl --location "$API_URL/pdf" \
  --header "Accept: application/json" \
  --header "Content-Type: multipart/form-data" \
  --header "Api-Key: $API_KEY" \
  --form "file=@$INPUT_PATH" \
  --form "structured_text_options=$OPTIONS" \
  --form "output=pdf_from_xml"

