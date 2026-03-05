using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2Export.Exporter;

public class AfflictionExport : ItemExport, IImageExport {
    private static readonly Vector2I ImgSize = new(734/2, 916/2);

    private readonly AfflictionModel model;

    public AfflictionExport(AfflictionModel model) {
        this.model = model.ToMutable();
        this.model.Amount = 999;
    }

    [JsonInclude][JsonPropertyName("id")]
    private string ID => model.Id.Entry;
    [JsonInclude][JsonPropertyName("name")]
    private string Name => model.Title.GetFormattedText();
    [JsonInclude][JsonPropertyName("description")]
    private string Description => StripBBCodeTags(model.DynamicDescription.GetFormattedText().Replace("999", "N"), EnergyIconHelper.GetPrefix(model));
    
    public ViewportManager.DrawRequest[] ExportImg(ExportConfig config) => [new(ImgSize, $"afflictions/{ID}", null, drawer => {
        var cardModel = ((CardModel)ModelDb.Get(typeof(UltimateDefend))).ToMutable();
        cardModel.AfflictInternal(model, model.Amount);
        NCard card = CardExport.CardScene.Instantiate<NCard>();
        drawer.AddChild(card);
        card.Modulate = Colors.White;
        card.Position = (Vector2)ImgSize / 2f;
        card.Model = cardModel;
        card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
        card._descriptionLabel.SetTextAutoSize(card._descriptionLabel.Text.Replace("999", "N"));
    })];

    public static List<AfflictionExport> FindAll() => [..ModelDb.DebugAfflictions.Select(static m => new AfflictionExport(m))];//[..new List<Assembly>([typeof(NGame).Assembly, ..ModManager.AllMods.Select(static m => m.assembly).Where(static a => a != null)]).SelectMany(static a => a.GetTypes()).Where(static t => t.IsAssignableTo(typeof(AfflictionModel)) && !t.IsAbstract).Select(static t => new AfflictionExport((AfflictionModel)ModelDb.Get(t)))];
}