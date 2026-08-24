#!/bin/sh

# By default, we use the US-based API service. This is the primary endpoint for global use.
API_URL="${PDFREST_URL:-https://api.pdfrest.com}"

# This sample converts Markdown input to a tagged PDF through the JSON /pdf flow.
# It maps a Markdown image target to an uploaded image resource.
INPUT_PATH="${1:-/path/to/sample.md}"
IMAGE_PATH="${2:-/path/to/logo.png}"
API_KEY="${PDFREST_API_KEY:-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}"
OPTIONS='{"title":"Structured Content Sample","language":"en-US","enable_tagging":true,"page_setup":{"size":"Letter","orientation":"portrait","margin":{"top":36,"right":42,"bottom":36,"left":42}},"style":{"font":"Arial","heading_font":"Arial","code_font":"Courier","text_size":11,"text_color_rgb":[34,34,34],"heading_scale":1.35,"table":{"column_width_weights":[2,3,2],"keep_header_with_first_row":true,"repeat_headers_on_overflow":true,"show_borders":true,"border_width":0.75,"border_color_rgb":[180,188,200],"header_fill_color_rgb":[33,64,98],"header_text_color_rgb":[255,255,255],"row_fill_color_rgb":[250,250,252],"alternate_row_fill_color_rgb":[235,240,246],"cell_padding":{"top":6,"right":8,"bottom":6,"left":8}}},"markdown":{"image_alt_text":{"sample-logo":"Sample logo"},"missing_image_alt_text":"fail","image_sources":{"sample-logo":{"image_id_index":0}}}}'

INPUT_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Filename: $(basename "$INPUT_PATH")" \
  --header "Content-Type: application/octet-stream" \
  --data-binary "@$INPUT_PATH" | jq -r ".files[0].id")
if [ -z "$INPUT_ID" ] || [ "$INPUT_ID" = "null" ]; then echo "Input upload failed" >&2; exit 1; fi
IMAGE_ID=$(curl --silent --show-error --location "$API_URL/upload" \
  --header "Api-Key: $API_KEY" \
  --header "Content-Filename: $(basename "$IMAGE_PATH")" \
  --header "Content-Type: application/octet-stream" \
  --data-binary "@$IMAGE_PATH" | jq -r ".files[0].id")
if [ -z "$IMAGE_ID" ] || [ "$IMAGE_ID" = "null" ]; then echo "Image upload failed" >&2; exit 1; fi
PAYLOAD=$(jq -n --arg id "$INPUT_ID" --argjson options "$OPTIONS" --arg image_id "$IMAGE_ID" \
  '{"id": $id, "structured_text_options": $options, "image_ids": [$image_id]}')
curl --location "$API_URL/pdf" \
  --header "Accept: application/json" \
  --header "Content-Type: application/json" \
  --header "Api-Key: $API_KEY" \
  --data "$PAYLOAD"
