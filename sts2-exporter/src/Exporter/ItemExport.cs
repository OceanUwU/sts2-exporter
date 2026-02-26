using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace STS2Export.Exporter;

public abstract partial class ItemExport {
    public ModExport Mod;

    [GeneratedRegex("\\[.*?\\]")]
    private static partial Regex BBCodeRegex();
    private static readonly Regex BBCodeSubstitutor = BBCodeRegex();
    protected static string StripBBCodeTags(string s) => Regex.Unescape(BBCodeSubstitutor.Replace(s, static m => ""));
}

public interface IImageExport {   
    public abstract string ImgPath { get; }
    public abstract string ImgFilename { get; }
    public abstract ViewportManager.DrawRequest ExportImg();
}