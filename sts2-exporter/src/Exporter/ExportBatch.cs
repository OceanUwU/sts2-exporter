using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace STS2Export.Exporter;

public class ExportBatch {
    private const string BaseDir = "./export";

    private readonly Dictionary<string, ModExport> mods = [];
    private readonly ItemList items = new();

    public int ImagesExported = 0;
    public int NumImagesToExport = 0;

    public void Run(bool images, bool basegame) {
        DirAccess.MakeDirRecursiveAbsolute(BaseDir);
        FindMods();
        FindItems();
        AssignItemsToMods();
        if (!basegame)
            DiscardBasegame();
        ExportMods();
        ExportAllData();
        if (images)
            ExportImages();
        Finish();
    }

    private void FindMods() {
        var mod = new ModExport();
        mods.Add(mod.ID, mod);
    }

    private void FindItems() => items.FindAll();

    private void AssignItemsToMods() {
        foreach (var item in items.All())
            mods["slay-the-spire-2"].AddItem(item);
    }

    private void DiscardBasegame() {
        items.RemoveIf(static i => i.Mod.IsBasegame);
        mods.Remove("slay-the-spire-2");
    }

    private void ExportMods() {
        foreach (var (_, mod) in mods)
            mod.Export(BaseDir);
    }

    private void ExportAllData() {
        FileAccess file = FileAccess.Open($"{BaseDir}/items.json", FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(items, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        file.Close();
    }

    private void ExportImages() {
        foreach (var item in items.All())
            if (item is IImageExport imageExport)
                foreach (var request in imageExport.ExportImg()) {
                    NumImagesToExport++;
                    ViewportManager.RequestDraw(request).ContinueWith(task => {
                        Image img = task.Result;
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
}