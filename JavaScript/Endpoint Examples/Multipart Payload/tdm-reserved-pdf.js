// This request demonstrates how to apply TDM rights metadata to a PDF.
var axios = require('axios');
var FormData = require('form-data');
var fs = require('fs');

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

// Create a new form data object and append the PDF file and TDM policy to it.
var tdmReservedPdfData = new FormData();
tdmReservedPdfData.append('file', fs.createReadStream('/path/to/file'));
tdmReservedPdfData.append('policy', 'https://example.com/tdm-policy');
tdmReservedPdfData.append('output', 'pdfrest_tdm_reserved_pdf');

// define configuration options for axios request
var tdmReservedPdfConfig = {
  method: 'post',
  maxBodyLength: Infinity, // set maximum length of the request body
  url: apiUrl + '/tdm-reserved-pdf',
  headers: {
    'Api-Key': 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', // Replace with your API key
    ...tdmReservedPdfData.getHeaders() // set headers for the request
  },
  data : tdmReservedPdfData // set the data to be sent with the request
};

// send request and handle response or error
axios(tdmReservedPdfConfig)
.then(function (response) {
  console.log(JSON.stringify(response.data));
})
.catch(function (error) {
  console.log(error);
});

// If you would like to download the file instead of getting the JSON response, please see the 'get-resource-id-endpoint.js' sample.
