<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Utils;

// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//$apiUrl = "https://eu-api.pdfrest.com";

/* This sample uploads a PDF, then converts it through the JSON /postscript flow.
 * Pairing this endpoint with /pdf creates a PDF -> PostScript -> PDF workflow commonly
 * called PDF refrying. Some print and prepress workflows use it to rebuild, flatten, or
 * normalize page content, but the lossy roundtrip can discard PDF-specific features.
 * These settings request Level 3, text-safe output, all pages at original scale,
 * shrink-to-fit without rotation, and printable annotations.
 */
$inputPath = '/path/to/sample.pdf';
$client = new Client(['http_errors' => false]);
$upload = function (string $path) use ($client, $apiUrl): string {
  $response = $client->post($apiUrl . '/upload', ['headers' => ['Api-Key' => 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', 'Content-Type' => 'application/octet-stream', 'Content-Filename' => basename($path)], 'body' => Utils::tryFopen($path, 'r')]);
  $body = json_decode((string) $response->getBody(), true);
  return $body['files'][0]['id'];
};
$inputId = $upload($inputPath);
$payload = [
  'id' => $inputId,
  'ps_level' => 3,
  'page_range' => 'all',
  'binary_output' => false,
  'scale' => 1,
  'rotate' => false,
  'shrink_to_fit' => true,
  'print_annotations' => true,
  'output' => 'postscript_from_pdf',
];
$response = $client->post($apiUrl . '/postscript', ['headers' => ['Api-Key' => 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', 'Content-Type' => 'application/json', 'Accept' => 'application/json'], 'json' => $payload]);
echo $response->getBody();
