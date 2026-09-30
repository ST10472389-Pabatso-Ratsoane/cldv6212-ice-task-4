using Azure.Data.Tables;
using POE_CoffeeNChill.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using POE_CoffeeNChill.Models;

namespace POE_CoffeeNChill.Functions.Menu;

public class GetMenuItemsByCategory
{
    private readonly
        ILogger<GetMenuItemsByCategory> _logger;

    private readonly TableClient _tableClient;

    public GetMenuItemsByCategory(
        ILogger<GetMenuItemsByCategory> logger,
        TableClient tableClient)
    {
        _logger = logger;
        _tableClient = tableClient;
    }

    [Function("GetMenuItemsByCategory")] // Azure Function for getting menu items based on category via GET requests
    public async Task<IActionResult> GetItems(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "menu/category/{category}")]
        HttpRequest request,
        string category)

    // Code Attribution 
    // This code was taken from stackoverflow
    // https://stackoverflow.com/questions/77511404/isolated-azure-function-with-consumption-plan
    // agamil
    // https://stackoverflow.com/users/6603925/agamil
    {
        try
        {
            var normalizedCategory =
                MenuCategoryHelper.Normalize(category);

            if (!MenuCategoryHelper.IsValid(
                    normalizedCategory))
            {
                return new BadRequestObjectResult(
                    "A valid category is required.");
            }

            var menuItems =
                new List<MenuItems>();

            await foreach (
                var item in
                _tableClient.QueryAsync<MenuItems>(
                    item =>
                        item.PartitionKey ==
                        normalizedCategory))
            {
                menuItems.Add(item);
            }

            _logger.LogInformation(
                "Returned {Count} items for category {Category}.",
                menuItems.Count,
                normalizedCategory);

            return new OkObjectResult(menuItems); // Returns the list of items matching the category 
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving menu items for category {Category}.",
                category);

            return new StatusCodeResult(
                StatusCodes.Status500InternalServerError); // Return 500 Internal Server Error because this represents a server/query failure
        }
    }
}
// Code Attribution 
// This code was taken from stackoverflow
// https://stackoverflow.com/questions/71271486/how-to-query-across-multiple-partition-keys-in-an-azure-storage-table
// Stephen Cleary
// https://stackoverflow.com/users/263693/stephen-cleary