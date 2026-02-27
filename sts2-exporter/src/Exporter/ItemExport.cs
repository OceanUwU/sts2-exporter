using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace STS2Export.Exporter;

public abstract partial class ItemExport {
    public ModExport Mod;

    [GeneratedRegex("\\[img.*?\\/img\\]")]
    private static partial Regex BBCodeImgRegex();
    private static readonly Regex BBCodeImgSubstitutor = BBCodeImgRegex();
    [GeneratedRegex("\\[.*?\\]")]
    private static partial Regex BBCodeRegex();
    private static readonly Regex BBCodeSubstitutor = BBCodeRegex();
    protected static string StripBBCodeTags(string s) => Regex.Unescape(BBCodeSubstitutor.Replace(BBCodeImgSubstitutor.Replace(s, static m =>{
        var path = m.Value["[img]".Length..(m.Value.Length-"[/img]".Length)];
        return $"{{img={path[(path.LastIndexOf('/')+1)..path.LastIndexOf('.')]}}}";
    }), static m => ""));
}

public interface IImageExport {   
    public abstract string ImgPath { get; }
    public abstract string ImgFilename { get; }
    public abstract ViewportManager.DrawRequest ExportImg();
}