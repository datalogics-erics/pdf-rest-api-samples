curl -X POST "https://api.pdfrest.com/pdf-with-added-attachment" \
  -H "Accept: application/json" \
  -H "Content-Type: multipart/form-data" \
  -H "Api-Key: xxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  -F "file=@/path/to/file" \
  -F "file_to_attach=@/path/to/file" \
  -F "output=example_out"
