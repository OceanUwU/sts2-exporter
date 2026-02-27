using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace STS2Export.Exporter;

public class PotionExport(PotionModel model) : ItemExport, IImageExport {
    private static readonly Vector2I ImgSize = new(64, 64);

    private readonly PotionModel model = model.ToMutable();

    [JsonInclude][JsonPropertyName("id")]
    private string ID => model.Id.Entry;
    [JsonInclude][JsonPropertyName("name")]
    private string Name => model.Title.GetFormattedText();
    [JsonInclude][JsonPropertyName("pool")]
    private string Pool => model.Pool.EnergyColorName.ToLower();
    [JsonInclude][JsonPropertyName("tier")]
    private string Rarity => model.Rarity.ToString();
    [JsonInclude][JsonPropertyName("description")]
    private string Description => StripBBCodeTags(model.DynamicDescription.GetFormattedText());
    
    [JsonIgnore]
    public string ImgPath => "potions";
    [JsonIgnore]
    public string ImgFilename => ID;
    public ViewportManager.DrawRequest ExportImg() => new(ImgSize, null, drawer => {
        NPotion potion = NPotion.Create(model);
        drawer.AddChild(potion);
        potion.Modulate = Colors.White;
        potion.Position = (Vector2)ImgSize / 2f - potion.Size / 2f;
        potion.Model = model;
    });

    public static List<PotionExport> FindAll() => [..ModelDb.AllPotions.Select(m => new PotionExport(m))];
}