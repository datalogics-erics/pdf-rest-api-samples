import json
import os
import requests

# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

# This sample converts a PostScript (.ps) file to PDF with a custom .joboptions profile.
# A .joboptions file contains Adobe Distiller-compatible conversion settings. The
# profile is optional; omit job_options to use default settings. pdfRest applies a
# supplied profile with Datalogics PDF Converter SDK. Datalogics maintains the SDK
# in partnership with Adobe, using the same Adobe technology that powers Distiller.
# Pairing this /pdf call with /postscript creates a PDF -> PostScript -> PDF workflow
# commonly called PDF refrying. Some print and prepress workflows use it to rebuild or
# normalize page content, but the lossy roundtrip can discard PDF-specific features.
input_path = "/path/to/sample.ps"
job_options_path = "/path/to/custom.joboptions"

from requests_toolbelt import MultipartEncoder
with open(input_path, "rb") as input_file, open(job_options_path, "rb") as job_options_file:
    fields = {
        "file": (os.path.basename(input_path), input_file, "application/postscript"),
        "job_options": (
            os.path.basename(job_options_path),
            job_options_file,
            "application/octet-stream",
        ),
        "output": "pdf_from_postscript",
    }
    form = MultipartEncoder(fields=fields)
    response = requests.post(api_url + "/pdf", data=form, headers={
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
