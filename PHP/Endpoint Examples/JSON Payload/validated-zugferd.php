<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;

// Upload a hybrid PDF, then validate its ZUGFeRD / Factur-X package by resource ID.
$apiUrl = 'https://api.pdfrest.com';
// $apiUrl = 'https://eu-api.pdfrest.com'; // EU/GDPR service
$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$zugferdPdf = '/path/to/zugferd-invoice.pdf';
$client = new Client(['http_errors' => false]);

$upload = $client->post($apiUrl . '/upload', [
    'headers' => ['Api-Key' => $apiKey, 'Content-Type' => 'application/pdf', 'Content-Filename' => basename($zugferdPdf)],
    'body' => fopen($zugferdPdf, 'r'),
]);
if ($upload->getStatusCode() >= 300) { fwrite(STDERR, (string) $upload->getBody()); exit(1); }
$pdfId = json_decode($upload->getBody(), true)['files'][0]['id'];

$response = $client->post($apiUrl . '/validated-zugferd', [
    'headers' => ['Accept' => 'application/json', 'Api-Key' => $apiKey],
    'json' => ['id' => $pdfId],
]);
echo $response->getBody();
if ($response->getStatusCode() >= 300) exit(1);
