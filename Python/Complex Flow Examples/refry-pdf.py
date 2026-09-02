"""Refry a PDF by converting it to PostScript and back to PDF.

PDF refrying is the common name for a PDF -> PostScript -> PDF roundtrip.
Some print, prepress, and legacy production workflows use it to rebuild or
normalize page content, flatten certain PDF constructs, or prepare a file for
downstream systems. The process is intentionally lossy and may remove tags,
forms, layers, annotations, transparency, metadata, and editability. Use this
workflow when a downstream system requires rebuilt page content or a
PostScript-based interchange file.

The PostScript-to-PDF step uses a custom .joboptions profile in this sample. A
.joboptions file contains Adobe Distiller-compatible conversion settings; it is
optional, and default settings are used when omitted. pdfRest applies the
profile with Datalogics PDF Converter SDK. Datalogics maintains the SDK in
partnership with Adobe, using the same Adobe technology that powers Distiller.

Run: python3 refry-pdf.py <pdf> <jobOptions> [outputPdf]
"""

import json
import os
import sys
from pathlib import Path

import requests
from requests_toolbelt import MultipartEncoder


API_URL = os.getenv("PDFREST_URL", "https://api.pdfrest.com").rstrip("/")
API_KEY = os.getenv("PDFREST_API_KEY", "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx")
INPUT_PATH = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("/path/to/sample.pdf")
JOB_OPTIONS_PATH = (
    Path(sys.argv[2]) if len(sys.argv) > 2 else Path("/path/to/custom.joboptions")
)
OUTPUT_PATH = Path(__file__).with_name("refried.pdf")
if len(sys.argv) > 3:
    OUTPUT_PATH = Path(sys.argv[3])


def post_multipart(endpoint, fields):
    """Send a multipart request and return its JSON response."""
    form = MultipartEncoder(fields=fields)
    response = requests.post(
        f"{API_URL}/{endpoint}",
        data=form,
        headers={
            "Accept": "application/json",
            "Content-Type": form.content_type,
            "Api-Key": API_KEY,
        },
        timeout=120,
    )
    print(f"{endpoint}: {response.status_code}")
    if not response.ok:
        raise RuntimeError(f"{endpoint} failed: {response.text}")
    return response.json()


with INPUT_PATH.open("rb") as input_file:
    postscript = post_multipart(
        "postscript",
        {
            "file": (INPUT_PATH.name, input_file, "application/pdf"),
            "ps_level": "3",
            "page_range": "all",
            "binary_output": "true",
            "scale": "1",
            "rotate": "false",
            "shrink_to_fit": "true",
            "print_annotations": "true",
            "output": "refry_intermediate",
        },
    )

with JOB_OPTIONS_PATH.open("rb") as job_options_file:
    final_pdf = post_multipart(
        "pdf",
        {
            "id": postscript["outputId"],
            "job_options": (
                JOB_OPTIONS_PATH.name,
                job_options_file,
                "application/octet-stream",
            ),
            "output": "refried",
        },
    )

download = requests.get(
    f"{API_URL}/resource/{final_pdf['outputId']}?format=file",
    headers={"Api-Key": API_KEY},
    timeout=120,
)
download.raise_for_status()
OUTPUT_PATH.write_bytes(download.content)
print(json.dumps(final_pdf, indent=2))
print(f"Created {OUTPUT_PATH}")
