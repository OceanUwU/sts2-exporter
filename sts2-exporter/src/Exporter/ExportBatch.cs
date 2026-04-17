using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using Scriban;
using Scriban.Runtime;

namespace STS2Export.Exporter;

public class ExportBatch {
    private const string BaseDir = "./export";
    private static readonly string TexDumpDir = BaseDir.PathJoin("texDump");

    private readonly Dictionary<string, ModExport> mods = [];
    private readonly ItemList items = new();

    public int ImagesExported = 0;
    public int NumImagesToExport = 0;

    public void Run(ExportConfig config) {
        DirAccess.MakeDirRecursiveAbsolute(BaseDir);
        FindMods();
        FindItems();
        AssignItemsToMods();
        if (!config.ExportBaseGame)
            DiscardBasegame();
        ExportMods();
        ExportAllData();
        if (config.DoTexDump)
            DumpTextures();
        if (config.ExportImages)
            ExportImages(config);
        Finish();
    }

    private void FindMods() {
        ModExport basegame = new(null);
        mods.Add(basegame.ID, basegame);
        foreach (var mod in ModManager.Mods) {
            ModExport export = new(mod);
            mods.Add(export.ID, export);
        }
    }

    private void FindItems() => items.FindAll();

    private void AssignItemsToMods() {
        foreach (var item in items.All())
            mods.First(mod => mod.Value.Assembly == item.Assembly).Value.AddItem(item);
    }

    private void DiscardBasegame() {
        items.RemoveIf(static i => i.Mod.IsBasegame);
        mods.Remove("slay-the-spire-2");
    }

    private void ExportMods() {
        foreach (var (_, mod) in mods.Where(m => m.Value.Items.All().Any()))
            mod.Export(BaseDir);
    }

    private void ExportAllData() {
        FileAccess file = FileAccess.Open($"{BaseDir}/items.json", FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(items, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        file.Close();
        ExportTemplate("wiki-card-data.lua", new{ cards = items.Cards }, BaseDir);
        ExportTemplate("wiki-relic-data.lua", new{ relics = items.Relics }, BaseDir);
        ExportTemplate("wiki-potion-data.lua", new{ potions = items.Potions }, BaseDir);
    }

    private void ExportTemplate(string template, object data, string path) {
        TemplateContext context = new() {
            LoopLimit = 0,
        };
        ScriptObject scriptObject = [];
        scriptObject.Import(data);
        context.PushGlobal(scriptObject);
        var file = FileAccess.Open($"res://sts2-exporter/templates/{template}.scriban", FileAccess.ModeFlags.Read);
        var templateText = file.GetAsText();
        file.Close();
        var output = Template.Parse(templateText).Render(context);
        file = FileAccess.Open(path.PathJoin(template), FileAccess.ModeFlags.Write);
        file.StoreString(output);
        file.Close();
    }

    private void ExportImages(ExportConfig config) {
        NGame._window.Size = new(1920, 1080);
        foreach (var item in items.All())
            if (item is IImageExport imageExport)
                foreach (var request in imageExport.ExportImg(config)) {
                    NumImagesToExport++;
                    ViewportManager.RequestDraw(request).ContinueWith(task => {
                        Image img = task.Result;
                        if (img == null) {
                            NumImagesToExport--;
                            return;
                        }
                        string path = $"{BaseDir}/{item.Mod.ID}/{request.Path}.png";
                        DirAccess.MakeDirRecursiveAbsolute(path[..path.LastIndexOf('/')]);
                        img.SavePng(path);
                        ImagesExported++;
                    });
                }
    }

    public static bool OpenDir() {
        if (!DirAccess.DirExistsAbsolute(BaseDir)) return false;
        OS.ShellOpen(BaseDir);
        return true;
    }

    public static bool DirExists() => DirAccess.DirExistsAbsolute(BaseDir);

    public static void DeleteDir() {
        DeleteRecursive(BaseDir);

        static void DeleteRecursive(string dir) {
            foreach (string d in DirAccess.GetDirectoriesAt(dir))
                DeleteRecursive(dir.PathJoin(d));
            foreach (string f in DirAccess.GetFilesAt(dir))
                DirAccess.RemoveAbsolute(dir.PathJoin(f));
            DirAccess.RemoveAbsolute(dir);
        }
    }

    private void Finish() {
        GD.Print("Export finished!");
    }

    public void DumpTextures() {
        DumpDir("res://");
        AtlasManager.LoadAllAtlases();
        foreach (string atlasName in AtlasManager._knownAtlases) {
            string atlasPath = AtlasResourceLoader._atlasBasePath.PathJoin(atlasName);
            string savePath = atlasPath.Replace("res:/", TexDumpDir);
            var atlas = AtlasManager._atlases[atlasName];
            foreach ((string spriteName, _) in atlas.SpriteMap) {
                var texture = AtlasManager.GetSprite(atlasName, spriteName);
                var path = savePath.PathJoin($"{spriteName}.png");
                DirAccess.MakeDirRecursiveAbsolute(path[..path.LastIndexOf('/')]);
                ViewportManager.RequestDraw(new((Vector2I)texture.GetSize(), action: drawer => drawer.DrawTexture(texture, Vector2.Zero)))
                    .ContinueWith(task => task.Result.SavePng(path));
            }
        }
        

        static void DumpDir(string path) {
            string exportPath = path.Replace("res:/", TexDumpDir);
            var files = ResourceLoader.ListDirectory(path);
            foreach (var file in files) {
                if (file.EndsWith('/')) {
                    DumpDir(path.PathJoin(file));
                } else if (file.EndsWith(".png")) {
                    DirAccess.MakeDirRecursiveAbsolute(exportPath);
                    var filePath = path.PathJoin(file);
                    var resource = ResourceLoader.Load(filePath);
                    if (resource is Texture2D texture)
                        texture.GetImage().SavePng(exportPath.PathJoin(file));
                    else if (resource is Image image)
                        image.SavePng(exportPath.PathJoin(file));
                }
            }
        }
    }
}