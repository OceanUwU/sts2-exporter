using Godot;
using STS2Export.Exporter;

namespace STS2Export;

public partial class ExporterScreen : Control {
    private ColorRect bg = new() { Color = Colors.DarkGreen };
    private Label testLabel = new() { Text = "StS2 Exporter", HorizontalAlignment = HorizontalAlignment.Center };
    private Label statusLabel = new() { Text = "", HorizontalAlignment = HorizontalAlignment.Center };
    private Button closeButton = new() { Text = "Close" };
    private Button openButton = new() { Text = "Open Folder" };
    private Button deleteButton = new() { Text = "Delete Folder" };
    private Button exportButton = new() { Text = "Export!" };
    private CheckBox exportImages = new() { Text = "Export images?", ButtonPressed = true };
    private CheckBox exportBasegame = new() { Text = "Export items from basegame?", ButtonPressed = true };
    private VBoxContainer vBox = new();

    private ExportBatch exporter;

    private bool closing = false;

    public override void _Ready() {
        base._Ready();
        Modulate = Modulate with { A = 0f };
        SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(closeButton);
        closeButton.SetAnchorsPreset(LayoutPreset.CenterTop);
        AddChild(vBox);
        vBox.SetAnchorsPreset(LayoutPreset.Center);
        vBox.AddChild(testLabel);
        vBox.AddChild(exportImages);
        vBox.AddChild(exportBasegame);
        vBox.AddChild(openButton);
        vBox.AddChild(deleteButton);
        vBox.AddChild(exportButton);
        vBox.AddChild(statusLabel);
        closeButton.Connect(Button.SignalName.Pressed, Callable.From(Close));
        openButton.Connect(Button.SignalName.Pressed, Callable.From(OpenDir));
        deleteButton.Connect(Button.SignalName.Pressed, Callable.From(DeleteDir));
        exportButton.Connect(Button.SignalName.Pressed, Callable.From(Export));
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1f, 0.25f);
    }

    public override void _Process(double delta) {
        Size = GetParent<Control>().Size;
        UpdateExportingProgress();
    }

    private void UpdateExportingProgress()  {
        if (exporter == null) return;
        if (exporter.ImagesExported >= exporter.NumImagesToExport) {
            exporter = null;
            deleteButton.Disabled = false;
            exportButton.Disabled = false;
            closeButton.Disabled = false;
            ShowStatus($"Done! Exported {exporter.NumImagesToExport} images in total.");
            return;
        }
        ShowStatus($"{exporter.ImagesExported}/{exporter.NumImagesToExport} images exported");
    }

    private void Close() {
        if (closing) return;
        closing = true;
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.25f);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    private void OpenDir() => ExportBatch.OpenDir();
    private void DeleteDir() {
        ExportBatch.DeleteDir();
        ClearStatus();
    }

    private void Export() {
        if (ExportBatch.DirExists()) {
            ShowError("Export directory already exists! You must delete it first.");
            return;
        }
        deleteButton.Disabled = true;
        exportButton.Disabled = true;
        closeButton.Disabled = true;
        exporter = new();
        exporter.Run(exportImages.ButtonPressed, exportBasegame.ButtonPressed);
        if (exporter.NumImagesToExport == 0) {
            exporter = null;
            deleteButton.Disabled = false;
            exportButton.Disabled = false;
            closeButton.Disabled = false;
            ShowStatus($"Done!");
        }
    }

    private void ClearStatus() => ShowStatus("");
    private void ShowError(string text) => ShowStatus(text, Colors.Red);
    private void ShowStatus(string text, Color? color = null) {
        statusLabel.Text = text;
        statusLabel.Modulate = color ?? Colors.Cyan;
    }
}