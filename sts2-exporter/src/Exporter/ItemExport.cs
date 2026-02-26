using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace STS2Export.Exporter;

public abstract class ItemExport {
    public ModExport Mod;
}

public interface IImageExport {   
    public abstract string ImgPath { get; }
    public abstract List<(string filename, ViewportManager.DrawRequest request)> ExportImgs();
}