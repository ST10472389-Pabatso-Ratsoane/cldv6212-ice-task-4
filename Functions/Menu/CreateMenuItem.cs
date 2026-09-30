using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using POE_CoffeeNChill.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace POE_CoffeeNChill.Functions.Menu;

public class CreateMenuItem
{
    // Attributes for logging and interacting with Azure Table Storage
    private readonly ILogger<CreateMenuItem> _logger;
    private readonly TableClient _tableClient;

    public CreateMenuItem(
        ILogger<CreateMenuItem> logger,
        TableClient tableClient)
    {
        _logger = logger;
        _tableClient = tableClient;
    }

    [Function("CreateMenuItem")]     // Azure Function for creating menu items via POST requests
    public async Task<IActionResult> CreateItem(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "menu")]
        HttpRequest request)
    {
        try
        {
            var createRequest =
                await JsonSerializer.DeserializeAsync<CreateMenuItemRequest>(
                    request.Body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }); // Deserialising the incoming JSON request into a Menu object, regardless of case sensitivity

            if (createRequest == null)
            {
                return new BadRequestObjectResult(
                    "A valid JSON request body is required.");
            }

            var category =
                MenuCategoryHelper.Normalize(
                    createRequest.PartitionKey);

            if (!MenuCategoryHelper.IsValid(category))
            {
                return new BadRequestObjectResult(
                    "A valid category is required.");
            }

            if (string.IsNullOrWhiteSpace(
                    createRequest.RowKey))
            {
                return new BadRequestObjectResult(
                    "A menu item ID / SKU is required.");
            }

            if (string.IsNullOrWhiteSpace(
                    createRequest.Name))
            {
                return new BadRequestObjectResult(
                    "A menu item name is required.");
            }

            if (createRequest.Price <= 0)
            {
                return new BadRequestObjectResult(
                    "Price must be greater than zero.");
            }

            var menuItem = new MenuItems
            {
                PartitionKey = category,
                RowKey = createRequest.RowKey.Trim(),
                Name = createRequest.Name.Trim(),
                Description =
                    createRequest.Description?.Trim()
                    ?? string.Empty,
                Price = createRequest.Price,
                IsAvailable =
                    createRequest.IsAvailable
            };

            await _tableClient.AddEntityAsync(menuItem); // Add the new menu table into Azure Table Storage

            _logger.LogInformation(
                "Menu item {MenuId} created in category {Category}.",
                menuItem.RowKey,
                menuItem.PartitionKey);

            return new ObjectResult(menuItem)
            {
                StatusCode = StatusCodes.Status201Created

                // Code Attribution 
                // This code was taken from stackoverflow
                // https://stackoverflow.com/questions/23892341/how-can-i-code-a-created-201-response-using-ihttpactionresult
                // chris31389
                // https://stackoverflow.com/users/2069306/chris31389
            };

        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Invalid JSON received while creating menu item.");

            return new BadRequestObjectResult(
                "The request body contains invalid JSON.");
        }
        catch (RequestFailedException ex)
            when (ex.Status == StatusCodes.Status409Conflict)
        {
            _logger.LogWarning(
                ex,
                "Attempted to create a duplicate menu item.");

            return new ConflictObjectResult(
                "A menu item with the same category and ID already exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while creating a menu item.");

            return new StatusCodeResult(
                StatusCodes.Status500InternalServerError); // Posts the error message
        }
    }
}
// Code Attribution 
// This code was taken from stackoverflow
// https://stackoverflow.com/questions/79660060/case-insensitive-json-parameter-names-in-functionsapplicationbuilder
// Pravallika KV
// https://stackoverflow.com/users/19991670/pravallika-kv