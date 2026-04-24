import requests
import json

# This sample uploads a PDF and applies TDM rights metadata.

# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

with open('/path/to/file', 'rb') as f:
    upload_data = f.read()

print("Uploading file...")
upload_response = requests.post(url=api_url+'/upload',
                    data=upload_data,
                    headers={'Content-Type': 'application/octet-stream', 'Content-Filename': 'file.pdf', "API-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"})

print("Upload response status code: " + str(upload_response.status_code))

if upload_response.ok:
    upload_response_json = upload_response.json()
    print(json.dumps(upload_response_json, indent = 2))


    uploaded_id = upload_response_json['files'][0]['id']
    tdm_reserved_pdf_data = { "id" : uploaded_id, "policy": "https://example.com/tdm-policy" }
    print(json.dumps(tdm_reserved_pdf_data, indent = 2))


    print("Applying TDM rights metadata...")
    tdm_reserved_pdf_response = requests.post(url=api_url+'/tdm-reserved-pdf',
                        data=json.dumps(tdm_reserved_pdf_data),
                        headers={'Content-Type': 'application/json', "API-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"})



    print("Processing response status code: " + str(tdm_reserved_pdf_response.status_code))
    if tdm_reserved_pdf_response.ok:
        tdm_reserved_pdf_response_json = tdm_reserved_pdf_response.json()
        print(json.dumps(tdm_reserved_pdf_response_json, indent = 2))

    else:
        print(tdm_reserved_pdf_response.text)
else:
    print(upload_response.text)
