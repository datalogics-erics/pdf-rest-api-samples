import requests
import json

with open('/path/to/file', 'rb') as f:
    upload_data = f.read()

print("Uploading file...")
upload_response = requests.post(url='https://api.pdfrest.com/upload',
                    data=upload_data,
                    headers={'Content-Type': 'application/octet-stream', 'content-filename': 'file.pdf', "API-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"})

print("Upload response status code: " + str(upload_response.status_code))

if upload_response.ok:
    upload_response_json = upload_response.json()
    print(json.dumps(upload_response_json, indent = 2))


    uploaded_id = upload_response_json['files'][0]['id']
    export_data = { "id" : uploaded_id, 'data_format': 'xml' }
    print(json.dumps(export_data, indent = 2))


    print("Processing file...")
    export_response = requests.post(url='https://api.pdfrest.com/exported-form-data',
                        data=json.dumps(export_data),
                        headers={'Content-Type': 'application/json', "API-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"})



    print("Processing response status code: " + str(export_response.status_code))
    if export_response.ok:
        export_response_json = export_response.json()
        print(json.dumps(export_response_json, indent = 2))

    else:
        print(export_response.text)
else:
    print(upload_response.text)
