namespace POE_CoffeeNChill.Models;

public class CreateMenuItemRequest
{
    public string? PartitionKey { get; set; }

    public string? RowKey { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public double Price { get; set; }

    public bool IsAvailable { get; set; }
}

public class UpdateMenuItemRequest
{
    public double? Price { get; set; }

    public bool? IsAvailable { get; set; }
}