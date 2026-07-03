using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;

namespace STS2Export.Exporter;

public class ModExport {
    [JsonInclude][JsonPropertyName("id")]
    public readonly string ID;
    [JsonInclude][JsonPropertyName("name")]
    public readonly string Name;
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

    [JsonIgnore]
    public readonly bool IsBasegame = false;
    [JsonIgnore]
    public readonly List<Assembly> Assemblies;

    private ModExport() {
        Items = new(this);
    }

    public ModExport(Mod mod) : this() {
        if (mod == null) {
            ID = "slay-the-spire-2";
            Name = "Slay the Spire 2";
            Assemblies = [typeof(NGame).Assembly];
            IsBasegame = true;
        } else {
            ID = mod.manifest.id;
            Name = mod.manifest.name;
            version = mod.manifest.version;
            authors = [mod.manifest.author];
            description = mod.manifest.description;
            Assemblies = mod.assemblies;
        }
    }

    public readonly ItemList Items;

    public void AddItem(ItemExport item) {
        item.Mod = this;
        Items.Add(item);
    }

    public void Export(string basePath) {
        string dir = $"{basePath}/{ID}";
        DirAccess.MakeDirRecursiveAbsolute(dir);
        FileAccess file = FileAccess.Open($"{dir}/items.json", FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(Items, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        file.Close();
    }
}