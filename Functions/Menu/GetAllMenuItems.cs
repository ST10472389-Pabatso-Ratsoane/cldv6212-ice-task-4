using Azure.Data.Tables;
using POE_CoffeeNChill.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace POE_CoffeeNChill.Functions.Menu;

public class GetAllMenuItems
{
    // Attributes for logging and interacting with Azure Table Storage
    private readonly ILogger<GetAllMenuItems> _logger;
    private readonly TableClient _tableClient;

    public GetAllMenuItems(
        ILogger<GetAllMenuItems> logger,
        TableClient tableClient)
    {
        _logger = logger;
        _tableClient = tableClient;
    }

    [Function("GetAllMenuItems")] // Azure Function for getting all menu items via GET requests
    public async Task<IActionResult> GetAllItems(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "menu")]
        HttpRequest request)
    {
        try
        {
            var menuItems =
                new List<MenuItems>();

            await foreach (
                var item in
                _tableClient.QueryAsync<MenuItems>()) // Query Azure Table Storage for all menu records
            {
                menuItems.Add(item); // Add each retrieved menu item to the list
            }

            _logger.LogInformation(
                "Returned {Count} menu items.",
                menuItems.Count);

            return new OkObjectResult(menuItems); // HTTP 200 OK response having the menu list in JSON from
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving all menu items.");

            return new StatusCodeResult(
                StatusCodes.Status500InternalServerError);
        }
    }
}

// Code Attribution 
// This code was taken from stackoverflow
// https://stackoverflow.com/questions/75079808/query-azure-table-storage-for-faster-retrieval-of-data-in-c-sharp
// Abhishek_Singh_Rana
// https://stackoverflow.com/users/20381556/abhishek-singh-rana