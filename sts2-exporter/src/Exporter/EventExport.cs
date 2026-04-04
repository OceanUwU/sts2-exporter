using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace STS2Export.Exporter;

public class EventExport : ItemExport, IImageExport {
    private static readonly Vector2I ImgSize = new(1920, 1080);

    private readonly EventModel model;

    public EventExport(EventModel model) {
        this.model = model.ToMutable();
        Assembly = model.GetType().Assembly;
    }

    [JsonInclude][JsonPropertyName("id")]
    private string ID => model.Id.Entry;
    [JsonInclude][JsonPropertyName("name")]
    private string Name => model.Title.GetFormattedText();
    //[JsonInclude][JsonPropertyName("acts")]
    //private string[] Acts => model.act
    [JsonInclude][JsonPropertyName("description")]
    private string Description => StripBBCodeTags(model.InitialDescription.GetFormattedText(), "colorless");
    [JsonInclude][JsonPropertyName("options")]
    private string[] Options => [..model.GameInfoOptions.Reverse().Select(loc => StripBBCodeTags(loc.GetFormattedText(), "colorless"))];
    
    public ViewportManager.DrawRequest[] ExportImg(ExportConfig config) => []; /*{
        if (model.LayoutType == MegaCrit.Sts2.Core.Events.EventLayoutType.Combat) return [];
        var scene = model.CreateScene();
        if (scene == null) return [];
        return [new(ImgSize, $"events/{ID}", null, drawer => {
            var visuals = scene.Instantiate<Control>();
            drawer.AddChild((Node)visuals);
            //scene.Position = (Vector2)ImgSize / 2f - potion.Size / 2f;
        })];
    }*/

    public static List<EventExport> FindAll() => [..ModelDb.AllEvents.Select(m => new EventExport(m))];
}