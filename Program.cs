using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Azure Table Storage
// MenuItems

builder.Services.AddSingleton<TableClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var connectionString = configuration["AzureTableStorage"] ?? configuration["AzureWebJobsStorage"];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Azure Storage connection string is missing.");
    }

    var tableServiceClient = new TableServiceClient(connectionString);

    var tableClient = tableServiceClient.GetTableClient("MenuItems");

    tableClient.CreateIfNotExists();

    return tableClient;
});

// Azure Blob Storage
// staff-docs

builder.Services.AddSingleton<BlobServiceClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var connectionString = configuration["AzureWebJobsStorage"];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("AzureWebJobsStorage connection string is missing.");
    }

    return new BlobServiceClient(connectionString);
});

builder.Build().Run();