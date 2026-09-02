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

/* This sample converts PDF to PostScript through multipart /postscript.
 * Pairing this endpoint with /pdf creates a PDF -> PostScript -> PDF workflow commonly
 * called PDF refrying. Some print and prepress workflows use it to rebuild, flatten, or
 * normalize page content, but the lossy roundtrip can discard PDF-specific features.
 * These settings request Level 3, text-safe output, all pages at original scale,
 * shrink-to-fit without rotation, and printable annotations.
 */
$inputPath = '/path/to/sample.pdf';
$multipart = [
  ['name' => 'file', 'contents' => Utils::tryFopen($inputPath, 'r'), 'filename' => basename($inputPath), 'headers' => ['Content-Type' => 'application/pdf']],
  ['name' => 'ps_level', 'contents' => '3'],
  ['name' => 'page_range', 'contents' => 'all'],
  ['name' => 'binary_output', 'contents' => 'false'],
  ['name' => 'scale', 'contents' => '1'],
  ['name' => 'rotate', 'contents' => 'false'],
  ['name' => 'shrink_to_fit', 'contents' => 'true'],
  ['name' => 'print_annotations', 'contents' => 'true'],
  ['name' => 'output', 'contents' => 'postscript_from_pdf'],
];
$response = (new Client())->post($apiUrl . '/postscript', ['headers' => ['Api-Key' => 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', 'Accept' => 'application/json'], 'multipart' => $multipart]);
echo $response->getBody();
