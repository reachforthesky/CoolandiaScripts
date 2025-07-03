using SQLite;
using System.Linq;
using System.Collections.Generic;

public static class ItemDataLoader
{
    public static List<ItemData> LoadAllItems(string dbPath)
    {
        List<ItemData> items = new();

        var connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadOnly);

        var itemRows = connection.Query<ItemRow>("SELECT ItemId, Name, IconSpriteId, StackLimit FROM Items;");

        foreach (var row in itemRows)
        {
            var tags = connection.Query<Tag>(
                @"SELECT T.TagId FROM ItemTags IT JOIN Tags T ON IT.TagId = T.TagId WHERE IT.ItemId = ?;",
                row.ItemId
            ).Select(row=>row.TagId).ToList();


            var statPairs = connection.Query<StatPair>(
                @"SELECT S.StatId, ISI.Value FROM ItemStats ISI JOIN Stats S ON ISI.StatId = S.StatId WHERE ISI.ItemId = ?;",
                row.ItemId
            );

            Dictionary<string, float> stats = statPairs.ToDictionary(row => row.StatId, row => row.Value);

            items.Add(new ItemData
            {
                itemId = row.ItemId,
                itemName = row.Name,
                iconId = row.IconSpriteId ?? "",
                stackSize = row.StackLimit,
                tags = tags,
                stats = stats
            });
        }

        connection.Close();
        return items;
    }
}

public class ItemRow
{
    [PrimaryKey]
    public string ItemId { get; set; }
    public string Name { get; set; }
    public string IconSpriteId { get; set; }
    public int StackLimit { get; set; }
}
public class  Tag
{
    public string TagId { get; set; }
}

public class StatPair
{
    public string StatId { get; set; }
    public float Value { get; set; }
}
