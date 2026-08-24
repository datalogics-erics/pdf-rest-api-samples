<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;
use GuzzleHttp\Psr7\Utils;

// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//$apiUrl = "https://eu-api.pdfrest.com";

$inputPath = '/path/to/sample.md';
$imagePath = '/path/to/logo.png';
$options = json_decode('{"title":"Structured Content Sample","language":"en-US","enable_tagging":true,"page_setup":{"size":"Letter","orientation":"portrait","margin":{"top":36,"right":42,"bottom":36,"left":42}},"style":{"font":"Arial","heading_font":"Arial","code_font":"Courier","text_size":11,"text_color_rgb":[34,34,34],"heading_scale":1.35,"table":{"column_width_weights":[2,3,2],"keep_header_with_first_row":true,"repeat_headers_on_overflow":true,"show_borders":true,"border_width":0.75,"border_color_rgb":[180,188,200],"header_fill_color_rgb":[33,64,98],"header_text_color_rgb":[255,255,255],"row_fill_color_rgb":[250,250,252],"alternate_row_fill_color_rgb":[235,240,246],"cell_padding":{"top":6,"right":8,"bottom":6,"left":8}}},"markdown":{"image_alt_text":{"sample-logo":"Sample logo"},"missing_image_alt_text":"fail","image_sources":{"sample-logo":{"image_id_index":0}}}}', true);

// This sample uploads Markdown input, then calls /pdf with a JSON payload.
// It demonstrates structured_text_options, tagged Markdown image mapping, and input/output resource IDs.
$client = new Client(['http_errors' => false]);
$upload = function (string $path) use ($client, $apiUrl): string {
  $response = $client->post($apiUrl . '/upload', ['headers' => ['Api-Key' => 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', 'Content-Type' => 'application/octet-stream', 'Content-Filename' => basename($path)], 'body' => Utils::tryFopen($path, 'r')]);
  $body = json_decode((string) $response->getBody(), true);
  return $body['files'][0]['id'];
};
$inputId = $upload($inputPath);
$imageId = $upload($imagePath);
$payload = ['id' => $inputId, 'structured_text_options' => $options];
$payload['image_ids'] = [$imageId];
$response = $client->post($apiUrl . '/pdf', ['headers' => ['Api-Key' => 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', 'Content-Type' => 'application/json', 'Accept' => 'application/json'], 'json' => $payload]);
echo $response->getBody();

