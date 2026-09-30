using System;
using System.Collections.Generic;
using System.Text;
using Azure;
using Azure.Data.Tables;

namespace POE_CoffeeNChill.Models
{
    public class MenuItems : ITableEntity
    {
        public enum Category // Shows the available menu items, inner eum
        {
            Cold_Drinks,
            Hot_Drinks,
            Burgers,
            Sandwiches,
            Pizza
        }
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty; // Unique Item SKU / ID (e.g., "COF-001"), acting as the menu id
        public string? Name { get; set; } = string.Empty; // Item name (e.g., "Chicken Pie", "Margherita Pizza")
        public string? Description { get; set; } = string.Empty; // Short menu description
        public double Price { get; set; } // Item price
        public bool? IsAvailable { get; set; } // Availability status
        public DateTimeOffset? Timestamp { get; set; } // Time for checking the item
        public ETag ETag { get; set; } // Tags to the table

    }
}



