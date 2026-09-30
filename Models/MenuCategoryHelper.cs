namespace POE_CoffeeNChill.Models;

public static class MenuCategoryHelper
{
    public static string Normalize(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return string.Empty;
        }

        return Uri.UnescapeDataString(category).Replace("_", " ") .Trim();
    }

    public static bool IsValid(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return false;
        }

        // Azure Table PartitionKey values should not contain
        // these reserved characters.
        char[] invalidCharacters =
        {
            '/', '\\', '#', '?'
        };

        return category.IndexOfAny(invalidCharacters) == -1;
    }
}