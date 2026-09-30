using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace POE_CoffeeNChill.Functions.StaffDocs;

public class UploadStaffDocument
{
    private readonly ILogger<UploadStaffDocument> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    private const string ContainerName = "staff-docs";

    public UploadStaffDocument(
        ILogger<UploadStaffDocument> logger,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    [Function("UploadStaffDocument")]
    public async Task<IActionResult> Upload(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")]
        HttpRequest request)
    {
        try
        {
            if (!request.HasFormContentType)
            {
                return new BadRequestObjectResult("The request must use multipart/form-data.");
            }

            var form = await request.ReadFormAsync();

            var file = form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("A non-empty document is required.");
            }

            var safeFileName =  Path.GetFileName(file.FileName);

            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                return new BadRequestObjectResult("A valid file name is required.");
            }

            var extension = Path.GetExtension(safeFileName).ToLowerInvariant();

            var allowedExtensions = new HashSet<string>
                {
                    ".pdf",
                    ".docx",
                    ".txt"
                };

            if (!allowedExtensions.Contains(extension))
            {
                return new BadRequestObjectResult("Only PDF, DOCX and TXT documents are supported.");
            }

            var containerClient = _blobServiceClient .GetBlobContainerClient(ContainerName);

            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(safeFileName);

            await using var stream = file.OpenReadStream();

            var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = string.IsNullOrWhiteSpace(file.ContentType)? "application/octet-stream" : file.ContentType
                        }
                };

            await blobClient.UploadAsync( stream, uploadOptions);

            _logger.LogInformation(
                "Uploaded staff document {FileName} to {ContainerName}.",
                safeFileName,
                ContainerName);

            return new ObjectResult(new
                {
                    message = "Document uploaded successfully.",
                    fileName = safeFileName,
                    size = file.Length,
                    contentType = file.ContentType
                })
            {
                StatusCode = StatusCodes.Status201Created
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error uploading staff document.");

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}