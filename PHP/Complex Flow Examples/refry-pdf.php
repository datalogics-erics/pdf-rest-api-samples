<?php
/* Refry a PDF by converting it to PostScript and back to PDF.
 *
 * PDF refrying is the common name for a PDF -> PostScript -> PDF roundtrip.
 * Some print, prepress, and legacy production workflows use it to rebuild or
 * normalize page content, flatten certain PDF constructs, or prepare a file
 * for downstream systems. The process is intentionally lossy and may remove
 * tags, forms, layers, annotations, transparency, metadata, and editability.
 * Use this workflow when a downstream system requires rebuilt page content or
 * a PostScript-based interchange file.
 *
 * The PostScript-to-PDF step uses a custom .joboptions profile in this sample.
 * A .joboptions file contains Adobe Distiller-compatible conversion settings;
 * it is optional, and default settings are used when omitted. pdfRest applies
 * the profile with Datalogics PDF Converter SDK. Datalogics maintains the SDK
 * in partnership with Adobe, using the same Adobe technology that powers
 * Distiller.
 *
 * Run: php refry-pdf.php <pdf> <jobOptions> [outputPdf]
 */

require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Utils;

$apiUrl = rtrim(getenv('PDFREST_URL') ?: 'https://api.pdfrest.com', '/');
$apiKey = getenv('PDFREST_API_KEY') ?: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$inputPath = $argv[1] ?? '/path/to/sample.pdf';
$jobOptionsPath = $argv[2] ?? '/path/to/custom.joboptions';
$outputPath = $argv[3] ?? __DIR__ . '/refried.pdf';
$client = new Client(['http_errors' => true]);

function postMultipart(Client $client, string $url, string $apiKey, array $parts): array
{
    $response = $client->post($url, [
        'headers' => ['Accept' => 'application/json', 'Api-Key' => $apiKey],
        'multipart' => $parts,
    ]);
    echo basename($url) . ': ' . $response->getStatusCode() . PHP_EOL;
    return json_decode($response->getBody(), true, 512, JSON_THROW_ON_ERROR);
}

$postscript = postMultipart($client, $apiUrl . '/postscript', $apiKey, [
    ['name' => 'file', 'contents' => Utils::tryFopen($inputPath, 'r'), 'filename' => basename($inputPath), 'headers' => ['Content-Type' => 'application/pdf']],
    ['name' => 'ps_level', 'contents' => '3'],
    ['name' => 'page_range', 'contents' => 'all'],
    ['name' => 'binary_output', 'contents' => 'true'],
    ['name' => 'scale', 'contents' => '1'],
    ['name' => 'rotate', 'contents' => 'false'],
    ['name' => 'shrink_to_fit', 'contents' => 'true'],
    ['name' => 'print_annotations', 'contents' => 'true'],
    ['name' => 'output', 'contents' => 'refry_intermediate'],
]);

$finalPdf = postMultipart($client, $apiUrl . '/pdf', $apiKey, [
    ['name' => 'id', 'contents' => $postscript['outputId']],
    ['name' => 'job_options', 'contents' => Utils::tryFopen($jobOptionsPath, 'r'), 'filename' => basename($jobOptionsPath), 'headers' => ['Content-Type' => 'application/octet-stream']],
    ['name' => 'output', 'contents' => 'refried'],
]);

$download = $client->get($apiUrl . '/resource/' . rawurlencode($finalPdf['outputId']) . '?format=file', ['headers' => ['Api-Key' => $apiKey]]);
file_put_contents($outputPath, $download->getBody()->getContents());
echo json_encode($finalPdf, JSON_PRETTY_PRINT) . PHP_EOL;
echo "Created $outputPath" . PHP_EOL;
