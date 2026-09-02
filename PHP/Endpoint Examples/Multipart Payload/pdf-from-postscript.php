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

/* This sample converts a PostScript (.ps) file to PDF with a custom .joboptions profile.
 * A .joboptions file contains Adobe Distiller-compatible conversion settings. The
 * profile is optional; omit job_options to use default settings. pdfRest applies a
 * supplied profile with Datalogics PDF Converter SDK. Datalogics maintains the SDK
 * in partnership with Adobe, using the same Adobe technology that powers Distiller.
 * Pairing this /pdf call with /postscript creates a PDF -> PostScript -> PDF workflow
 * commonly called PDF refrying. Some print and prepress workflows use it to rebuild or
 * normalize page content, but the lossy roundtrip can discard PDF-specific features.
 */
$inputPath = '/path/to/sample.ps';
$jobOptionsPath = '/path/to/custom.joboptions';
$multipart = [
  ['name' => 'file', 'contents' => Utils::tryFopen($inputPath, 'r'), 'filename' => basename($inputPath), 'headers' => ['Content-Type' => 'application/postscript']],
  ['name' => 'job_options', 'contents' => Utils::tryFopen($jobOptionsPath, 'r'), 'filename' => basename($jobOptionsPath), 'headers' => ['Content-Type' => 'application/octet-stream']],
  ['name' => 'output', 'contents' => 'pdf_from_postscript'],
];
$response = (new Client())->post($apiUrl . '/pdf', ['headers' => ['Api-Key' => 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', 'Accept' => 'application/json'], 'multipart' => $multipart]);
echo $response->getBody();
