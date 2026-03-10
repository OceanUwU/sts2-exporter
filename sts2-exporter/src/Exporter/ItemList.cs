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
    public List<RelicExport> Relics = [];
    [JsonInclude][JsonPropertyName("potions")]
    public List<PotionExport> Potions = [];
    [JsonInclude][JsonPropertyName("events")]
    public List<EventExport> Events = [];
    [JsonInclude][JsonPropertyName("creatures")]
    public List<CreatureExport> Creatures = [];
    [JsonInclude][JsonPropertyName("enchantments")]
    public List<EnchantmentExport> Enchantments = [];
    [JsonInclude][JsonPropertyName("keywords")]
    public List<KeywordExport> Keywords = [];
    [JsonInclude][JsonPropertyName("afflictions")]
    public List<AfflictionExport> Afflictions = [];

    public ItemList() {}

    public ItemList(ModExport mod) : this() {
        this.mod = mod;
    }

    public void Add(ItemExport item) {
             if (item is CardExport c) Cards.Add(c);
        else if (item is RelicExport r) Relics.Add(r);
        else if (item is PotionExport p) Potions.Add(p);
        else if (item is CreatureExport cr) Creatures.Add(cr);
        else if (item is KeywordExport k) Keywords.Add(k);
        else if (item is EventExport e) Events.Add(e);
        else if (item is EnchantmentExport ench) Enchantments.Add(ench);
        else if (item is AfflictionExport a) Afflictions.Add(a);
    }

    public void RemoveIf(Func<ItemExport, bool> predicate) {
        Func<ItemExport, bool> p = c => !predicate(c);
        Cards = [..Cards.Where(p).Cast<CardExport>()];
        Relics = [..Relics.Where(p).Cast<RelicExport>()];
        Potions = [..Potions.Where(p).Cast<PotionExport>()];
        Events = [..Events.Where(p).Cast<EventExport>()];
        Creatures = [..Creatures.Where(p).Cast<CreatureExport>()];
        Keywords = [..Keywords.Where(p).Cast<KeywordExport>()];
        Enchantments = [..Enchantments.Where(p).Cast<EnchantmentExport>()];
        Afflictions = [..Afflictions.Where(p).Cast<AfflictionExport>()];
    }

    public void FindAll() {
        Cards.AddRange(CardExport.FindAll());
        Relics.AddRange(RelicExport.FindAll());
        Potions.AddRange(PotionExport.FindAll());
        Events.AddRange(EventExport.FindAll());
        Creatures.AddRange(CreatureExport.FindAll());
        Keywords.AddRange(KeywordExport.FindAll());
        Enchantments.AddRange(EnchantmentExport.FindAll());
        Afflictions.AddRange(AfflictionExport.FindAll());
    }

    public List<ItemExport> All() => [ ..Cards, ..Relics, ..Potions, ..Events, ..Creatures, ..Keywords, ..Enchantments, ..Afflictions ];
}