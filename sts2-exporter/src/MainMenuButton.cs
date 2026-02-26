using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using STS2Export.Exporter;

namespace STS2Export;

[HarmonyPatch(typeof(NMainMenu), "_Ready")]
public class MainMenuButton {
    private static NMainMenuTextButton button;

    public static void Prefix(NMainMenu __instance) {
        button = (NMainMenuTextButton)__instance.GetNode<NMainMenuTextButton>("MainMenuTextButtons/SettingsButton").Duplicate();
    }

    public static void Postfix(NMainMenu __instance) {
        __instance.GetNode<NMainMenuTextButton>("MainMenuTextButtons/SettingsButton").AddSibling(button);
        ViewportManager.AddToTree(button.GetTree());
        button.GetChild<MegaLabel>(0).Text = "Exporter";
        button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnPress));
    }

    private static void OnPress(NButton button) {
        new ExportBatch().Run();
    }
}