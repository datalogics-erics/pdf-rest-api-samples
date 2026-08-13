<?php

/*
 * Generate a tagged invoice from JSON and CSV input using pdfRest.
 *
 * Set PDFREST_API_KEY before running this sample. It reads metadata.json,
 * style.json, and line-items.csv from invoice-data, then downloads the PDF
 * beside this script. Install Guzzle with `composer require guzzlehttp/guzzle`
 * and run
 * with `php create-invoice-from-structured-data.php`.
 */

require 'vendor/autoload.php';

use GuzzleHttp\Client;
use GuzzleHttp\Psr7\Request;
use GuzzleHttp\Psr7\Utils;

$apiUrl = rtrim(getenv('PDFREST_URL') ?: 'https://api.pdfrest.com', '/');
$apiKey = getenv('PDFREST_API_KEY') ?: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
$dataDir = __DIR__ . '/invoice-data';
$outputPath = __DIR__ . '/invoice-from-structured-data.pdf';
$client = new Client(['http_errors' => true]);

function rgb(array $values): string { return implode(',', $values); }
function money(float $value): string { return '$' . number_format($value, 2); }

function postJson(Client $client, string $url, string $apiKey, array $payload): array
{
    $response = $client->send(new Request('POST', $url, ['Accept' => 'application/json', 'Api-Key' => $apiKey, 'Content-Type' => 'application/json'], json_encode($payload)));
    return json_decode($response->getBody(), true, 512, JSON_THROW_ON_ERROR);
}

function postMultipart(Client $client, string $url, string $apiKey, array $fields): array
{
    $multipart = [];
    foreach ($fields as $name => $value) {
        if (is_array($value) && isset($value['path'])) {
            $multipart[] = ['name' => $name, 'contents' => Utils::tryFopen($value['path'], 'r'), 'filename' => $value['name']];
        } else {
            $multipart[] = ['name' => $name, 'contents' => is_string($value) ? $value : json_encode($value)];
        }
    }
    $response = $client->send(new Request('POST', $url, ['Accept' => 'application/json', 'Api-Key' => $apiKey], null), ['multipart' => $multipart]);
    return json_decode($response->getBody(), true, 512, JSON_THROW_ON_ERROR);
}

function textObjects(array $metadata, array $style): array
{
    $seller = $metadata['seller']; $customer = $metadata['customer']; $objects = [];
    $add = function (int $x, int $y, string $text, float $size, string $color, int $width, string $structure = 'P', bool $bold = false) use (&$objects, $style): void {
        $objects[] = ['font' => $bold ? $style['boldFont'] : $style['bodyFont'], 'max_width' => $width, 'opacity' => '1', 'page' => '1', 'rotation' => '0', 'text' => $text, 'text_color_rgb' => $color, 'text_size' => $size, 'x' => $x, 'y' => $y, 'tag_structure_type' => $structure];
    };
    $primary = rgb($style['primaryColorRgb']); $muted = rgb($style['mutedTextColorRgb']);
    $add(405, 724, 'INVOICE', 22, $primary, 153, 'H1', true); $add(405, 696, 'Invoice ' . $metadata['invoiceNumber'], 9, $muted, 153, 'P', true); $add(405, 682, 'Issued ' . $metadata['issueDate'], 9, $muted, 153); $add(405, 668, 'Due ' . $metadata['dueDate'], 9, $muted, 153);
    $add(66, 622, 'FROM', 8, $primary, 216, 'H2', true); $add(66, 606, $seller['name'], 9, '34,34,34', 216, 'P', true); $add(66, 592, $seller['taxId'], 8, $muted, 216); $add(66, 578, $seller['addressLine1'], 8, $muted, 216); $add(66, 566, $seller['city'] . ', ' . $seller['region'] . ' ' . $seller['postalCode'], 8, $muted, 216);
    $add(330, 622, 'BILL TO', 8, $primary, 216, 'H2', true); $add(330, 606, $customer['name'], 9, '34,34,34', 216, 'P', true); $add(330, 592, $customer['addressLine1'], 8, $muted, 216); $add(330, 578, $customer['city'] . ', ' . $customer['region'] . ' ' . $customer['postalCode'], 8, $muted, 216);
    return $objects;
}

function tableObjects(array $metadata, array $style, array $items): array
{
    $subtotal = array_reduce($items, fn(float $sum, array $item): float => $sum + ((float)$item['quantity'] * (float)$item['unitPrice']), 0.0);
    $tax = round($subtotal * $metadata['taxRate'], 2); $total = $subtotal + $tax;
    $headerStyle = ['background_color_rgb' => $style['primaryColorRgb'], 'text_color_rgb' => [255, 255, 255]];
    $headers = ['Description', 'Qty', 'Unit Price', 'Amount']; $header = [];
    foreach ($headers as $index => $text) $header[] = ['text' => $text, 'tag_structure_type' => 'TH', 'style' => $index ? $headerStyle + ['text_align' => 'right'] : $headerStyle];
    $rows = array_map(function (array $item): array { $quantity = (float)$item['quantity']; $unitPrice = (float)$item['unitPrice']; return ['cells' => [['text' => $item['description']], ['text' => $item['quantity'], 'style' => ['text_align' => 'right']], ['text' => money($unitPrice), 'style' => ['text_align' => 'right']], ['text' => money($quantity * $unitPrice), 'style' => ['text_align' => 'right']]]]; }, $items);
    $summary = function (string $label, float $value, array $styleOverride = []): array { return ['cells' => [['text' => $label, 'col_span' => 3, 'style' => ['text_align' => 'right'] + $styleOverride], ['text' => money($value), 'style' => ['text_align' => 'right'] + $styleOverride]]]; };
    return [[
        'page' => 1, 'x' => 54, 'y' => 510, 'width' => 504, 'columns' => [['width' => 276], ['width' => 54], ['width' => 84], ['width' => 90]],
        'continuation_page_top_margin' => 85, 'page_bottom_margin' => 96, 'final_page_bottom_margin' => 164, 'overflow_behavior' => 'split-row', 'row_split_behavior' => 'prefer-next-page', 'repeat_header_on_overflow' => true, 'show_footer_on_last_page' => true, 'tag_structure_type' => 'Table',
        'style' => ['border' => ['top' => ['color_rgb' => $style['borderColorRgb'], 'width' => 0.5], 'right' => ['color_rgb' => $style['borderColorRgb'], 'width' => 0.5], 'bottom' => ['color_rgb' => $style['borderColorRgb'], 'width' => 0.5], 'left' => ['color_rgb' => $style['borderColorRgb'], 'width' => 0.5]], 'padding' => ['top' => 5, 'right' => 6, 'bottom' => 5, 'left' => 6], 'text_size' => $style['tableHeaderFontSize'], 'text_color_rgb' => $style['textColorRgb']],
        'header_rows' => [['cells' => $header]], 'rows' => $rows, 'footer_rows' => [$summary('Subtotal', $subtotal), $summary('Tax (' . number_format($metadata['taxRate'] * 100, 2) . '%)', $tax), $summary('Total', $total, ['background_color_rgb' => $style['accentColorRgb'], 'text_size' => 10])],
    ]];
}

function shapes(array $style, int $page, bool $footer = false): array
{
    $border = ['stroke_color_rgb' => rgb($style['borderColorRgb']), 'stroke_width' => 0.5, 'tag_is_artifact' => true];
    if ($footer) return [['type' => 'rectangle', 'page' => $page, 'x' => 54, 'y' => 70, 'width' => 504, 'height' => 104, 'fill_color_rgb' => '248,250,251'] + $border];
    return [['type' => 'rectangle', 'page' => 1, 'x' => 54, 'y' => 540, 'width' => 240, 'height' => 96, 'fill_color_rgb' => rgb($style['accentColorRgb'])] + $border, ['type' => 'rectangle', 'page' => 1, 'x' => 318, 'y' => 540, 'width' => 240, 'height' => 96, 'fill_color_rgb' => rgb($style['accentColorRgb'])] + $border];
}

$metadata = json_decode(file_get_contents($dataDir . '/metadata.json'), true, 512, JSON_THROW_ON_ERROR);
$style = json_decode(file_get_contents($dataDir . '/style.json'), true, 512, JSON_THROW_ON_ERROR);
$handle = fopen($dataDir . '/line-items.csv', 'r'); $headers = fgetcsv($handle); $items = [];
while (($row = fgetcsv($handle)) !== false) if (count($row) >= count($headers)) $items[] = array_combine($headers, $row);
fclose($handle);

$blank = postJson($client, $apiUrl . '/blank-pdf', $apiKey, ['page_size' => 'letter', 'page_count' => 1, 'page_orientation' => 'portrait']);
$currentId = $blank['outputId'];
$currentId = postMultipart($client, $apiUrl . '/pdf-with-added-shapes', $apiKey, ['id' => $currentId, 'shape_objects' => json_encode(shapes($style, 1)), 'tag_enabled' => 'true'])['outputId'];
$currentId = postMultipart($client, $apiUrl . '/pdf-with-added-text', $apiKey, ['id' => $currentId, 'text_objects' => json_encode(textObjects($metadata, $style)), 'tag_enabled' => 'true', 'tag_language' => 'en-US'])['outputId'];
$currentId = postMultipart($client, $apiUrl . '/pdf-with-added-tables', $apiKey, ['id' => $currentId, 'table_objects' => json_encode(tableObjects($metadata, $style, $items)), 'tag_enabled' => 'true', 'tag_language' => 'en-US'])['outputId'];
$currentId = postMultipart($client, $apiUrl . '/pdf-with-added-image', $apiKey, ['id' => $currentId, 'image_objects' => json_encode(['image_index' => 0, 'page' => 1, 'x' => 54, 'y' => 716, 'width' => 200, 'tag_alt_text' => 'Northstar Sample Supply logo', 'tag_structure_type' => 'Figure']), 'image_files' => ['path' => $dataDir . '/northstar-logo.png', 'name' => 'northstar-logo.png'], 'tag_enabled' => 'true', 'tag_language' => 'en-US'])['outputId'];
$pageCount = (int)postMultipart($client, $apiUrl . '/pdf-info', $apiKey, ['id' => $currentId, 'queries' => 'page_count'])['page_count'];
$currentId = postMultipart($client, $apiUrl . '/pdf-with-added-shapes', $apiKey, ['id' => $currentId, 'shape_objects' => json_encode(shapes($style, $pageCount, true)), 'tag_enabled' => 'true'])['outputId'];
$footer = [];
$addFooter = function (string $text, int $page, int $x, int $y, float $size, int $width, string $structure = 'P', bool $bold = false) use (&$footer, $style): void { $footer[] = ['font' => $bold ? $style['boldFont'] : $style['bodyFont'], 'max_width' => $width, 'opacity' => '1', 'page' => (string)$page, 'rotation' => '0', 'text' => $text, 'text_color_rgb' => rgb($style['mutedTextColorRgb']), 'text_size' => $size, 'x' => $x, 'y' => $y, 'tag_structure_type' => $structure]; };
$addFooter('Payment terms', $pageCount, 66, 156, 8, 480, 'H2', true); $addFooter($metadata['paymentTerms'], $pageCount, 66, 142, 7.5, 480); $addFooter('Notes', $pageCount, 66, 112, 8, 480, 'H2', true); $addFooter($metadata['notes'], $pageCount, 66, 98, 7.5, 480);
for ($page = 1; $page <= $pageCount; $page++) { $addFooter('Generated from structured JSON and CSV input with pdfRest.', $page, 54, 54, 7.5, 400); $addFooter("Page $page of $pageCount", $page, 490, 54, 7.5, 68); }
$final = postMultipart($client, $apiUrl . '/pdf-with-added-text', $apiKey, ['id' => $currentId, 'text_objects' => json_encode($footer), 'tag_enabled' => 'true', 'tag_language' => 'en-US', 'output' => 'invoice_from_structured_data']);
$pdf = $client->get($apiUrl . '/resource/' . rawurlencode($final['outputId']) . '?format=file', ['headers' => ['Api-Key' => $apiKey]]);
file_put_contents($outputPath, $pdf->getBody()->getContents());
echo "Created $outputPath\n";
