import json
import os
import requests

# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

# This sample converts PDF to PostScript through multipart /postscript.
# Pairing this endpoint with /pdf creates a PDF -> PostScript -> PDF workflow commonly
# called PDF refrying. Some print and prepress workflows use it to rebuild, flatten, or
# normalize page content, but the lossy roundtrip can discard PDF-specific features.
# These settings request Level 3, text-safe output, all pages at original scale,
# shrink-to-fit without rotation, and printable annotations.
input_path = "/path/to/sample.pdf"

from requests_toolbelt import MultipartEncoder
with open(input_path, "rb") as input_file:
    fields = {
        "file": (os.path.basename(input_path), input_file, "application/pdf"),
        "ps_level": "3",
        "page_range": "all",
        "binary_output": "false",
        "scale": "1",
        "rotate": "false",
        "shrink_to_fit": "true",
        "print_annotations": "true",
        "output": "postscript_from_pdf",
    }
    form = MultipartEncoder(fields=fields)
    response = requests.post(api_url + "/postscript", data=form, headers={
        "Accept": "application/json",
        "Content-Type": form.content_type,
        "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    })

print("Response status code: " + str(response.status_code))
if response.ok:
    print(json.dumps(response.json(), indent=2))
else:
    print(response.text)
    raise SystemExit(1)
