// ItemDataLoader_Dapper.cs
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Unity.VisualScripting.Dependencies.Sqlite;

public class Tag
{
    public string TagId { get; set; }
}

public static class ItemDataLoader
{
    public static List<ItemData> LoadAllItems(string dbPath)
    {
        List<ItemData> items = new List<ItemData>();

        string connectionString = $"URI=file:{dbPath}";

        using (var connection = new SQLiteConnection(connectionString))
        {

            // Get base items
            var itemRows = connection.Query<(string ItemId, string Name, int StackLimit)>(
                "SELECT ItemId, Name, StackLimit FROM Items;"
            );

            foreach (var row in itemRows)
            {
                var itemId = row.ItemId;

                // Tags: simple list
                var tags = connection.Query<Tag>(
                    @"SELECT T.TagId
                      FROM ItemTags IT
                      JOIN Tags T ON IT.TagId = T.TagId
                      WHERE IT.ItemId = @ItemId;",
                    new { ItemId = itemId }
                ).Select(tag => tag.TagId).ToList();

                // Stats: key-value pairs
                var statPairs = connection.Query<(string StatId, float Value)>(
                    @"SELECT S.StatId, ISI.Value
                      FROM ItemStats ISI
                      JOIN Stats S ON ISI.StatId = S.StatId
                      WHERE ISI.ItemId = @ItemId;",
                    new { ItemId = itemId }
                );

                var stats = new Dictionary<string, float>();
                foreach (var pair in statPairs)
                {
                    stats[pair.StatId] = pair.Value;
                }

                ItemData item = new ItemData
                {
                    itemId = row.ItemId,
                    itemName = row.Name,
                    iconId = row.ItemId, // or whatever logic you want
                    stackSize = row.StackLimit,
                    tags = tags,
                    stats = stats
                };

                items.Add(item);
            }
        }

        return items;
    }
}
