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

// This sample converts an Email (.eml) file to PDF by sending it directly in a
// multipart /pdf request.
$inputPath = '/path/to/sample.eml';
$multipart = [
  ['name' => 'file', 'contents' => Utils::tryFopen($inputPath, 'r'), 'filename' => basename($inputPath), 'headers' => ['Content-Type' => 'message/rfc822']],
  ['name' => 'output', 'contents' => 'pdf_from_email'],
];
$response = (new Client())->post($apiUrl . '/pdf', ['headers' => ['Api-Key' => 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', 'Accept' => 'application/json'], 'multipart' => $multipart]);
echo $response->getBody();
