using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace STS2Export.Exporter;

public class ModExport {
    [JsonInclude][JsonPropertyName("id")]
    private readonly string id = "basegame";
    [JsonInclude][JsonPropertyName("name")]
    private readonly string name = "Slay the Spire 2";
    [JsonInclude][JsonPropertyName("version")]
    private readonly string version;
    [JsonInclude][JsonPropertyName("authors")]
    private readonly string[] authors = [];
    [JsonInclude][JsonPropertyName("credits")]
    private readonly string credits;
    [JsonInclude][JsonPropertyName("description")]
    private readonly string description;

    private readonly bool isBasegame;

    public ModExport() {
        isBasegame = id == "basegame";
    }

    private readonly ItemList items = new();

    public void FindAll() => items.FindAll(); //TEMP

    public void AddItem(ItemExport item) {
        item.Mod = this;
        items.Add(item);
    }

    public void Export(string basePath) {
        string dir = $"{basePath}/{id}";
        DirAccess.MakeDirRecursiveAbsolute(dir);
        foreach (var item in items.All()) {
            if (item is IImageExport imageExport) {
                ViewportManager.RequestDraw(imageExport.ExportImg()).ContinueWith(task => {
                    Image img = task.Result;
                    string imgDir = $"{dir}/{imageExport.ImgPath}";
                    DirAccess.MakeDirRecursiveAbsolute(imgDir);
                    img.SavePng($"{imgDir}/{imageExport.ImgFilename}.png");
                });
            }
        }
        FileAccess file = FileAccess.Open($"{dir}/items.json", FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(items, new JsonSerializerOptions() { WriteIndented = true }));
        file.Close();
    }
}