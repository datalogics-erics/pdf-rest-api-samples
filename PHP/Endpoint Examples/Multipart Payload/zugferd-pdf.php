<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Utils;

// Create a ZUGFeRD / Factur-X PDF/A-3 invoice from invoice XML.
$apiUrl = 'https://api.pdfrest.com';
// $apiUrl = 'https://eu-api.pdfrest.com'; // EU/GDPR service
$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$invoiceXml = '/path/to/invoice.xml';

$response = (new Client())->post($apiUrl . '/zugferd-pdf', [
    'headers' => ['Accept' => 'application/json', 'Api-Key' => $apiKey],
    'multipart' => [
        ['name' => 'file', 'contents' => Utils::tryFopen($invoiceXml, 'r'), 'filename' => basename($invoiceXml), 'headers' => ['Content-Type' => 'application/xml']],
        ['name' => 'render_options', 'contents' => json_encode(['locale' => 'de-DE', 'label_language' => 'de', 'font' => 'arial', 'bold_font' => 'arialbold', 'accent_color_rgb' => [0, 92, 171]])],
        ['name' => 'output', 'contents' => 'zugferd_invoice'],
    ],
]);

echo $response->getBody();
