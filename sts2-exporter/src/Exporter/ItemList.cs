using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace STS2Export.Exporter;

public class ItemList {
    [JsonInclude][JsonPropertyName("mod")]
    private readonly ModExport mod = null;
    [JsonInclude][JsonPropertyName("cards")]
    public List<CardExport> Cards = [];
    [JsonInclude][JsonPropertyName("relics")]
    private List<RelicExport> relics = [];
    [JsonInclude][JsonPropertyName("potions")]
    private List<PotionExport> potions = [];
    [JsonInclude][JsonPropertyName("events")]
    private List<EventExport> events = [];
    [JsonInclude][JsonPropertyName("creatures")]
    private List<CreatureExport> creatures = [];
    [JsonInclude][JsonPropertyName("enchantments")]
    private List<EnchantmentExport> enchantments = [];
    [JsonInclude][JsonPropertyName("keywords")]
    private List<KeywordExport> keywords = [];

    public ItemList() {}

    public ItemList(ModExport mod) : this() {
        this.mod = mod;
    }

    public void Add(ItemExport item) {
             if (item is CardExport c) Cards.Add(c);
        else if (item is RelicExport r) relics.Add(r);
        else if (item is PotionExport p) potions.Add(p);
        else if (item is CreatureExport cr) creatures.Add(cr);
        else if (item is KeywordExport k) keywords.Add(k);
        else if (item is EventExport e) events.Add(e);
        else if (item is EnchantmentExport ench) enchantments.Add(ench);
    }

    public void RemoveIf(Func<ItemExport, bool> predicate) {
        Func<ItemExport, bool> p = c => !predicate(c);
        Cards = [..Cards.Where(p).Cast<CardExport>()];
        relics = [..relics.Where(p).Cast<RelicExport>()];
        potions = [..potions.Where(p).Cast<PotionExport>()];
        events = [..events.Where(p).Cast<EventExport>()];
        creatures = [..creatures.Where(p).Cast<CreatureExport>()];
        keywords = [..keywords.Where(p).Cast<KeywordExport>()];
        enchantments = [..enchantments.Where(p).Cast<EnchantmentExport>()];
    }

    public void FindAll() {
        Cards.AddRange(CardExport.FindAll());
        relics.AddRange(RelicExport.FindAll());
        potions.AddRange(PotionExport.FindAll());
        events.AddRange(EventExport.FindAll());
        creatures.AddRange(CreatureExport.FindAll());
        keywords.AddRange(KeywordExport.FindAll());
        enchantments.AddRange(EnchantmentExport.FindAll());
    }

    public List<ItemExport> All() => [ ..Cards, ..relics, ..potions, ..events, ..creatures, ..keywords, ..enchantments, ];
}