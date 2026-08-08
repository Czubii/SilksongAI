using LiteDB;
using System.Linq;
using System.Text.Json;

public static class LiteDBRepositoryExtensions
{
    /// <summary>
    /// Debug-prints all typed entities in the repository using PluginLog.Info.
    /// </summary>
    public static void DebugPrintAll<TData>(this LiteDBRepository<TData> repository)
    {
        var items = repository.All;
        string header = $"=== Repository Dump [{typeof(TData).Name}] ({items.Count} items) ===";

        PluginLog.Info(header);

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        int index = 0;
        foreach (var item in items)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(item, options);
            PluginLog.Info($"[{index++}] {json}");
        }

        string footer = new string('=', header.Length);
        PluginLog.Info(footer);
    }

    /// <summary>
    /// Bypasses C# model mapping and debug-prints raw LiteDB BSON documents directly from the database file.
    /// Useful for verifying mapped primary keys (_id) and field names.
    /// </summary>
    public static void DebugPrintRawBson(this string dbPath, string collectionName = "recordings")
    {
        using (LiteDatabase db = new LiteDatabase(dbPath))
        {
            ILiteCollection<BsonDocument> col = db.GetCollection(collectionName);
            var docs = col.FindAll().ToList();

            string header = $"=== Raw BSON Dump [{collectionName}] ({docs.Count} docs) ===";
            PluginLog.Info(header);

            int index = 0;
            foreach (BsonDocument doc in docs)
            {
                // LiteDB's builtin JsonSerializer formats BsonDocuments safely
                string rawJson = LiteDB.JsonSerializer.Serialize(doc);
                PluginLog.Info($"[{index++}] {rawJson}");
            }

            string footer = new string('=', header.Length);
            PluginLog.Info(footer);
        }
    }
}