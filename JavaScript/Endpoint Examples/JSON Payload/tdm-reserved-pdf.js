var axios = require("axios");
var fs = require("fs");

// This sample uploads a PDF and applies TDM rights metadata.

// By default, we use the US-based API service. This is the primary endpoint for global use.
var apiUrl = "https://api.pdfrest.com";

/* For GDPR compliance and enhanced performance for European users, you can switch to the EU-based service by uncommenting the URL below.
 * For more information visit https://pdfrest.com/pricing#how-do-eu-gdpr-api-calls-work
 */
//var apiUrl = "https://eu-api.pdfrest.com";

var uploadData = fs.createReadStream("/path/to/file");

var uploadConfig = {
  method: "post",
  maxBodyLength: Infinity,
  url: apiUrl + "/upload",
  headers: {
    "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // Replace with your API key
    "Content-Filename": "filename.pdf",
    "Content-Type": "application/octet-stream",
  },
  data: uploadData, // set the data to be sent with the request
};

// send request and handle response or error
axios(uploadConfig)
  .then(function (upload_response) {
    console.log(JSON.stringify(upload_response.data));
    var uploadedId = upload_response.data.files[0].id;

    var tdmReservedPdfConfig = {
      method: "post",
      maxBodyLength: Infinity,
      url: apiUrl + "/tdm-reserved-pdf",
      headers: {
        "Api-Key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // Replace with your API key
        "Content-Type": "application/json",
      },
      data: { id: uploadedId, policy: "https://example.com/tdm-policy" }, // set the data to be sent with the request
    };

    // send request and handle response or error
    axios(tdmReservedPdfConfig)
      .then(function (tdmReservedPdfResponse) {
        console.log(JSON.stringify(tdmReservedPdfResponse.data));
      })
      .catch(function (error) {
        console.log(error);
      });
  })
  .catch(function (error) {
    console.log(error);
  });
