using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2Export.Exporter;

public class CardExport : ItemExport, IImageExport {
    private static readonly PackedScene CardScene = GD.Load<PackedScene>((string)typeof(NCard).GetField("_scenePath", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
    private static readonly Vector2I ImgSize = new(734, 916);
    
    private readonly CardModel model;

    public CardExport(CardModel model, int upgrades) {
        this.model = model.ToMutable();
        this.Upgrades = upgrades;
        for (int i = 0; i < this.Upgrades; i++)
            this.model.UpgradeInternal();
    }

    [JsonInclude][JsonPropertyName("id")]
    public string ID => model.Id.Entry;
    [JsonInclude][JsonPropertyName("name")]
    public string Name => StripBBCodeTags(model.Title, EnergyIconHelper.GetPrefix(model));
    [JsonInclude][JsonPropertyName("color")]
    public string Color => model.VisualCardPool.Title.ToLower();
    [JsonInclude][JsonPropertyName("rarity")]
    public string Rarity => model.Rarity.ToString();
    [JsonInclude][JsonPropertyName("type")]
    public string Type => model.Type.ToString();
    [JsonInclude][JsonPropertyName("cost")]
    public string Cost {
        get {
            if (model.EnergyCost.CostsX) return "X";
            var cost = model.EnergyCost.GetWithModifiers(CostModifiers.None);
            return cost == -1 ? "" : cost.ToString();
        }
    }
    [JsonInclude][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)][JsonPropertyName("starCost")]
    public int? StarCost => model.CanonicalStarCost == -1 ? null : (model.HasStarCostX ? -1 : model.CanonicalStarCost);
    [JsonInclude][JsonPropertyName("description")]
    public string Description => StripBBCodeTags(model.GetDescriptionForPile(PileType.None), EnergyIconHelper.GetPrefix(model));
    [JsonInclude][JsonPropertyName("upgrades")]
    public readonly int Upgrades;
    [JsonIgnore]
    public CardExport UpgradedVersion => Upgrades >= model.MaxUpgradeLevel ? null : new(model.CanonicalInstance, Upgrades + 1);
    private string UpgradeDesc => UpgradedVersion is CardExport up ? up.Description : Description;
    [JsonIgnore]
    public string TextAndUpgrade => ProcessCombinedDescription(CombineDescriptions(Description, UpgradeDesc, TextMode.Normal), TextMode.Normal);
    [JsonIgnore]
    public string TextWikiData => ProcessCombinedDescription(CombineDescriptions(Description, UpgradeDesc, TextMode.WikiData), TextMode.WikiData);
    [JsonIgnore]
    public string TextWikiFormat => ProcessCombinedDescription(CombineDescriptions(Description, UpgradeDesc, TextMode.WikiFormat), TextMode.WikiFormat);

    private enum TextMode { Normal, WikiData, WikiFormat }

    private static string CombineDescriptions(string a, string b, TextMode mode) {
        static string PreProcessText(string a, TextMode mode) {
            a = a.Replace("."," .").Replace(","," ,").Replace("\n", " \n ");
            if (mode == TextMode.WikiData || mode == TextMode.WikiFormat)
                a = a.Replace("[E]", "<E>");
            return a;
        }

        static int WordCost(string aW, string bW, TextMode mode) {
            if (aW == bW) return 0;
            if (mode != TextMode.WikiData && bW == aW + "s") return 10;
            return 21;
        }

        static string WordReplacement(string aW, string bW, TextMode mode) {
            if (aW == bW) return aW;
            if (mode != TextMode.WikiData && bW == aW + "s") return aW + "(s)";
            return aW + " (" + bW + ")";
        }

        // Combine description with upgrade description
        if (a == b && mode == TextMode.Normal) return a;
        // prepare punctuation, so we count it as separate words
        a = PreProcessText(a, mode);
        b = PreProcessText(b, mode);
        // Split input into words
        string[] aWords = a.Split(' ');
        string[] bWords = b.Split(' ');
        // Use the standard sequence alignment algorithm (Needleman–Wunsch)
        int INSERT_A = 10;
        int INSERT_B = 10;
        int[,] score = new int[aWords.Length+1, bWords.Length+1];
        for (int aI=0; aI <= aWords.Length; aI++) {
            score[aI, 0] = aI * INSERT_A;
        }
        for (int bI=0; bI <= bWords.Length; bI++) {
            score[0, bI] = bI * INSERT_B;
        }
        for (int aI=1; aI <= aWords.Length; aI++) {
            for (int bI=1 ; bI <= bWords.Length ; bI++) {
                score[aI, bI] = Math.Min(score[aI-1, bI] + INSERT_A,
                                Math.Min(score[aI, bI-1] + INSERT_B,
                                        score[aI-1, bI-1] + WordCost(aWords[aI-1],bWords[bI-1],mode)));
            }
        }
        // Now return the optimal alignment, first in reverse order
        List<string> words = [];
        List<char> source = [];
        int ai=aWords.Length, bi=bWords.Length;
        while (ai > 0 && bi > 0) {
            int acost       = score[ai-1,bi] + INSERT_A;
            int bcost       = score[ai,bi-1] + INSERT_B;
            int replacecost = score[ai-1,bi-1] + WordCost(aWords[ai-1],bWords[bi-1],mode);
            if (bcost <= acost && bcost <= replacecost) {
                words.Add(bWords[bi-1]);
                source.Add('b');
                bi--;
            } else if (acost <= replacecost) {
                words.Add(aWords[ai-1]);
                source.Add('a');
                ai--;
            } else {
                words.Add(WordReplacement(aWords[ai-1],bWords[bi-1],mode));
                source.Add('c');
                ai--; bi--;
            }
        }
        while (bi > 0) {
            words.Add(bWords[bi-1]);
            source.Add('b');
            bi--;
        }
        while (ai > 0) {
            words.Add(aWords[ai-1]);
            source.Add('a');
            ai--;
        }
        // Now reverse
        words.Reverse();
        source.Reverse();
        // Add parentheses to destinguish the sources
        // We keep track of which source we are taking words from ('a', 'b', or a combination 'c')
        char prev = 'c';
        int astart = 0;
        StringBuilder builder = new();
        if (mode == TextMode.Normal || mode == TextMode.WikiFormat) {
            for (int i = 0; i < words.Count; i++) {
                if (i > 0) builder.Append(' ');
                if (source[i] == 'a' && prev != 'a') astart = i;
                if (source[i] != 'b' && prev == 'b') builder.Append(") ");
                if (source[i] == 'b' && prev != 'b') builder.Append('(');
                if (source[i] == 'c' && prev == 'a') {
                    // a deletion not followed by an insertion. For example "Exhaust. (not Exhaust.)".
                    builder.Append("(not");
                    for (int j = astart ; j < i ; j++) {
                        builder.Append(' ');
                        builder.Append(words[j]);
                    }
                    builder.Append(")");
                }
                prev = source[i];
                // is this a keyword?
                builder.Append(words[i]);
            }
            if (prev == 'b') builder.Append(')');
            if (prev == 'a') {
                builder.Append(" (not");
                for (int j = astart ; j < words.Count ; j++) {
                    builder.Append(' ');
                    builder.Append(words[j]);
                }
                builder.Append(')');
            }
        } else {
            for (int i = 0 ; i < words.Count ; i++) {
                if      (source[i] == 'c' && prev == 'a') builder.Append("|] ");
                else if (source[i] == 'c' && prev == 'b') builder.Append("] ");
                else if (source[i] == 'b' && prev == 'a') builder.Append("|");
                else if (source[i] == 'b' && prev == 'c') { if (i > 0) builder.Append("| "); else builder.Append("|"); }
                else if (source[i] == 'a' && prev == 'c') { if (i > 0) builder.Append(" ["); else builder.Append("["); }
                else if (source[i] == 'a' && prev == 'b') builder.Append("] [");
                else if (i > 0) builder.Append(" ");
                prev = source[i];
                // is this a keyword?
                builder.Append(words[i]);
            }
            if (prev == 'b') builder.Append("]");
            if (prev == 'a') builder.Append("|");
        }
        // Join and remove unnecesary spaces
        String replace = builder.ToString().Replace(" .", ".").Replace(" ,", ",").Replace(" \n ","\n");
        if (mode == TextMode.WikiData) {
            return replace.Replace(" ]","]").Replace("[ ","[");
        } else {
            return replace.Replace(" )",")").Replace("( ","(");
        }
    }

    private string ProcessCombinedDescription(String description, TextMode textMode) {
        description = description.Replace(" \n] ", "\n]").Replace(" [\n ", "[\n").Replace("\n", "\\n");
        do {
            int start = description.IndexOf('[');
            if (description.Length > start + 8 && description[start + 1] == '#' && description[start + 8] == ']') {
                String code = description.Substring(start + 1, start + 8);

                description = (start > 0 ? description[..start] : "") + (textMode == TextMode.Normal ? "<span style=\"color:" + code + "\">" : "") + description[(start + 9)..];

                if (textMode == TextMode.Normal) {
                    int braces = description.IndexOf("[]");
                    int nextSpace = description.IndexOf(' ', start + 28);
                    if (nextSpace >= 0 && nextSpace < braces) {
                        description = description[..nextSpace] + "</span>" + description[nextSpace..];
                    } else if (braces >= 0) {
                        description = description.ReplaceFirst("\\[]", "</span>");
                    } else {
                        description += "</span>";
                    }
                } else {
                    description = description.ReplaceFirst("\\[]", "");
                }
                continue;
            }
        } while (false);

        return description;
    }

    public ViewportManager.DrawRequest[] ExportImg() {
        bool exportBeta = Upgrades == 0 && model.MaxUpgradeLevel == 0;
        return exportBeta ? [Request(), Request(true)] : [Request()];

        ViewportManager.DrawRequest Request(bool forceBeta = false) => new(ImgSize, $"{(forceBeta ? "beta-" : "")}card-images/{(Upgrades == 0 ? ID : $"{ID}Plus{Upgrades}")}", null, drawer => {
            NCard card = CardScene.Instantiate<NCard>();
            drawer.AddChild(card);
            card.Scale = Vector2.One * 2f;
            card.Modulate = Colors.White;
            card.Position = (Vector2)ImgSize / 2f;
            card.Model = model;
            card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
            if ((Upgrades > 0 || forceBeta) && model.HasBetaPortrait)
                card.GetNode<TextureRect>(model.Rarity == CardRarity.Ancient ? "%AncientPortrait" : "%Portrait").Texture = ResourceLoader.Load<Texture2D>(model.BetaPortraitPath, null, ResourceLoader.CacheMode.Reuse);
        });
    }

    public static List<CardExport> FindAll() => [..ModelDb.AllCards.Where(m => m.ShouldShowInCardLibrary).SelectMany(m => Enumerable.Range(0, m.MaxUpgradeLevel + 1).Select(u => new CardExport(m, u)))];
}