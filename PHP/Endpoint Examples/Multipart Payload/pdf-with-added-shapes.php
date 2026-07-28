<?php
require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;
use GuzzleHttp\Psr7\Utils;

// By default, we use the US-based API service. This is the primary endpoint for global use.
$apiUrl = 'https://api.pdfrest.com';

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
// $apiUrl = 'https://eu-api.pdfrest.com';

// Add a lightly shaded review panel and a divider line to page one.
// Coordinates are measured from the lower-left corner in PDF units (72 units = 1 inch).
$shapeObjects = [
    [
        'type' => 'rectangle', 'page' => 1, 'x' => 54, 'y' => 540, 'width' => 504, 'height' => 108,
        'fill_color_rgb' => '245,247,250', 'stroke_color_rgb' => '26,72,112', 'stroke_width' => 1,
        'tag_is_artifact' => true,
    ],
    [
        'type' => 'line', 'page' => 1, 'x1' => 72, 'y1' => 576, 'x2' => 540, 'y2' => 576,
        'stroke_color_rgb' => '26,72,112', 'stroke_width' => 1.5,
        'tag_actual_text' => 'Review section divider', 'tag_structure_type' => 'Figure',
    ],
];

$request = new Request('POST', $apiUrl . '/pdf-with-added-shapes', [
    'Api-Key' => 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', // Replace with your API key.
]);
$response = (new Client(['http_errors' => false]))->send($request, [
    'multipart' => [
        ['name' => 'file', 'contents' => Utils::tryFopen('/path/to/input.pdf', 'r'), 'filename' => 'input.pdf'],
        ['name' => 'shape_objects', 'contents' => json_encode($shapeObjects)],
        ['name' => 'tag_enabled', 'contents' => 'true'],
        ['name' => 'output', 'contents' => 'review-panel'],
    ],
]);

echo $response->getBody() . PHP_EOL;

// To download the returned file, use the outputId with the get-resource-id endpoint sample.
