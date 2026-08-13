"""Generate a tagged invoice from JSON and CSV input using pdfRest."""

import csv
import json
import os
from pathlib import Path

import requests
from requests_toolbelt import MultipartEncoder


# This workflow creates a blank document, then composes the invoice by passing
# each response ID to the next pdfRest endpoint.
API_URL = os.getenv("PDFREST_URL", "https://api.pdfrest.com").rstrip("/")
API_KEY = os.getenv("PDFREST_API_KEY", "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx")
DATA_DIR = Path(__file__).with_name("invoice-data")
OUTPUT_PATH = Path(__file__).with_name("invoice-from-structured-data.pdf")

TABLE_WIDTH = 504
TABLE_COLUMN_WIDTHS = (276, 54, 84, 90)
TABLE_PADDING = {"top": 5, "right": 6, "bottom": 5, "left": 6}


def post_multipart(endpoint, fields):
    """Send a multipart request and return its JSON response."""
    encoder = MultipartEncoder(fields=fields)
    response = requests.post(
        f"{API_URL}/{endpoint}",
        data=encoder,
        headers={
            "Accept": "application/json",
            "Content-Type": encoder.content_type,
            "Api-Key": API_KEY,
        },
        timeout=120,
    )
    print(f"{endpoint}: {response.status_code}")
    if not response.ok:
        raise RuntimeError(f"{endpoint} failed: {response.text}")
    return response.json()


def load_inputs():
    """Load the fictional invoice metadata, style, and line items."""
    with open(DATA_DIR / "metadata.json", encoding="utf-8") as metadata_file:
        metadata = json.load(metadata_file)
    with open(DATA_DIR / "style.json", encoding="utf-8") as style_file:
        style = json.load(style_file)
    with open(DATA_DIR / "line-items.csv", newline="", encoding="utf-8") as csv_file:
        line_items = list(csv.DictReader(csv_file))
    return metadata, style, line_items


def money(value):
    """Format a numeric value as a currency string."""
    return f"${value:,.2f}"


def rgb_string(values):
    """Format an RGB list for text and shape endpoint fields."""
    return ",".join(str(value) for value in values)


def add_text_object(
    objects,
    style,
    page,
    x,
    y,
    value,
    size,
    color,
    width,
    structure="P",
    bold=False,
):
    """Append one configured object for the Add Text endpoint."""
    objects.append(
        {
            "font": style["boldFont"] if bold else style["bodyFont"],
            "max_width": width,
            "opacity": "1",
            "page": page,
            "rotation": "0",
            "text": value,
            "text_color_rgb": color,
            "text_size": size,
            "x": x,
            "y": y,
            "tag_structure_type": structure,
        }
    )


def build_header_text_objects(metadata, style):
    """Build the first-page invoice heading and billing text."""
    seller = metadata["seller"]
    customer = metadata["customer"]
    primary = rgb_string(style["primaryColorRgb"])
    muted = rgb_string(style["mutedTextColorRgb"])
    text = []

    def add(page, x, y, value, size, color, width, structure="P", bold=False):
        add_text_object(
            text,
            style,
            page,
            x,
            y,
            value,
            size,
            color,
            width,
            structure,
            bold,
        )

    add("1", 405, 724, "INVOICE", 22, primary, 153, "H1", True)
    add("1", 405, 696, f'Invoice {metadata["invoiceNumber"]}', 9, muted, 153, bold=True)
    add("1", 405, 682, f'Issued {metadata["issueDate"]}', 9, muted, 153)
    add("1", 405, 668, f'Due {metadata["dueDate"]}', 9, muted, 153)

    add("1", 66, 622, "FROM", 8, primary, 216, "H2", True)
    add("1", 66, 606, seller["name"], 9, "34,34,34", 216, bold=True)
    add("1", 66, 592, seller["taxId"], 8, muted, 216)
    add("1", 66, 578, seller["addressLine1"], 8, muted, 216)
    add("1", 66, 566, f'{seller["city"]}, {seller["region"]} {seller["postalCode"]}', 8, muted, 216)
    add("1", 330, 622, "BILL TO", 8, primary, 216, "H2", True)
    add("1", 330, 606, customer["name"], 9, "34,34,34", 216, bold=True)
    add("1", 330, 592, customer["addressLine1"], 8, muted, 216)
    add(
        "1",
        330,
        578,
        f'{customer["city"]}, {customer["region"]} {customer["postalCode"]}',
        8,
        muted,
        216,
    )

    return text


def build_footer_text_objects(metadata, style, page_count):
    """Build running page text and final-page payment details."""
    primary = rgb_string(style["primaryColorRgb"])
    muted = rgb_string(style["mutedTextColorRgb"])
    text = []
    final_page = str(page_count)
    add_text_object(text, style, final_page, 66, 136, "Payment terms", 8, primary, 480, "H2", True)
    add_text_object(text, style, final_page, 66, 122, metadata["paymentTerms"], 7.5, muted, 480)
    add_text_object(text, style, final_page, 66, 92, "Notes", 8, primary, 480, "H2", True)
    add_text_object(text, style, final_page, 66, 78, metadata["notes"], 7.5, muted, 480)

    for page in range(1, page_count + 1):
        footer_y = 54
        add_text_object(
            text,
            style,
            str(page),
            54,
            footer_y,
            "Generated from structured JSON and CSV input with pdfRest.",
            7.5,
            muted,
            400,
        )
        add_text_object(
            text,
            style,
            str(page),
            490,
            footer_y,
            f"Page {page} of {page_count}",
            7.5,
            muted,
            68,
        )
    return text


def build_table(metadata, style, line_items, subtotal, tax, total):
    """Convert every CSV line item into one auto-paginated table."""
    rows = []
    for item in line_items:
        quantity = float(item["quantity"])
        unit_price = float(item["unitPrice"])
        cells = [
            {"text": item["description"]},
            {"text": item["quantity"], "style": {"text_align": "right"}},
            {"text": money(unit_price), "style": {"text_align": "right"}},
            {"text": money(quantity * unit_price), "style": {"text_align": "right"}},
        ]
        rows.append({"cells": cells})

    if not rows:
        rows.append({"cells": [{"text": "No line items", "col_span": 4}]})

    border = {"color_rgb": style["borderColorRgb"], "width": 0.5}
    header_rows = [
        {
            "cells": [
                {
                    "text": "Description",
                    "tag_structure_type": "TH",
                    "style": {
                        "background_color_rgb": style["primaryColorRgb"],
                        "text_color_rgb": [255, 255, 255],
                    },
                },
                {
                    "text": "Qty",
                    "tag_structure_type": "TH",
                    "style": {
                        "background_color_rgb": style["primaryColorRgb"],
                        "text_color_rgb": [255, 255, 255],
                        "text_align": "right",
                    },
                },
                {
                    "text": "Unit Price",
                    "tag_structure_type": "TH",
                    "style": {
                        "background_color_rgb": style["primaryColorRgb"],
                        "text_color_rgb": [255, 255, 255],
                        "text_align": "right",
                    },
                },
                {
                    "text": "Amount",
                    "tag_structure_type": "TH",
                    "style": {
                        "background_color_rgb": style["primaryColorRgb"],
                        "text_color_rgb": [255, 255, 255],
                        "text_align": "right",
                    },
                },
            ]
        }
    ]
    footer_rows = [
        {
            "cells": [
                {"text": "Subtotal", "col_span": 3, "style": {"text_align": "right"}},
                {"text": money(subtotal), "style": {"text_align": "right"}},
            ]
        },
        {
            "cells": [
                {
                    "text": f'Tax ({metadata["taxRate"] * 100:.2f}%)',
                    "col_span": 3,
                    "style": {"text_align": "right"},
                },
                {"text": money(tax), "style": {"text_align": "right"}},
            ]
        },
        {
            "cells": [
                {
                    "text": "Total",
                    "col_span": 3,
                    "style": {
                        "background_color_rgb": style["accentColorRgb"],
                        "text_align": "right",
                        "text_size": 10,
                    },
                },
                {
                    "text": money(total),
                    "style": {
                        "background_color_rgb": style["accentColorRgb"],
                        "text_align": "right",
                        "text_size": 10,
                    },
                },
            ]
        },
    ]

    # The table chooses its own page breaks. These reserved regions keep it
    # below continuation-page headers, above running footers, and clear of the
    # larger payment-details panel on whichever page becomes the final page.
    return {
        "page": 1,
        "x": 54,
        "y": 510,
        "width": TABLE_WIDTH,
        "columns": [{"width": width} for width in TABLE_COLUMN_WIDTHS],
        "continuation_page_top_margin": 85,
        "page_bottom_margin": 96,
        "final_page_bottom_margin": 164,
        "overflow_behavior": "split-row",
        "row_split_behavior": "prefer-next-page",
        "repeat_header_on_overflow": True,
        "show_footer_on_last_page": True,
        "style": {
            "border": {"top": border, "right": border, "bottom": border, "left": border},
            "padding": TABLE_PADDING,
            "text_size": float(style["tableHeaderFontSize"]),
            "text_color_rgb": style["textColorRgb"],
        },
        "header_rows": header_rows,
        "rows": rows,
        "footer_rows": footer_rows,
        "tag_structure_type": "Table",
    }


def build_header_shape_objects(style):
    """Build the first-page billing panel."""
    border_color = rgb_string(style["borderColorRgb"])
    return [
        {
            "type": "rectangle",
            "page": 1,
            "x": 54,
            "y": 540,
            "width": 240,
            "height": 96,
            "fill_color_rgb": rgb_string(style["accentColorRgb"]),
            "stroke_color_rgb": border_color,
            "stroke_width": 0.5,
            "tag_is_artifact": True,
        },
        {
            "type": "rectangle",
            "page": 1,
            "x": 318,
            "y": 540,
            "width": 240,
            "height": 96,
            "fill_color_rgb": rgb_string(style["accentColorRgb"]),
            "stroke_color_rgb": border_color,
            "stroke_width": 0.5,
            "tag_is_artifact": True,
        },
    ]


def build_footer_shape_objects(style, page_count):
    """Build the details panel in the final reserved page region."""
    border_color = rgb_string(style["borderColorRgb"])
    return [
        {
            "type": "rectangle",
            "page": page_count,
            "x": 54,
            "y": 70,
            "width": 504,
            "height": 104,
            "fill_color_rgb": "248,250,251",
            "stroke_color_rgb": border_color,
            "stroke_width": 0.5,
            "tag_is_artifact": True,
        },
    ]


def download_output(output_id):
    """Download the final resource to the sample directory."""
    response = requests.get(
        f"{API_URL}/resource/{output_id}?format=file",
        headers={"Api-Key": API_KEY},
        timeout=120,
    )
    response.raise_for_status()
    OUTPUT_PATH.write_bytes(response.content)


def main():
    """Run the complete invoice-generation workflow."""
    metadata, style, line_items = load_inputs()
    subtotal = sum(float(item["quantity"]) * float(item["unitPrice"]) for item in line_items)
    tax = round(subtotal * metadata["taxRate"], 2)
    total = subtotal + tax

    table = build_table(metadata, style, line_items, subtotal, tax, total)

    blank = post_multipart(
        "blank-pdf",
        {
            "page_size": "letter",
            "page_count": "1",
            "page_orientation": "portrait",
        },
    )
    current_id = blank["outputId"]

    shapes = build_header_shape_objects(style)
    shape_result = post_multipart(
        "pdf-with-added-shapes",
        {
            "id": current_id,
            "shape_objects": json.dumps(shapes),
            "tag_enabled": "true",
        },
    )
    current_id = shape_result["outputId"]

    text_result = post_multipart(
        "pdf-with-added-text",
        {
            "id": current_id,
            "text_objects": json.dumps(build_header_text_objects(metadata, style)),
            "tag_enabled": "true",
            "tag_language": "en-US",
        },
    )
    current_id = text_result["outputId"]

    # One table request handles any number of CSV rows and creates continuation
    # pages as needed.
    table_result = post_multipart(
        "pdf-with-added-tables",
        {
            "id": current_id,
            "table_objects": json.dumps(table),
            "tag_enabled": "true",
            "tag_language": "en-US",
        },
    )
    current_id = table_result["outputId"]

    logo_path = DATA_DIR / "northstar-logo.png"
    with open(logo_path, "rb") as logo_file:
        image_result = post_multipart(
            "pdf-with-added-image",
            {
                "id": current_id,
                "image_objects": json.dumps(
                    {
                        "image_index": 0,
                        "page": 1,
                        "x": 54,
                        "y": 716,
                        "width": 200,
                        "tag_alt_text": "Northstar Sample Supply logo",
                        "tag_structure_type": "Figure",
                    }
                ),
                "image_files": (logo_path.name, logo_file, "image/png"),
                "tag_enabled": "true",
                "tag_language": "en-US",
            },
        )
    current_id = image_result["outputId"]

    info_result = post_multipart("pdf-info", {"id": current_id, "queries": "page_count"})
    page_count = int(info_result["page_count"])

    footer_shapes = build_footer_shape_objects(style, page_count)
    shape_result = post_multipart(
        "pdf-with-added-shapes",
        {
            "id": current_id,
            "shape_objects": json.dumps(footer_shapes),
            "tag_enabled": "true",
        },
    )
    current_id = shape_result["outputId"]

    footer_text = build_footer_text_objects(metadata, style, page_count)
    text_result = post_multipart(
        "pdf-with-added-text",
        {
            "id": current_id,
            "text_objects": json.dumps(footer_text),
            "tag_enabled": "true",
            "tag_language": "en-US",
            "output": "invoice_from_structured_data",
        },
    )
    download_output(text_result["outputId"])
    print(f"Created {OUTPUT_PATH}")


if __name__ == "__main__":
    main()
