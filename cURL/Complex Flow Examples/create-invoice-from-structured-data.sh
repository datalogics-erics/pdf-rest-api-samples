#!/bin/sh

# Generate a tagged invoice from JSON and CSV input using pdfRest.
#
# Set PDFREST_API_KEY before running this sample. It reads metadata.json,
# style.json, and line-items.csv from invoice-data, then downloads the PDF
# beside this script. Requires curl and jq. Run with:
#   sh create-invoice-from-structured-data.sh

set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
DATA_DIR="$SCRIPT_DIR/invoice-data"
OUTPUT_PATH="$SCRIPT_DIR/invoice-from-structured-data.pdf"
API_URL="${PDFREST_URL:-https://api.pdfrest.com}"
API_KEY="${PDFREST_API_KEY:-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}"

post_json() {
  curl --fail-with-body --silent --show-error --location "$API_URL/$1" \
    --header 'Accept: application/json' --header "Api-Key: $API_KEY" \
    --header 'Content-Type: application/json' --data-binary "$2"
}

post_multipart() {
  endpoint=$1
  shift
  curl --fail-with-body --silent --show-error --location "$API_URL/$endpoint" \
    --header 'Accept: application/json' --header "Api-Key: $API_KEY" "$@"
}

METADATA=$(cat "$DATA_DIR/metadata.json")
STYLE=$(cat "$DATA_DIR/style.json")

# The bundled descriptions are quoted CSV fields, so jq's capture expression
# preserves commas inside a description while converting rows to JSON.
ITEMS=$(jq -R -s 'split("\n") | .[1:] | map(select(length > 0) | capture("^(?<itemCode>[^,]+),\\\"(?<description>.*)\\\",(?<quantity>[^,]+),(?<unitPrice>[^,]+)$"))' "$DATA_DIR/line-items.csv")

BLANK=$(post_json blank-pdf '{"page_size":"letter","page_count":1,"page_orientation":"portrait"}')
CURRENT_ID=$(printf '%s' "$BLANK" | jq -r '.outputId')

HEADER_SHAPES=$(jq -n --argjson style "$STYLE" '[
  {type:"rectangle",page:1,x:54,y:540,width:240,height:96,fill_color_rgb:($style.accentColorRgb | join(",")),stroke_color_rgb:($style.borderColorRgb | join(",")),stroke_width:0.5,tag_is_artifact:true},
  {type:"rectangle",page:1,x:318,y:540,width:240,height:96,fill_color_rgb:($style.accentColorRgb | join(",")),stroke_color_rgb:($style.borderColorRgb | join(",")),stroke_width:0.5,tag_is_artifact:true}
]')
CURRENT=$(post_multipart pdf-with-added-shapes -F "id=$CURRENT_ID" -F "shape_objects=$HEADER_SHAPES" -F 'tag_enabled=true')
CURRENT_ID=$(printf '%s' "$CURRENT" | jq -r '.outputId')

HEADER_TEXT=$(jq -n --argjson metadata "$METADATA" --argjson style "$STYLE" '
  def text($page;$x;$y;$value;$size;$color;$width;$structure;$bold): {font: (if $bold then $style.boldFont else $style.bodyFont end),max_width:$width,opacity:"1",page:($page|tostring),rotation:"0",text:$value,text_color_rgb:$color,text_size:$size,x:$x,y:$y,tag_structure_type:$structure};
  ($style.primaryColorRgb | join(",")) as $primary | ($style.mutedTextColorRgb | join(",")) as $muted |
  [text(1;405;724;"INVOICE";22;$primary;153;"H1";true),text(1;405;696;("Invoice " + $metadata.invoiceNumber);9;$muted;153;"P";true),text(1;405;682;("Issued " + $metadata.issueDate);9;$muted;153;"P";false),text(1;405;668;("Due " + $metadata.dueDate);9;$muted;153;"P";false),text(1;66;622;"FROM";8;$primary;216;"H2";true),text(1;66;606;$metadata.seller.name;9;"34,34,34";216;"P";true),text(1;66;592;$metadata.seller.taxId;8;$muted;216;"P";false),text(1;66;578;$metadata.seller.addressLine1;8;$muted;216;"P";false),text(1;66;566;($metadata.seller.city + ", " + $metadata.seller.region + " " + $metadata.seller.postalCode);8;$muted;216;"P";false),text(1;330;622;"BILL TO";8;$primary;216;"H2";true),text(1;330;606;$metadata.customer.name;9;"34,34,34";216;"P";true),text(1;330;592;$metadata.customer.addressLine1;8;$muted;216;"P";false),text(1;330;578;($metadata.customer.city + ", " + $metadata.customer.region + " " + $metadata.customer.postalCode);8;$muted;216;"P";false)]
')
CURRENT=$(post_multipart pdf-with-added-text -F "id=$CURRENT_ID" -F "text_objects=$HEADER_TEXT" -F 'tag_enabled=true' -F 'tag_language=en-US')
CURRENT_ID=$(printf '%s' "$CURRENT" | jq -r '.outputId')

TABLE_OBJECTS=$(jq -n --argjson metadata "$METADATA" --argjson style "$STYLE" --argjson items "$ITEMS" '
  def right: {text_align:"right"};
  def money: "$" + ((. * 100 | round) / 100 | tostring);
  ($style.primaryColorRgb) as $primary | ($style.borderColorRgb) as $border | ($style.accentColorRgb) as $accent |
  ($items | map((.quantity|tonumber) * (.unitPrice|tonumber)) | add) as $subtotal | (($subtotal * $metadata.taxRate * 100 | round) / 100) as $tax | ($subtotal + $tax) as $total |
  [{page:1,x:54,y:510,width:504,columns:[{width:276},{width:54},{width:84},{width:90}],continuation_page_top_margin:85,page_bottom_margin:96,final_page_bottom_margin:164,overflow_behavior:"split-row",row_split_behavior:"prefer-next-page",repeat_header_on_overflow:true,show_footer_on_last_page:true,tag_structure_type:"Table",style:{padding:{top:5,right:6,bottom:5,left:6},text_size:$style.tableHeaderFontSize,text_color_rgb:$style.textColorRgb,border:{top:{color_rgb:$border,width:0.5},right:{color_rgb:$border,width:0.5},bottom:{color_rgb:$border,width:0.5},left:{color_rgb:$border,width:0.5}}},header_rows:[{cells:( ["Description","Qty","Unit Price","Amount"] | to_entries | map({text:.value,tag_structure_type:"TH",style:({background_color_rgb:$primary,text_color_rgb:[255,255,255]} + (if .key > 0 then right else {} end))) )}],rows:($items | map({cells:[{text:.description},{text:.quantity,style:right},{text:(.unitPrice|tonumber|money),style:right},{text:((.quantity|tonumber) * (.unitPrice|tonumber)|money),style:right}]})),footer_rows:[{cells:[{text:"Subtotal",col_span:3,style:right},{text:($subtotal|money),style:right}]},{cells:[{text:("Tax (" + (($metadata.taxRate*100)|tostring) + "%)"),col_span:3,style:right},{text:($tax|money),style:right}]},{cells:[{text:"Total",col_span:3,style:(right + {background_color_rgb:$accent,text_size:10})},{text:($total|money),style:(right + {background_color_rgb:$accent,text_size:10})}]}]}]')
CURRENT=$(post_multipart pdf-with-added-tables -F "id=$CURRENT_ID" -F "table_objects=$TABLE_OBJECTS" -F 'tag_enabled=true' -F 'tag_language=en-US')
CURRENT_ID=$(printf '%s' "$CURRENT" | jq -r '.outputId')

CURRENT=$(post_multipart pdf-with-added-image -F "id=$CURRENT_ID" \
  -F 'image_objects={"image_index":0,"page":1,"x":54,"y":716,"width":200,"tag_alt_text":"Northstar Sample Supply logo","tag_structure_type":"Figure"}' \
  -F "image_files=@$DATA_DIR/northstar-logo.png" -F 'tag_enabled=true' -F 'tag_language=en-US')
CURRENT_ID=$(printf '%s' "$CURRENT" | jq -r '.outputId')
PAGE_COUNT=$(post_multipart pdf-info -F "id=$CURRENT_ID" -F 'queries=page_count' | jq -r '.page_count')

FOOTER_SHAPES=$(jq -n --arg page "$PAGE_COUNT" --argjson style "$STYLE" '[{type:"rectangle",page:($page|tonumber),x:54,y:70,width:504,height:104,fill_color_rgb:"248,250,251",stroke_color_rgb:($style.borderColorRgb|join(",")),stroke_width:0.5,tag_is_artifact:true}]')
CURRENT=$(post_multipart pdf-with-added-shapes -F "id=$CURRENT_ID" -F "shape_objects=$FOOTER_SHAPES" -F 'tag_enabled=true')
CURRENT_ID=$(printf '%s' "$CURRENT" | jq -r '.outputId')

FOOTER_TEXT=$(jq -n --arg page "$PAGE_COUNT" --argjson metadata "$METADATA" --argjson style "$STYLE" '
  def text($page;$x;$y;$value;$size;$width;$structure): {font:(if $structure == "H2" then $style.boldFont else $style.bodyFont end),max_width:$width,opacity:"1",page:($page|tostring),rotation:"0",text:$value,text_color_rgb:($style.mutedTextColorRgb|join(",")),text_size:$size,x:$x,y:$y,tag_structure_type:$structure};
  [text($page;66;156;"Payment terms";8;480;"H2"),text($page;66;142;$metadata.paymentTerms;7.5;480;"P"),text($page;66;112;"Notes";8;480;"H2"),text($page;66;98;$metadata.notes;7.5;480;"P")] + ([range(1;($page|tonumber)+1)[] as $p | text($p;54;54;"Generated from structured JSON and CSV input with pdfRest.";7.5;400;"P"), text($p;490;54;("Page " + ($p|tostring) + " of " + $page);7.5;68;"P")] | flatten)
')
FINAL=$(post_multipart pdf-with-added-text -F "id=$CURRENT_ID" -F "text_objects=$FOOTER_TEXT" -F 'tag_enabled=true' -F 'tag_language=en-US' -F 'output=invoice_from_structured_data')
FINAL_ID=$(printf '%s' "$FINAL" | jq -r '.outputId')
curl --fail-with-body --silent --show-error --location "$API_URL/resource/$FINAL_ID?format=file" --header "Api-Key: $API_KEY" --output "$OUTPUT_PATH"
echo "Created $OUTPUT_PATH"
