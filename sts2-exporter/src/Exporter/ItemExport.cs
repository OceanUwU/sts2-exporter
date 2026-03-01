using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace STS2Export.Exporter;

public abstract partial class ItemExport {
    [JsonInclude][JsonPropertyName("v")]
    private readonly byte slayTheSpireVersion = 2;
    [JsonInclude][JsonPropertyName("mod")][JsonConverter(typeof(ModPropertyConverter))]
    public ModExport Mod;

    [GeneratedRegex("\\[img.*?\\/img\\]")]
    private static partial Regex BBCodeImgRegex();
    private static readonly Regex BBCodeImgSubstitutor = BBCodeImgRegex();
    [GeneratedRegex("\\[.*?\\]")]
    private static partial Regex BBCodeRegex();
    private static readonly Regex BBCodeSubstitutor = BBCodeRegex();
    //protected static string StripBBCodeTags(string s) => Regex.Unescape(BBCodeSubstitutor.Replace(BBCodeImgSubstitutor.Replace(s, static m =>{
    //    var path = m.Value["[img]".Length..(m.Value.Length-"[/img]".Length)];
    //    return $"{{img={path[(path.LastIndexOf('/')+1)..path.LastIndexOf('.')]}}}";
    //}), static m => ""));

    protected static string StripBBCodeTags(string s, string prefix) => BBCodeSubstitutor.Replace(
        s
            .Replace( $"[img]res://images/packed/sprite_fonts/{prefix}_energy_icon.png[/img]", "{E}")
            .Replace( $"[img]res://images/packed/sprite_fonts/star_icon.png[/img]", "{STAR}"),
        static m => ""
    ).Replace("{E}", "[E]").Replace("{STAR}", "[STAR]");

    private class ModPropertyConverter : JsonConverter<ModExport> {
        public override ModExport Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ModExport value, JsonSerializerOptions options) {
            writer.WriteStringValue(value.Name);
        }
    }
}

public interface IImageExport {   
    public abstract ViewportManager.DrawRequest[] ExportImg();
}