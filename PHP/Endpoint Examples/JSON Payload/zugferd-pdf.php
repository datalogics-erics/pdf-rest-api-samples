<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;

// Upload invoice XML and PDF, then create a ZUGFeRD / Factur-X PDF/A-3 invoice by resource ID.
// pdfRest preserves the supplied PDF when it agrees with the canonical XML.
$apiUrl = 'https://api.pdfrest.com';
// $apiUrl = 'https://eu-api.pdfrest.com'; // EU/GDPR service
$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$invoiceXml = '/path/to/invoice.xml';
$invoicePdf = '/path/to/invoice.pdf';
$client = new Client(['http_errors' => false]);

$upload = $client->post($apiUrl . '/upload', [
    'headers' => ['Api-Key' => $apiKey, 'Content-Type' => 'application/xml', 'Content-Filename' => basename($invoiceXml)],
    'body' => fopen($invoiceXml, 'r'),
]);
if ($upload->getStatusCode() >= 300) { fwrite(STDERR, (string) $upload->getBody()); exit(1); }
$xmlId = json_decode($upload->getBody(), true)['files'][0]['id'];

$pdfUpload = $client->post($apiUrl . '/upload', [
    'headers' => ['Api-Key' => $apiKey, 'Content-Type' => 'application/pdf', 'Content-Filename' => basename($invoicePdf)],
    'body' => fopen($invoicePdf, 'r'),
]);
if ($pdfUpload->getStatusCode() >= 300) { fwrite(STDERR, (string) $pdfUpload->getBody()); exit(1); }
$pdfId = json_decode($pdfUpload->getBody(), true)['files'][0]['id'];

// Fallback generation handles a mismatch or an unconfirmed PDF/XML match.
// The render options style only that replacement PDF, not a preserved supplied PDF.
$response = $client->post($apiUrl . '/zugferd-pdf', [
    'headers' => ['Accept' => 'application/json', 'Api-Key' => $apiKey],
    'json' => ['id' => $xmlId, 'pdf_id' => $pdfId, 'regenerate_pdf' => true, 'output' => 'zugferd_invoice', 'render_options' => ['locale' => 'de-DE', 'label_language' => 'de', 'font' => 'arial', 'bold_font' => 'arialbold', 'accent_color_rgb' => [0, 92, 171]]],
]);
echo $response->getBody();
if ($response->getStatusCode() >= 300) exit(1);
