using Azure.Data.Tables;
using POE_CoffeeNChill.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using POE_CoffeeNChill.Models;

namespace POE_CoffeeNChill.Functions.Menu;

public class DeleteMenuItem
{
    private readonly ILogger<DeleteMenuItem> _logger;
    private readonly TableClient _tableClient;

    public DeleteMenuItem(ILogger<DeleteMenuItem> logger, TableClient tableClient)
    {
        _logger = logger;
        _tableClient = tableClient;
    }

    [Function("DeleteMenuItem")]
    public async Task<IActionResult> DeleteItem(
        [HttpTrigger( AuthorizationLevel.Anonymous, "delete", Route = "menu/{category}/{menuId}")]
        HttpRequest request,
        string category,
        string menuId)
    {
        try
        {
            var normalizedCategory = MenuCategoryHelper.Normalize(category);

            if (!MenuCategoryHelper.IsValid(normalizedCategory))
            {
                return new BadRequestObjectResult("A valid category is required.");
            }

            if (string.IsNullOrWhiteSpace(menuId))
            {
                return new BadRequestObjectResult("A menu item ID is required.");
            }

            var response = await _tableClient.GetEntityIfExistsAsync<MenuItems>(normalizedCategory, menuId);

            if (!response.HasValue)
            {
                return new NotFoundObjectResult($"Menu item '{menuId}' was not found.");
            }

            var menuItem = response.Value;

            await _tableClient.DeleteEntityAsync(menuItem.PartitionKey, menuItem.RowKey, menuItem.ETag);

            _logger.LogInformation("Deleted menu item {MenuId} from {Category}.", menuId, normalizedCategory);

            return new NoContentResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu item {MenuId}.", menuId);

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);  // Posts the error message
        }
    }
}