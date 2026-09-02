import json
import os
import requests

# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

# This sample converts an Email (.eml) file to PDF. It uploads the Email file
# first, then calls /pdf with its resource ID.
input_path = "/path/to/sample.eml"

def upload(path):
    with open(path, "rb") as source:
        response = requests.post(api_url + "/upload", data=source, headers={
            "Content-Type": "application/octet-stream",
            "Content-Filename": os.path.basename(path),
            "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
        })
    if not response.ok:
        print(response.text)
        raise SystemExit(1)
    return response.json()["files"][0]["id"]

input_id = upload(input_path)
payload = {
    "id": input_id,
    "output": "pdf_from_email",
}
response = requests.post(api_url + "/pdf", json=payload, headers={
    "Accept": "application/json",
    "Content-Type": "application/json",
    "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
})

print("Response status code: " + str(response.status_code))
if response.ok:
    print(json.dumps(response.json(), indent=2))
else:
    print(response.text)
    raise SystemExit(1)
