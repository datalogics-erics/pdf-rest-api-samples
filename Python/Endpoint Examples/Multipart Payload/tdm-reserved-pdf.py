from requests_toolbelt import MultipartEncoder
import requests
import json

# This sample applies metadata on a PDF declaring Text and Data Mining (TDM) rights.

# By default, we use the US-based API service. This is the primary endpoint for global use.
api_url = "https://api.pdfrest.com"

# For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
# For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
#api_url = "https://eu-api.pdfrest.com"

tdm_reserved_pdf_endpoint_url = api_url+'/tdm-reserved-pdf'

# The /tdm-reserved-pdf endpoint can take a single PDF file or id as input.
mp_encoder_tdm_reserved_pdf = MultipartEncoder(
    fields={
        'file': ('file_name.pdf', open('/path/to/file', 'rb'), 'application/pdf'),
        'policy': 'https://example.com/tdm-policy',
        'output' : 'example_tdm_reserved_pdf_out'
    }
)

# Let's set the headers that the tdm-reserved-pdf endpoint expects.
# Since MultipartEncoder is used, the 'Content-Type' header gets set to 'multipart/form-data' via the content_type attribute below.
headers = {
    'Accept': 'application/json',
    'Content-Type': mp_encoder_tdm_reserved_pdf.content_type,
    'Api-Key': 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' # place your api key here
}

print("Sending POST request to tdm-reserved-pdf endpoint...")
response = requests.post(tdm_reserved_pdf_endpoint_url, data=mp_encoder_tdm_reserved_pdf, headers=headers)

print("Response status code: " + str(response.status_code))

if response.ok:
    response_json = response.json()
    print(json.dumps(response_json, indent = 2))
else:
    print(response.text)

# If you would like to download the file instead of getting the JSON response, please see the 'get-resource-id-endpoint.py' sample.
