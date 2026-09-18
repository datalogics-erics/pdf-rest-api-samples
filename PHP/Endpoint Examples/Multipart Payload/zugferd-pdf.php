<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Utils;

// Create a ZUGFeRD / Factur-X PDF/A-3 invoice from XML and an existing invoice PDF.
// pdfRest preserves the supplied PDF when it agrees with the canonical XML.
$apiUrl = 'https://api.pdfrest.com';
// $apiUrl = 'https://eu-api.pdfrest.com'; // EU/GDPR service
$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$invoiceXml = '/path/to/invoice.xml';
$invoicePdf = '/path/to/invoice.pdf';

$response = (new Client())->post($apiUrl . '/zugferd-pdf', [
    'headers' => ['Accept' => 'application/json', 'Api-Key' => $apiKey],
    'multipart' => [
        ['name' => 'file', 'contents' => Utils::tryFopen($invoiceXml, 'r'), 'filename' => basename($invoiceXml), 'headers' => ['Content-Type' => 'application/xml']],
        ['name' => 'pdf_file', 'contents' => Utils::tryFopen($invoicePdf, 'r'), 'filename' => basename($invoicePdf), 'headers' => ['Content-Type' => 'application/pdf']],
        // Fallback: generate a replacement PDF for a mismatch or unconfirmed PDF/XML match.
        ['name' => 'regenerate_pdf', 'contents' => 'true'],
        // These styles apply only to that fallback-generated PDF, not to a preserved PDF.
        ['name' => 'render_options', 'contents' => json_encode(['locale' => 'de-DE', 'label_language' => 'de', 'font' => 'arial', 'bold_font' => 'arialbold', 'accent_color_rgb' => [0, 92, 171]])],
        ['name' => 'output', 'contents' => 'zugferd_invoice'],
    ],
]);

echo $response->getBody();
