using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace STS2Export.Exporter;

public class ModExport {
    [JsonInclude][JsonPropertyName("id")]
    public readonly string ID = "slay-the-spire-2";
    [JsonInclude][JsonPropertyName("name")]
    public readonly string Name = "Slay the Spire 2";
    [JsonInclude][JsonPropertyName("version")]
    private readonly string version = "";
    [JsonInclude][JsonPropertyName("authors")]
    private readonly string[] authors = [];
    [JsonInclude][JsonPropertyName("credits")]
    private readonly string credits = "";
    [JsonInclude][JsonPropertyName("description")]
    private readonly string description = "";
    [JsonInclude][JsonPropertyName("stsVersion")]
    private readonly byte slayTheSpireVersion = 2;

    public readonly bool IsBasegame;

    public ModExport() {
        IsBasegame = ID == "slay-the-spire-2";
        items = new(this);
    }

    private readonly ItemList items;

    public void FindAll() => items.FindAll(); //TEMP

    public void AddItem(ItemExport item) {
        item.Mod = this;
        items.Add(item);
    }

    public void Export(string basePath) {
        string dir = $"{basePath}/{ID}";
        DirAccess.MakeDirRecursiveAbsolute(dir);
        FileAccess file = FileAccess.Open($"{dir}/items.json", FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(items, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        file.Close();
    }
}