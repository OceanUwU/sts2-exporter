using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace STS2Export.Exporter;

public class ExportBatch {
    private const string BaseDir = "./export";

    private readonly Dictionary<string, ModExport> mods = [];
    private readonly ItemList items = new();

    public void Run() {
        ClearDir();
        FindMods();
        FindItems();
        AssignItemsToMods();
        ExportMods();
        ExportAllData();
        Finish();
    }

    private void ClearDir() {
        DirAccess.RemoveAbsolute(BaseDir);
        DirAccess.MakeDirRecursiveAbsolute(BaseDir);
    }

    private void FindMods() {
        var mod = new ModExport();
        mods.Add("test", new ModExport());
        //TODO: find other mods
    }

    private void FindItems() => items.FindAll();

    private void AssignItemsToMods() {
        foreach (var item in items.All())
            mods["test"].AddItem(item);
    }

    private void ExportMods() {
        foreach (var (_, mod) in mods)
            mod.Export(BaseDir);
    }

    private void ExportAllData() {
        FileAccess file = FileAccess.Open($"{BaseDir}/items.json", FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(items, new JsonSerializerOptions() { WriteIndented = true }));
        file.Close();
    }

    private void Finish() {
        GD.Print("Export finished!");
        OS.ShellOpen(BaseDir);
    }
}