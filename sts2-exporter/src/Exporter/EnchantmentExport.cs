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

public class EnchantmentExport : ItemExport, IImageExport {
    private static readonly Vector2I ImgSize = new(68, 54);

    private readonly EnchantmentModel model;

    public EnchantmentExport(EnchantmentModel model) {
        Assembly = model.GetType().Assembly;
        this.model = model.ToMutable();
        this.model.Amount = 999;
    }

    [JsonInclude][JsonPropertyName("id")]
    private string ID => model.Id.Entry;
    [JsonInclude][JsonPropertyName("name")]
    private string Name => model.Title.GetFormattedText();
    [JsonInclude][JsonPropertyName("description")]
    private string Description => StripBBCodeTags(model.DynamicDescription.GetFormattedText().Replace("999", "N"), EnergyIconHelper.GetPrefix(model));
    
    public ViewportManager.DrawRequest[] ExportImg(ExportConfig config) => [new(ImgSize, $"enchantments/{ID}", null, drawer => {
        var cardModel = ((CardModel)ModelDb.Get(typeof(UltimateDefend))).ToMutable();
        NCard card = CardExport.CardScene.Instantiate<NCard>();
        drawer.AddChild(card);
        card.Model = cardModel;
        cardModel.Enchantment = model;
        card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
        drawer.RemoveChild(card);
        Control tab = new() { Size = Vector2.Zero };
        //card._enchantmentLabel.Modulate = Colors.Transparent;
        if (model.ShowAmount)
            card._enchantmentLabel.Text = "N";
        card._enchantmentTab.Reparent(tab);
        drawer.AddChild(tab);
        tab.Position = new(164, 161);
        card.QueueFree();
    })];

    public static List<EnchantmentExport> FindAll() => [..ModelDb.DebugEnchantments.Select(static m => new EnchantmentExport(m))];//[..new List<Assembly>([typeof(NGame).Assembly, ..ModManager.AllMods.Select(static m => m.assembly).Where(static a => a != null)]).SelectMany(static a => a.GetTypes()).Where(static t => t.IsAssignableTo(typeof(EnchantmentModel)) && !t.IsAbstract).Select(static t => new EnchantmentExport((EnchantmentModel)ModelDb.Get(t)))];
}