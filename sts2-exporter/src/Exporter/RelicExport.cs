using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.InspectScreens;

namespace STS2Export.Exporter;

public class RelicExport(RelicModel model) : ItemExport, IImageExport {
    private static readonly Vector2I ImgSize = new(200, 200);

    private readonly RelicModel model = model;

    [JsonInclude][JsonPropertyName("id")]
    private string ID => model.Id.Entry;
    [JsonInclude][JsonPropertyName("name")]
    private string Name => model.Title.GetFormattedText();
    [JsonInclude][JsonPropertyName("pool")]
    private string Pool => model.Pool.EnergyColorName.ToLower();
    //
    // TODO: find which ancient the relics come from ??
    //
    //[JsonInclude][JsonPropertyName("ancient")]
    //private string Ancient => model.
    [JsonInclude][JsonPropertyName("tier")]
    private string Rarity => model.Rarity.ToString();
    [JsonInclude][JsonPropertyName("description")]
    private string Description => StripBBCodeTags(model.DynamicDescription.GetFormattedText(), EnergyIconHelper.GetPrefix(model));
    [JsonInclude][JsonPropertyName("flavorText")]
    private string Flavor => StripBBCodeTags(model.Flavor.GetFormattedText(), EnergyIconHelper.GetPrefix(model));
    
    private static TextureRect exampleTexRect;
    public ViewportManager.DrawRequest[] ExportImg(ExportConfig config) => [new(ImgSize, $"relics/{ID}", null, drawer => {
        if (exampleTexRect == null) {
            var screen = NInspectRelicScreen.Create();
            exampleTexRect = screen.GetNode<TextureRect>("%RelicImage");
        }
        TextureRect textureRect = (TextureRect)exampleTexRect.Duplicate();
        drawer.AddChild(textureRect);
        textureRect.SelfModulate = Colors.White;
        textureRect.Texture = model.BigIcon;
        textureRect.Position = (Vector2)ImgSize / 2f - textureRect.Size / 2f;
    })];

    public static List<RelicExport> FindAll() => [..ModelDb.AllRelics.Select(m => new RelicExport(m))];
}