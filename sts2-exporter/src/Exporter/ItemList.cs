using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace STS2Export.Exporter;

public class ItemList {
    [JsonInclude][JsonPropertyName("cards")]
    private readonly List<CardExport> cards = [];
    [JsonInclude][JsonPropertyName("relics")]
    private readonly List<RelicExport> relics = [];
    [JsonInclude][JsonPropertyName("potions")]
    private readonly List<PotionExport> potions = [];
    [JsonInclude][JsonPropertyName("creatures")]
    private readonly List<CreatureExport> creatures = [];
    [JsonInclude][JsonPropertyName("keywords")]
    private readonly List<KeywordExport> keywords = [];

    public void Add(ItemExport item) {
             if (item is CardExport c) cards.Add(c);
        else if (item is RelicExport r) relics.Add(r);
        else if (item is PotionExport p) potions.Add(p);
        else if (item is CreatureExport cr) creatures.Add(cr);
        else if (item is KeywordExport k) keywords.Add(k);
    }

    public void FindAll() {
        cards.AddRange(CardExport.FindAll());
        relics.AddRange(RelicExport.FindAll());
        potions.AddRange(PotionExport.FindAll());
        creatures.AddRange(CreatureExport.FindAll());
        keywords.AddRange(KeywordExport.FindAll());
    }

    public List<ItemExport> All() => [..cards, ..relics, ..potions, ..creatures, ..keywords, ];
}