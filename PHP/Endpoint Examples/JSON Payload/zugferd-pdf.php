<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;

// Upload invoice XML, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
$apiUrl = 'https://api.pdfrest.com';
// $apiUrl = 'https://eu-api.pdfrest.com'; // EU/GDPR service
$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$invoiceXml = '/path/to/invoice.xml';
$client = new Client(['http_errors' => false]);

$upload = $client->post($apiUrl . '/upload', [
    'headers' => ['Api-Key' => $apiKey, 'Content-Type' => 'application/xml', 'Content-Filename' => basename($invoiceXml)],
    'body' => fopen($invoiceXml, 'r'),
]);
if ($upload->getStatusCode() >= 300) { fwrite(STDERR, (string) $upload->getBody()); exit(1); }
$xmlId = json_decode($upload->getBody(), true)['files'][0]['id'];

$response = $client->post($apiUrl . '/zugferd-pdf', [
    'headers' => ['Accept' => 'application/json', 'Api-Key' => $apiKey],
    'json' => ['id' => $xmlId, 'output' => 'zugferd_invoice', 'render_options' => ['locale' => 'de-DE', 'label_language' => 'de', 'font' => 'arial', 'bold_font' => 'arialbold', 'accent_color_rgb' => [0, 92, 171]]],
]);
echo $response->getBody();
if ($response->getStatusCode() >= 300) exit(1);
