<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;
use GuzzleHttp\Psr7\Utils;

// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = getenv('PDFREST_URL') ?: 'https://api.pdfrest.com';
$apiKey = getenv('PDFREST_API_KEY') ?: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';

$inputPath = $argv[1] ?? '/path/to/sample.csv';
$options = json_decode('{"title":"Structured Content Sample","language":"en-US","enable_tagging":true,"page_setup":{"size":"Letter","orientation":"portrait","margin":{"top":36,"right":42,"bottom":36,"left":42}},"style":{"font":"Arial","heading_font":"Arial","code_font":"Courier","text_size":11,"text_color_rgb":[34,34,34],"heading_scale":1.35,"table":{"column_width_weights":[2,3,2],"keep_header_with_first_row":true,"repeat_headers_on_overflow":true,"show_borders":true,"border_width":0.75,"border_color_rgb":[180,188,200],"header_fill_color_rgb":[33,64,98],"header_text_color_rgb":[255,255,255],"row_fill_color_rgb":[250,250,252],"alternate_row_fill_color_rgb":[235,240,246],"cell_padding":{"top":6,"right":8,"bottom":6,"left":8}}},"csv":{"first_row_is_header":true,"delimiter":",","columns":[{"index":0,"width_weight":2,"text_align":"left"},{"index":1,"width_weight":3,"text_align":"left"},{"index":2,"width_weight":1,"text_align":"right"}]}}', true);

// This sample converts CSV input to a tagged PDF through multipart /pdf.
// It demonstrates structured_text_options and the format-specific conversion options.
$multipart = [
  ['name' => 'file', 'contents' => Utils::tryFopen($inputPath, 'r'), 'filename' => basename($inputPath)],
  ['name' => 'structured_text_options', 'contents' => json_encode($options)],
  ['name' => 'output', 'contents' => 'pdf_from_csv'],
];
$response = (new Client())->post($apiUrl . '/pdf', ['headers' => ['Api-Key' => $apiKey, 'Accept' => 'application/json'], 'multipart' => $multipart]);
echo $response->getBody();

