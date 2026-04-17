using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using BaseLib.Utils.Patching;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes;

namespace STS2Export.Exporter;

public class KeywordExport: ItemExport, IImageExport {    
    [JsonInclude][JsonPropertyName("id")]
    public string ID;
    [JsonInclude][JsonPropertyName("name")]
    public string Title;
    [JsonInclude][JsonPropertyName("description")]
    public string Description;
    [JsonInclude][JsonPropertyName("type")]
    public string Type = "Keyword";
    private Func<IHoverTip> func;
    private DynamicVar dynamicVar;
    [JsonIgnore]
    private readonly Texture2D icon;
    [JsonInclude][JsonPropertyName("hasIcon")]
    public bool HasIcon => icon is not null;

    public KeywordExport(HoverTip hoverTip) {
        Assembly = hoverTip.GetType().Assembly;
        Title = hoverTip.Title;
        Description = StripBBCodeTags(hoverTip.Description.Replace("98765", "N"), "colorless");;
        ID = hoverTip.Id;
        icon = hoverTip.Icon;
    }

    public KeywordExport(HoverTip hoverTip, object obj) : this(hoverTip, obj.GetType()) {}
    public KeywordExport(HoverTip hoverTip, Type type) : this(hoverTip) {
        Assembly = type.Assembly;
    }


    public KeywordExport(CardKeyword cardKeyword) : this((HoverTip)HoverTipFactory.FromKeyword(cardKeyword), typeof(CardKeyword)) {}
    public KeywordExport(CardKeyword cardKeyword, Type type) : this((HoverTip)HoverTipFactory.FromKeyword(cardKeyword), type) {}

    public KeywordExport(PowerModel power) : this(power.DumbHoverTip, power) { 
        icon = power.BigIcon;
        Type = power.Type == MegaCrit.Sts2.Core.Entities.Powers.PowerType.None ? "Power" : power.Type.ToString();
    }

    public KeywordExport(OrbModel orb) : this(orb.DumbHoverTip, orb) { 
        Type = "Orb";
    }

    public static KeywordExport FromDynamicVar(Type type) => new((HoverTip)DynamicVarExtensions.DynamicVarTips.Get((DynamicVar)Activator.CreateInstance(type, 98765m))(), type);

    public KeywordExport(Func<IHoverTip> func, DynamicVar dynamicVar)
    {
        this.func = func;
        this.dynamicVar = dynamicVar;
    }

    public ViewportManager.DrawRequest[] ExportImg(ExportConfig config) => HasIcon ? [new((Vector2I)icon.GetSize(), $"keywords/{ID}", node => node.DrawTexture(icon, Vector2.Zero))] : [];
    
    public static List<KeywordExport> FindAll() => [
        ..Enum.GetValues<CardKeyword>().Select(static e => new KeywordExport(e)),
        ..GetCustomEnums.GetEnumsOfType<CardKeyword>().Select(static e => new KeywordExport((CardKeyword)(int)e.key, e.declaringType)),
        ..Enum.GetValues<StaticHoverTip>().Where(static e => !e.ToString().EndsWith("Dynamic")).Select(static e => new KeywordExport((HoverTip)HoverTipFactory.Static(e))),
        ..GetCustomEnums.GetEnumsOfType<StaticHoverTip>().Select(static e => new KeywordExport((HoverTip)HoverTipFactory.Static((StaticHoverTip)(int)e.key), e.declaringType)),
        ..ModelDb.AllPowers.Select(static p => new KeywordExport(p)),
        ..OrbModel._validOrbs.Select(static o => new KeywordExport(ModelDb.GetById<OrbModel>(o))),
        ..CustomOrbModel.RegisteredOrbs.Select(static o => new KeywordExport(o)),
        ..ModManager.Mods.SelectMany(static m => m.assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(DynamicVar))).Select(static v => FromDynamicVar(v))),
    ];
}

[HarmonyPatch(typeof(GenEnumValues), nameof(GenEnumValues.FindAndGenerate))]
public class GetCustomEnums {
    private static readonly Dictionary<Type, List<(object key, Type declaringType)>> Enums = [];

    public static List<(object key, Type declaringType)> GetEnumsOfType<T>() => Enums.TryGetValue(typeof(T), out var list) ? list : [];

    static void Store(FieldInfo field, object key) {
        if (field.DeclaringType == null) return;
        if (!Enums.TryGetValue(field.FieldType, out var list)) {
            list = [];
            Enums[field.FieldType] = list;
        }
        list.Add((key, field.DeclaringType));
    }

    static List<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => new InstructionPatcher(instructions)
        .Match(new InstructionMatcher()
            .call(AccessTools.Method(typeof(CustomEnums), nameof(CustomEnums.GenerateKey)))
            .stloc_s(11)
        )
        .Insert([
            CodeInstruction.LoadLocal(8),
            CodeInstruction.LoadLocal(11),
            CodeInstruction.Call(typeof(GetCustomEnums), nameof(Store)),
        ]);
}