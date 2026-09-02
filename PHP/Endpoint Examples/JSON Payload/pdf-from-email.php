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

// This sample converts an Email (.eml) file to PDF. It uploads the Email file
// first, then calls /pdf with its resource ID.
$inputPath = '/path/to/sample.eml';
$client = new Client(['http_errors' => false]);
$upload = function (string $path) use ($client, $apiUrl): string {
  $response = $client->post($apiUrl . '/upload', ['headers' => ['Api-Key' => 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', 'Content-Type' => 'application/octet-stream', 'Content-Filename' => basename($path)], 'body' => Utils::tryFopen($path, 'r')]);
  $body = json_decode((string) $response->getBody(), true);
  return $body['files'][0]['id'];
};
$inputId = $upload($inputPath);
$payload = ['id' => $inputId, 'output' => 'pdf_from_email'];
$response = $client->post($apiUrl . '/pdf', ['headers' => ['Api-Key' => 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', 'Content-Type' => 'application/json', 'Accept' => 'application/json'], 'json' => $payload]);
echo $response->getBody();
