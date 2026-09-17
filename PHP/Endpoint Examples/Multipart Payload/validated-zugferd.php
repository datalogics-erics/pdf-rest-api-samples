<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Utils;

// Validate a hybrid ZUGFeRD / Factur-X PDF without modifying it.
$apiUrl = 'https://api.pdfrest.com';
// $apiUrl = 'https://eu-api.pdfrest.com'; // EU/GDPR service
$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$zugferdPdf = '/path/to/zugferd-invoice.pdf';

$response = (new Client(['http_errors' => false]))->post($apiUrl . '/validated-zugferd', [
    'headers' => ['Accept' => 'application/json', 'Api-Key' => $apiKey],
    'multipart' => [['name' => 'file', 'contents' => Utils::tryFopen($zugferdPdf, 'r'), 'filename' => basename($zugferdPdf), 'headers' => ['Content-Type' => 'application/pdf']]],
]);
echo $response->getBody();
if ($response->getStatusCode() >= 300) exit(1);
