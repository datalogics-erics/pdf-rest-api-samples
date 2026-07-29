<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;

// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = 'https://api.pdfrest.com';

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
// $apiUrl = 'https://eu-api.pdfrest.com';

$apiKey = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'; // Replace with your API key.
$client = new Client(['http_errors' => false]);

// Upload the input PDF first, then use its resource ID in a JSON endpoint call.
$uploadResponse = $client->send(new Request('POST', $apiUrl . '/upload', [
    'Api-Key' => $apiKey,
    'Content-Filename' => 'input.pdf',
    'Content-Type' => 'application/octet-stream',
], file_get_contents('/path/to/input.pdf')));

echo $uploadResponse->getBody() . PHP_EOL;
$uploadResult = json_decode($uploadResponse->getBody(), true);
if (!isset($uploadResult['files'][0]['id'])) {
    exit("The PDF upload did not return a resource ID.\n");
}

$shapeObjects = [
    ['type' => 'rectangle', 'page' => 1, 'x' => 54, 'y' => 540, 'width' => 504, 'height' => 108,
        'fill_color_rgb' => '245,247,250', 'stroke_color_rgb' => '26,72,112', 'stroke_width' => 1, 'tag_is_artifact' => true],
    ['type' => 'line', 'page' => 1, 'x1' => 72, 'y1' => 576, 'x2' => 540, 'y2' => 576,
        'stroke_color_rgb' => '26,72,112', 'stroke_width' => 1.5,
        'tag_actual_text' => 'Review section divider', 'tag_structure_type' => 'Figure'],
];

$requestBody = json_encode([
    'id' => $uploadResult['files'][0]['id'],
    'shape_objects' => $shapeObjects,
    'tag_enabled' => true,
    'output' => 'review-panel',
]);
$response = $client->send(new Request('POST', $apiUrl . '/pdf-with-added-shapes', [
    'Accept' => 'application/json',
    'Api-Key' => $apiKey,
    'Content-Type' => 'application/json',
], $requestBody));

echo $response->getBody() . PHP_EOL;
