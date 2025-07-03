using System.Collections.Generic;
using System.Linq;
using SQLite;

public class SpriteData
{
    public string SpriteId { get; set; }
    public string FilePath { get; set; }
    public int? Frame { get; set; }
    public string Notes { get; set; }
}

public static class SpriteDataLoader
{
    public static List<SpriteData> LoadAllSprites(string dbPath)
    {
        List<SpriteData> sprites = new List<SpriteData>();

        string connectionString = dbPath;

        using (var connection = new SQLiteConnection(connectionString))
        {
            var spriteRows = connection.Query<(string SpriteId, string FilePath, int? Frame, string Notes)>(
                "SELECT SpriteId, FilePath, Frame, Notes FROM Sprites;"
            ).ToList();

            foreach (var row in spriteRows)
            {
                SpriteData sprite = new SpriteData
                {
                    SpriteId = row.SpriteId,
                    FilePath = row.FilePath,
                    Frame = row.Frame,
                    Notes = row.Notes
                };

                sprites.Add(sprite);
            }
        }

        return sprites;
    }
}
