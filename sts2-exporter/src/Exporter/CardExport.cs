using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2Export.Exporter;

public class CardExport : ItemExport, IImageExport {
    private static readonly PackedScene CardScene = GD.Load<PackedScene>((string)typeof(NCard).GetField("_scenePath", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
    private static readonly Vector2I ImgSize = new(734, 916);
    
    private readonly CardModel model;

    public CardExport(CardModel model, int upgrades) {
        this.model = model.ToMutable();
        this.upgrades = upgrades;
        for (int i = 0; i < this.upgrades; i++)
            this.model.UpgradeInternal();
    }

    [JsonInclude][JsonPropertyName("id")]
    private string ID => model.Id.Entry;
    [JsonInclude][JsonPropertyName("name")]
    private string Name => model.TitleLocString.GetFormattedText();
    [JsonInclude][JsonPropertyName("color")]
    private string Color => model.VisualCardPool.Title.ToLower();
    [JsonInclude][JsonPropertyName("rarity")]
    private string Rarity => model.Rarity.ToString();
    [JsonInclude][JsonPropertyName("type")]
    private string Type => model.Type.ToString();
    [JsonInclude][JsonPropertyName("cost")]
    private int Cost => model.EnergyCost.CostsX ? -1 : model.EnergyCost.GetWithModifiers(CostModifiers.None);
    [JsonInclude][JsonPropertyName("description")]
    private string Description => StripBBCodeTags(model.GetDescriptionForPile(PileType.None));
    [JsonInclude][JsonPropertyName("upgrades")]
    private readonly int upgrades;

    [JsonIgnore]
    public string ImgPath => "card-images";
    [JsonIgnore]
    public string ImgFilename => upgrades == 0 ? ID : $"{ID}Plus{upgrades}";
    public ViewportManager.DrawRequest ExportImg() => new(ImgSize, null, drawer => {
        NCard card = CardScene.Instantiate<NCard>();
        drawer.AddChild(card);
        card.Scale = Vector2.One * 2f;
        card.Modulate = Colors.White;
        card.Position = (Vector2)ImgSize / 2f;
        card.Model = model;
        card.UpdateVisuals(MegaCrit.Sts2.Core.Entities.Cards.PileType.None, MegaCrit.Sts2.Core.Entities.Cards.CardPreviewMode.Normal);
        if (upgrades > 0 && model.HasBetaPortrait)
            card.GetNode<TextureRect>("%Portrait").Texture = ResourceLoader.Load<Texture2D>(model.BetaPortraitPath, null, ResourceLoader.CacheMode.Reuse);;
    });

    public static List<CardExport> FindAll() => [..ModelDb.AllCards.SelectMany(m => Enumerable.Range(0, m.MaxUpgradeLevel + 1).Select(u => new CardExport(m, u)))];
}