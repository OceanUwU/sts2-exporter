using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2Export.Exporter;

public class CreatureExport : ItemExport, IImageExport {
    public CreatureExport(MonsterModel monsterModel) {
        ID = monsterModel.Id.Entry;
        name = monsterModel.Title.GetFormattedText();
        minHP = monsterModel.MinInitialHp;
        maxHP = monsterModel.MaxInitialHp;
        this.monsterModel = monsterModel;
    }

    public CreatureExport(CharacterModel characterModel) {
        ID = characterModel.Id.Entry;
        name = characterModel.Title.GetFormattedText();
        type = "Player";
        //minHP = characterModel.StartingHp;
        maxHP = characterModel.StartingHp;
        this.characterModel = characterModel;
    }

    private readonly MonsterModel monsterModel;
    private readonly CharacterModel characterModel;

    [JsonInclude][JsonPropertyName("id")]
    private readonly string ID;
    [JsonInclude][JsonPropertyName("name")]
    private readonly string name;
    [JsonInclude][JsonPropertyName("type")]
    private readonly string type;
    [JsonInclude][JsonPropertyName("minHP")]
    private readonly int minHP;
    [JsonInclude][JsonPropertyName("maxHP")]
    private readonly int maxHP;
    [JsonInclude][JsonPropertyName("minHPA")]
    private readonly int minHPAscension;
    [JsonInclude][JsonPropertyName("maxHPA")]
    private readonly int maxHPAscension;

    public static List<CreatureExport> FindAll() => [
        ..ModelDb.AllCharacters.Select(m => new CreatureExport(m)),
        ..ModelDb.Monsters.OrderBy(m => m.Id.Entry).Select(m => new CreatureExport(m)),
    ];

    [JsonIgnore]
    public string ImgPath => "creatures";
    [JsonIgnore]
    public string ImgFilename => ID;
    public ViewportManager.DrawRequest ExportImg() {
        NCreatureVisuals visuals = null;
        if (monsterModel != null)
            visuals = monsterModel.CreateVisuals();
        else if (characterModel != null)
            visuals = characterModel.CreateVisuals();
        Control bounds = visuals.GetNode<Control>("%Bounds");
        return new((Vector2I)bounds.Size, null, drawer => {
            drawer.AddChildSafely(visuals);
            if (visuals != null && visuals.HasSpineAnimation) {
                var animController = visuals.SpineBody;
                var animState = visuals.SpineBody.GetAnimationState();
                if (monsterModel != null) {
                    monsterModel.GenerateAnimator(animController);
                    visuals.SetUpSkin(monsterModel);
                }
                else if (characterModel != null) {
                    characterModel.GenerateAnimator(animController);
                }
                animState.SetAnimation("idle_loop");
            }
            visuals.Position = bounds.Size / 2f + new Vector2(0f, bounds.Size.Y * 0.5f);
            visuals.Show();
            visuals.Modulate = Colors.White;
        });
    }
}