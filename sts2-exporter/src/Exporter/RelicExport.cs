using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.InspectScreens;

namespace STS2Export.Exporter;

public class RelicExport : ItemExport, IImageExport {
    private static readonly Vector2I ImgSize = new(200, 200);
    private const float TipScale = 2f;
    private const float TipCardGap = 10f;
    private const float TipTopMargin = 20f;

    private readonly RelicModel model;

    public RelicExport(RelicModel model) {
        Assembly = model.GetType().Assembly;
        this.model = model;
    }

    [JsonInclude][JsonPropertyName("id")]
    public string ID => model.Id.Entry;
    [JsonInclude][JsonPropertyName("name")]
    public string Name => model.Title.GetFormattedText();
    [JsonInclude][JsonPropertyName("pool")]
    public string Pool => model.Pool.EnergyColorName.ToLower();
    [JsonInclude][JsonPropertyName("ancient")]
    public string Ancient => ModelDb.AllAncients.FirstOrDefault(m => m.AllPossibleOptions.Select(static o => o.Relic.Id.Entry).Contains(model.Id.Entry))?.Title.GetRawText();
    [JsonInclude][JsonPropertyName("tier")]
    public string Rarity => model.Rarity.ToString();
    [JsonInclude][JsonPropertyName("description")]
    public string Description => StripBBCodeTags(model.DynamicDescription.GetFormattedText(), EnergyIconHelper.GetPrefix(model));
    [JsonInclude][JsonPropertyName("flavorText")]
    public string Flavor => StripBBCodeTags(model.Flavor.GetFormattedText(), EnergyIconHelper.GetPrefix(model));
    
    private static TextureRect exampleTexRect;
    public ViewportManager.DrawRequest[] ExportImg(ExportConfig config) {
        ViewportManager.DrawRequest relicImg = new(ImgSize, $"relics/{ID}", null, drawer => {
            if (exampleTexRect == null) {
                var screen = NInspectRelicScreen.Create();
                exampleTexRect = screen.GetNode<TextureRect>("%RelicImage");
            }
            TextureRect textureRect = (TextureRect)exampleTexRect.Duplicate();
            drawer.AddChild(textureRect);
            textureRect.SelfModulate = Colors.White;
            textureRect.Texture = model.BigIcon;
            textureRect.Position = (Vector2)ImgSize / 2f - textureRect.Size / 2f;
        });
        Control hoverTipParent = new();
        var tipSet = NHoverTipSet.CreateAndShow(hoverTipParent, HoverTipFactory.FromRelic(model));
        tipSet.Reparent(hoverTipParent);
        tipSet._cardHoverTipContainer.LayoutResizeAndReposition(Vector2.Zero, HoverTipAlignment.Center);
        Vector2I tipSize = (Vector2I)(new Vector2(tipSet._textHoverTipContainer.Size.X + (tipSet._cardHoverTipContainer.Size.X > 0 ? TipCardGap : 0) + tipSet._cardHoverTipContainer.Size.X, Mathf.Max(tipSet._textHoverTipContainer.Size.Y, tipSet._cardHoverTipContainer.Size.Y + TipTopMargin * TipScale)) * TipScale);
        ViewportManager.DrawRequest tipImg = new(tipSize, $"relic-tips/{ID}", null, drawer =>{
            drawer.AddChild(hoverTipParent);
            tipSet.Scale *= TipScale;
            tipSet.PivotOffset = Vector2.Zero;
            tipSet._cardHoverTipContainer.Position = new(tipSet._textHoverTipContainer.Size.X + TipCardGap, TipTopMargin);
        });
        return [relicImg, tipImg];
    }

    public static List<RelicExport> FindAll() => [..ModelDb.AllRelics.Select(m => new RelicExport(m))];
}