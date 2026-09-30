using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace POE_CoffeeNChill.Functions.StaffDocs;

public class DownloadStaffDocument
{
    private readonly ILogger<DownloadStaffDocument> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    private const string ContainerName = "staff-docs";

    public DownloadStaffDocument(
        ILogger<DownloadStaffDocument> logger,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    [Function("DownloadStaffDocument")]
    public async Task<IActionResult> Download(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "documents/download/{fileName}")]
        HttpRequest request,
        string fileName)
    {
        try
        {
            var safeFileName = Path.GetFileName(fileName);

            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                return new BadRequestObjectResult( "A valid file name is required.");
            }

            var containerClient = _blobServiceClient .GetBlobContainerClient(ContainerName);

            var blobClient = containerClient.GetBlobClient(safeFileName);

            if (!await blobClient.ExistsAsync())
            {
                return new NotFoundObjectResult( $"Document '{safeFileName}' was not found.");
            }

            var download = await blobClient.DownloadStreamingAsync();

            var contentType = download.Value.Details.ContentType ?? "application/octet-stream";

            _logger.LogInformation( "Downloaded staff document {FileName}.", safeFileName);

            return new FileStreamResult(  download.Value.Content, contentType)
            {
                FileDownloadName = safeFileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error downloading staff document {FileName}.",
                fileName);

            return new StatusCodeResult( StatusCodes.Status500InternalServerError);
        }
    }
}