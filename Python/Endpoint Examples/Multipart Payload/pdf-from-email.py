import json
import os
import requests

# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

# This sample converts an Email (.eml) file to PDF by sending it directly in a
# multipart /pdf request.
input_path = "/path/to/sample.eml"

from requests_toolbelt import MultipartEncoder
with open(input_path, "rb") as input_file:
    fields = {
        "file": (os.path.basename(input_path), input_file, "message/rfc822"),
        "output": "pdf_from_email",
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
