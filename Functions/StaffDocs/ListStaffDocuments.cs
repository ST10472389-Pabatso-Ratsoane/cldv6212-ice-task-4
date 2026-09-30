using Azure.Storage.Blobs;
using POE_CoffeeNChill.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace POE_CoffeeNChill.Functions.StaffDocs;

public class ListStaffDocuments
{
    private readonly ILogger<ListStaffDocuments> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    private const string ContainerName = "staff-docs";

    public ListStaffDocuments(
        ILogger<ListStaffDocuments> logger,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    [Function("ListStaffDocuments")]
    public async Task<IActionResult> List(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "documents")]
        HttpRequest request)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

            await containerClient.CreateIfNotExistsAsync();

            var documents = new List<StaffDocuments>();

            await foreach (var blob in containerClient.GetBlobsAsync())
            {
                documents.Add(new StaffDocuments
                    {
                        FileName = blob.Name,

                        Size = blob.Properties.ContentLength ?? 0,

                        LastModified = blob.Properties.LastModified,

                        ContentType = blob.Properties.ContentType ?? "application/octet-stream"
                    });
            }

            _logger.LogInformation( "Returned {Count} staff documents.", documents.Count);

            return new OkObjectResult(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving staff documents.");

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}