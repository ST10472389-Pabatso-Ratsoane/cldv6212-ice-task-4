using System;
using System.Collections.Generic;
using System.Text;
using Azure.Data.Tables;
using POE_CoffeeNChill.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;   
using System.Text.Json;

    namespace POE_CoffeeNChill.Functions.Menu;

    public class UpdateMenuItem
    {
        private readonly ILogger<UpdateMenuItem> _logger;
        private readonly TableClient _tableClient;

        public UpdateMenuItem(ILogger<UpdateMenuItem> logger, TableClient tableClient)
        {
            _logger = logger;
            _tableClient = tableClient;
        }

        [Function("UpdateMenuItem")]
        public async Task<IActionResult> UpdateItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{menuId}")] HttpRequest request, string category, string menuId)
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

                var updateRequest = await JsonSerializer.DeserializeAsync<UpdateMenuItemRequest>(request.Body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                if (updateRequest == null)
                {
                    return new BadRequestObjectResult("A valid JSON request body is required.");
                }

                if (updateRequest.Price == null && updateRequest.IsAvailable == null)
                {
                    return new BadRequestObjectResult("Provide a price, availability status, or both.");
                }

                if (updateRequest.Price.HasValue && updateRequest.Price.Value <= 0)
                {
                    return new BadRequestObjectResult("Price must be greater than zero.");
                }

            var response = await _tableClient.GetEntityIfExistsAsync<MenuItems>(normalizedCategory, menuId);

                if (!response.HasValue)
                {
                    return new NotFoundObjectResult($"Menu item '{menuId}' was not found in category '{normalizedCategory}'.");
                }

                var existingItem = response.Value;

                if (updateRequest.Price.HasValue)
                {
                    existingItem.Price = updateRequest.Price.Value;
                }

                if (updateRequest.IsAvailable.HasValue)
                {
                    existingItem.IsAvailable = updateRequest.IsAvailable.Value;
                }

                await _tableClient.UpdateEntityAsync(existingItem, existingItem.ETag, TableUpdateMode.Replace);

                // 3. Upsert/Update the entity in Table Storage
                // Code Attribution 
                // This code was taken from stackoverflow
                // https://stackoverflow.com/questions/73262773/azure-data-tables-generic-base-class-issue
                // DavSin
                // https://stackoverflow.com/users/13952268/davsin

                _logger.LogInformation("Updated menu item {MenuId} in {Category}.", menuId, normalizedCategory);

                return new OkObjectResult(existingItem);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON received while updating menu item.");

                return new BadRequestObjectResult("The request body contains invalid JSON.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu item {MenuId}.", menuId);

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }