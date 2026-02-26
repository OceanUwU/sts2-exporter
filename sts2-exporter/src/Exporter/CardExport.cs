using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2Export.Exporter;

public class CardExport(CardModel model) : ItemExport, IImageExport {
    private static readonly PackedScene CardScene = GD.Load<PackedScene>((string)typeof(NCard).GetField("_scenePath", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
    private static readonly Vector2I ImgSize = new(676, 916);
    
    private readonly CardModel model = model;

    [JsonInclude][JsonPropertyName("id")]
    private string ID => model.Id.Entry;
    [JsonInclude][JsonPropertyName("name")]
    private string name => model.TitleLocString.GetFormattedText();
    [JsonInclude][JsonPropertyName("color")]
    private string color => model.VisualCardPool.Title.ToLower();

    public static List<CardExport> FindAll() => [..ModelDb.AllCards.Select(m => new CardExport(m))];

    [JsonIgnore]
    public string ImgPath => "card-images";
    public List<(string, ViewportManager.DrawRequest)> ExportImgs() {
        List<(string, ViewportManager.DrawRequest)> imgs = [];
        for (int upgradeLevel = 0; upgradeLevel <= model.MaxUpgradeLevel; upgradeLevel++) {
            CardModel modelCopy = model.ToMutable();
            for (int i = 0; i < upgradeLevel; i++)
                modelCopy.UpgradeInternal();
            string filename = upgradeLevel == 0 ? ID : $"{ID}Plus{upgradeLevel}";
            imgs.Add((filename, new(ImgSize, null, drawer => {
                NCard card = CardScene.Instantiate<NCard>();
                drawer.AddChild(card);
                card.Scale = Vector2.One * 2f;
                card.Modulate = Colors.White;
                card.Position = (Vector2)ImgSize / 2f;
                card.Model = modelCopy;
                card.UpdateVisuals(MegaCrit.Sts2.Core.Entities.Cards.PileType.None, MegaCrit.Sts2.Core.Entities.Cards.CardPreviewMode.Normal);
            })));
        }
        return imgs;
    }
}